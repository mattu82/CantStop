// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Math;

namespace CantStop;

internal enum GameEventType { Default, Start, Roll, Stop, End }

internal class GameEvent
{
    [JsonInclude] internal DateTime timestamp;
    [JsonInclude] internal GameEventType type = GameEventType.Default;
    [JsonInclude] internal int player;
    [JsonInclude] internal List<int> dice = [];
    [JsonInclude] internal List<int> selected = [];
}

internal class Stats
{
    internal int rolls;
    internal int failedrolls;
    internal int maxbustcount;
    internal int distance;
    internal int maxstreak;
    internal TimeSpan playtime;

    internal static string BustcountToOdds(int bustcount)
    {
        return bustcount switch
        {
            0 => "100%",
            < 648 => $"{(1296 - bustcount) / (double)bustcount:0.00} : 1",
            _ => $"1 : {(double)bustcount / (1296 - bustcount):0.00}"
        };
    }
}

internal class GameLog
{
    [JsonInclude] internal string summary;
    [JsonInclude] internal bool complete;
    [JsonInclude] internal Config config;
    [JsonInclude] internal int[] shuffleorder;
    [JsonInclude] internal DateTime start;
    [JsonInclude] internal List<GameEvent> log;

    public GameLog(int[] shuffleorder)
    {
        start = DateTime.Now;
        summary = start.ToString("MMM dd h:mmtt ");
        complete = false;
        config = Program.Cfg;
        this.shuffleorder = new int[shuffleorder.Length];
        for (var i = 0; i < shuffleorder.Length; i++)
        {
            this.shuffleorder[i] = shuffleorder[i];
            summary += config.playernames[shuffleorder[i]];
            if (i !=  shuffleorder.Length - 1) summary += ",";
        }
        summary += " (in progress)";
        log = [];
    }

    internal static GameLog? FromFile(string filename)
    {
        using var fin = new StreamReader(filename);
        return JsonSerializer.Deserialize<GameLog>(fin.ReadToEnd());
    }

    internal void Log(GameEventType t, int p, List<int> d, List<int> o)
    {
        log.Add(new GameEvent { timestamp = DateTime.Now, type = t, player = p, dice = d, selected = o });
        Dump();
    }

    internal void Dump()
    {
        var logdir = Program.LogPath + @"\GameLogs"; 
        Directory.CreateDirectory(logdir);


        using var fout = new StreamWriter(logdir + @"\" + start.ToString("yyyyMMddHHmmss") + ".json");
        fout.Write(JsonSerializer.Serialize(this));

            
    }
}

internal class GameLogSummary
{
    internal string file = string.Empty;
    internal string summary = string.Empty;
    internal bool complete;

    internal static GameLogSummary? FromFile(string filename)
    {
        var log = GameLog.FromFile(filename);

        return log is null ? null : new GameLogSummary { file = filename, summary = log.summary, complete = log.complete };
    }
}

internal class GameEngine
{
    private int[] _markers = [];
    private readonly Random _rnd = new();
    private int _bustcount;

    internal int[] nextmarkers = [];
    internal int[] scored = [];
    internal List<int>[][] board = [];
    internal List<int> dice = [];
    internal bool showroll;
    internal bool showstop;
    internal int player;
    internal int optionselected;
    internal List<List<int>> options = [];
    internal bool startofturn;
    internal bool gameover;
    internal bool triggerwin;
    internal bool triggerfail;
    internal bool triggerscore;
    internal bool triggerroll;

    private int[] _shuffleorder = [];

    internal GameLog gamelog = new([]);

    internal bool gameInProgress;

    internal GameLog? replaylog;

    private DateTime _lastReplayEvent;
    private TimeSpan _stepTime;
    private double _gevi;

    internal Stats[] playerstats = [];
    private int _rollcount;
    private DateTime _turnstart;

    internal List<GameLogSummary> replaylogs = [];
    internal List<GameLogSummary> continuelogs = [];

    internal Config? cfgbk;
    internal bool showstats = true;

    internal GameEngine() {
        UpdateLogs();
    }

    internal void UpdateLogs()
    {
        replaylogs = [];
        continuelogs = [];

        if (!Directory.Exists(Program.LogPath + @"\GameLogs")) return;
        foreach (var log in Directory.GetFiles(Program.LogPath + @"\GameLogs", "*.json").OrderByDescending(File.GetLastWriteTime))
        {
            var tmp = GameLogSummary.FromFile(log);

            if (tmp is null) continue;
            if (tmp.complete) replaylogs.Add(tmp); else continuelogs.Add(tmp);
        }
    }

    private void ShuffleOrder()
    {
        for (var i = 0; i < Program.Cfg.players; i++)
            _shuffleorder[i] = i;

        if (!Program.Cfg.shuffle) return;
        {
            for (var i = 0; i < Program.Cfg.players - 1; i++)
            {
                var tmp = _shuffleorder[i];
                var tmp2 = _rnd.Next(i, Program.Cfg.players);
                _shuffleorder[i] = _shuffleorder[tmp2];
                _shuffleorder[tmp2] = tmp;
            }
        }
    }

    private void ProcessOption(int x, int y)
    {
        if (CanAdvance(x) && CanAdvance(y) && (3 - nextmarkers.Count(m => m != -1) != 1 || _markers[x] != -1 || _markers[y] != -1))
        {
            if (x == y && (_markers[x] == 2 * (6 - Abs(7 - x)) - 1 || board[x][2 * (6 - Abs(7 - x)) - 1].Contains(player)))
                options.AddIfUnique([x]);
            else
                options.AddIfUnique([Min(x, y), Max(x, y)]);
        }
        else
        {
            if (CanAdvance(x)) options.AddIfUnique([x]);
            if (CanAdvance(y)) options.AddIfUnique([y]);
        }
    }

    private bool CanAdvance(int x)
    {
        if (board[x][2 * (6 - Abs(7 - x))].Count != 0) return false;

        if (nextmarkers[x] == 2 * (6 - Abs(7 - x))) return false;

        return nextmarkers.Count(m => m != -1) < 3 || nextmarkers[x] != -1;
    }


    internal void RollClicked() { RollClicked(null); }

    private void RollClicked(GameEvent? gev)
    {
        if (!startofturn && _bustcount > playerstats[player].maxbustcount)
            playerstats[player].maxbustcount = _bustcount;

        startofturn = false;

        for (var i = 2; i <= 12; i++)
        {
            _markers[i] = nextmarkers[i];
        }

        RollDice(gev);
    }

    internal void StopClicked() { StopClicked(null); }

    private void StopClicked(GameEvent? gev)
    {
        if (gev is null)
        {
            Log(GameEventType.Stop);
            playerstats[player].playtime += DateTime.Now - _turnstart;
        }
        else
        {
            playerstats[player].playtime += gev.timestamp - _turnstart;
        }

        if (optionselected != -1)
        {
            if (_rollcount > playerstats[player].maxstreak)
                playerstats[player].maxstreak = _rollcount;

            for (var i = 2; i <= 12; i++)
            {
                if (nextmarkers[i] != -1)
                {
                    var lp = -1;

                    for (var j = 0; j < nextmarkers[i]; j++)
                    {
                        if (!board[i][j].Contains(player)) continue;
                        board[i][j].Remove(player);
                        lp = j;
                    }

                    board[i][nextmarkers[i]].Add(player);

                    playerstats[player].distance += nextmarkers[i] - lp;
                }

                if (nextmarkers[i] != 2 * (6 - Abs(7 - i))) continue;
                {
                    scored[i] = player;

                    for (var j = 0; j <= 2 * (6 - Abs(7 - i)); j++)
                        board[i][j] = [player];

                    if (scored.Count(x => x == player) >= 3)
                    {
                        triggerwin = true;
                        gameover = true;
                        showstop = false;
                        showroll = false;

                        if (gev is not null) continue;
                        Log(GameEventType.End);

                        var sb = new StringBuilder();
                        sb.Append(gamelog.start.ToString("MMM dd h:mmtt "));
                        int[] tmp = [0, 1, 2, 3];
                        var scoreorder = tmp.OrderByDescending(x => scored.Count(y => y==x)).ThenBy(x => x >= Program.Cfg.players ? int.MaxValue : _shuffleorder[x]).ToArray();
                        for (var p = 0; p < Program.Cfg.players; p++)
                        {
                            sb.Append(Program.Cfg.playernames[scoreorder[p]] + ":" + scored.Count(s => s == scoreorder[p]) + " ");
                        }
                        gamelog.summary = sb.ToString();
                        gamelog.complete = true;

                        gamelog.Dump();
                        gameInProgress = false;
                    }
                    else
                        triggerscore = true;
                }
            }
        }

        if (gameover) return;
        {
            var iop = -1;

            for (var i = 0; i < Program.Cfg.players; i++)
                if (_shuffleorder[i] == player) iop = i;

            player = _shuffleorder[(iop + 1) % Program.Cfg.players];

            _markers = [-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1];
            nextmarkers = [-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1];

            startofturn = true;
            showstop = false;
            showroll = true;
            _rollcount = 0;

            optionselected = -1;

            if (Program.Cfg.autoroll && gev is null)
                RollClicked();
        }
    }

    internal void OptionClicked(int option) { OptionClicked(option, null); }

    internal void OptionClicked(int option, GameEvent? gev)
    {
        optionselected = option;

        for (var i = 2; i <= 12; i++)
            nextmarkers[i] = _markers[i];

        foreach (var o in options[option])
        {


            if (nextmarkers[o] == -1)
            {
                var j = 2 * (6 - Abs(7 - o)) - 1;

                while (j >= 0 && !board[o][j].Contains(player))
                    j--;

                nextmarkers[o] = j + 1;
            }
            else
                nextmarkers[o]++;
        }

        _bustcount = 0;

        for (var d1 = 1; d1 <= 6; d1++)
        for (var d2 = 1; d2 <= 6; d2++)
        for (var d3 = 1; d3 <= 6; d3++)
        for (var d4 = 1; d4 <= 6; d4++)
        {
            if (!CanAdvance(d1 + d2)
                && !CanAdvance(d1 + d3)
                && !CanAdvance(d1 + d4)
                && !CanAdvance(d2 + d3)
                && !CanAdvance(d2 + d4)
                && !CanAdvance(d3 + d4)
               )
                _bustcount++;
        }

        showroll = true;
        showstop = Program.Cfg.alwaysshowstop || _bustcount != 0;

        if (Program.Cfg.autoroll && Program.Cfg.autooption && _bustcount == 0 && options.Count == 1 && gev is null)
            RollClicked();
    }

    private void RollDice(GameEvent? gev)
    {
        playerstats[player].rolls++;
        _rollcount++;

        if (gev is not null)
        {
            dice = gev.dice;

            if (_rollcount == 1) _turnstart = gev.timestamp;
        }
        else
        {
            dice = [_rnd.Next(1, 7), _rnd.Next(1, 7), _rnd.Next(1, 7), _rnd.Next(1, 7)];
            Log(GameEventType.Roll);
            _turnstart = DateTime.Now;
        }

        dice = [.. dice.OrderBy(x => x)];



        optionselected = -1;
        options = [];

        ProcessOption(dice[0] + dice[1], dice[2] + dice[3]);
        ProcessOption(dice[0] + dice[2], dice[1] + dice[3]);
        ProcessOption(dice[0] + dice[3], dice[1] + dice[2]);

        switch (options.Count)
        {
            case 0:
            {
                playerstats[player].failedrolls++;
                triggerfail = true;

                showstop = true;
                showroll = false;

                if (Program.Cfg.autostop && gev is null)
                    StopClicked();
                break;
            }
            case 1 when Program.Cfg.autooption && gev is null:
                triggerroll = true;
                OptionClicked(0);
                break;
            default:
                triggerroll = true;
                options = [.. options.OrderBy(x => x[0] * 12 + (x.Count > 1 ? x[1] : 0))];

                showroll = false;
                showstop = false;
                break;
        }

    }

    internal string GetOdds()
    {
        return "odds to continue: " + Stats.BustcountToOdds(_bustcount);
    }



    internal string GetScores()
    {
        var retval = new StringBuilder();

        for (var p = 0; p < Program.Cfg.players; p++)
            retval.Append(Program.Cfg.playernames[_shuffleorder[p]] + ":" + scored.Count(s => s == _shuffleorder[p]) + " ");

        return retval.ToString();
    }

    internal string OptionString(int o)
    {
        return options[o][0] + (options[o].Count > 1 ? "," + options[o][1] : "");
    }

    internal void NewGame()
    {
        _markers = [-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1];
        nextmarkers = [-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1];
        scored = [-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1];
        board = [[],[],
            [[],[],[]],
            [[],[],[],[],[]],
            [[],[],[],[],[],[],[]],
            [[],[],[],[],[],[],[],[],[]],
            [[],[],[],[],[],[],[],[],[],[],[]],
            [[],[],[],[],[],[],[],[],[],[],[],[],[]],
            [[],[],[],[],[],[],[],[],[],[],[]],
            [[],[],[],[],[],[],[],[],[]],
            [[],[],[],[],[],[],[]],
            [[],[],[],[],[]],
            [[],[],[]]
        ];
        showroll = true;
        showstop = false;
        optionselected = -1;
        options = [];
        startofturn = true;
        gameover = false;
        triggerwin = false;
        triggerfail = false;
        triggerscore = false;
        triggerroll = false;

        _shuffleorder = new int[Program.Cfg.players];
        playerstats = new Stats[Program.Cfg.players];
        for (var i = 0; i < Program.Cfg.players; i++)
            playerstats[i] = new Stats();

        _rollcount = 0;

        if (replaylog is not null) return;
        ShuffleOrder();
        player = _shuffleorder[0];

        gamelog = new GameLog(_shuffleorder);
        Log(GameEventType.Start);
        gameInProgress = true;

        if (Program.Cfg.autoroll)
            RollClicked();
    }

    private void Log(GameEventType t)
    {
        gamelog.Log(t, player, dice, optionselected == -1 ? [] : options[optionselected]);
    }

    internal void InitReplay(string file)
    {
        var gl = GameLog.FromFile(file);

        if (gl is not null)
            InitReplay(gl);
    }

    internal void InitReplay(GameLog gl)
    {
        if (Program.Cfg != gl.config)
        {
            cfgbk = Program.Cfg;
            Program.Cfg = gl.config;
        }
        replaylog = gl;

        NewGame();

        for (var i = 0; i < gl.shuffleorder.Length; i++)
            _shuffleorder[i] = gl.shuffleorder[i];
        player = _shuffleorder[0];

        _lastReplayEvent = DateTime.Now;
        _stepTime = new TimeSpan(30 * 10000000/(gl.log.Count*2));
        _gevi = 0;
    }

        
    internal void ReplayAll()
    {
        if (replaylog is null) return;

        while (_gevi < replaylog.log.Count)

        {
            ReplayEvent(replaylog.log[(int)Floor(_gevi)],1);
            ReplayEvent(replaylog.log[(int)Floor(_gevi)],2);
            _gevi++;
        }

        triggerroll = false;
        triggerwin = false;
        triggerscore = false;
        triggerfail = false;

        gamelog = replaylog;
        replaylog = null;

        if (!gameover) gameInProgress = true;
    }

    private void ReplayEvent(GameEvent gev, int step)
    {
        if (gev.type == GameEventType.End) return;

        switch (step)
        {
            case 1:
                optionselected = -1;
                for (var i = 0; i < options.Count; i++)
                    if (options[i].Count == gev.selected.Count
                        && options[i][0] == gev.selected[0]
                        && (gev.selected.Count == 1 || options[i][1] == gev.selected[1]))
                        OptionClicked(i, gev);
                break;
            case 2:
                switch (gev.type)
                {
                    case GameEventType.Roll:
                        RollClicked(gev);
                        break;
                    case GameEventType.Stop:
                        StopClicked(gev);
                        break;
                    case GameEventType.Default:
                    case GameEventType.Start:
                    case GameEventType.End:
                    default:
                        break;
                }
                break;
        }

        triggerroll = false;
    }




    internal bool ReplayTimerUp()
    {
        if (replaylog is null) return false;

        return DateTime.Now - _lastReplayEvent > _stepTime;
    }

    internal void StepReplay()
    {
        if (replaylog is null) return;

        if (_gevi >= replaylog.log.Count)
        {
            gamelog = replaylog;
            if (!gameover) gameInProgress = true;

            replaylog = null;
            return;
        }

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        ReplayEvent(replaylog.log[(int)Floor(_gevi)], _gevi == (int)_gevi ? 1 : 2 );

        _lastReplayEvent = DateTime.Now;
        _gevi+= .5;
    }


}

internal static class GameEngineExtensions
{
    internal static void AddIfUnique(this List<List<int>> thislist, List<int> item)
    {
        if (!thislist.Exists(x => x[0] == item[0] && (x.Count == 1 || x[1] == item[1])))
            thislist.Add(item);
    }

    internal static List<string> GetSummaries(this List<GameLogSummary> thisList)
    {
        return [.. thisList.Select(log => log.summary)];
    }

    internal static List<string> GetDeletes(this List<GameLogSummary> thisList)
    {
        var retval = new List<string>();
        for (var i = 0; i < thisList.Count; i++) { retval.Add("DELETE"); }
        return retval;
    }

}
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CantStop;

internal class Config
{
    internal bool dirty;
    [JsonInclude] internal string version = Program.Version;
    [JsonInclude] internal int players = 2;
    [JsonInclude] internal string[] playernames = ["Player 1", "Player 2", "Player 3", "Player 4"];
    [JsonInclude] internal string[] playercolor = ["#FF0000", "#FFFF00", "#00FF00", "#0000FF"];
    [JsonInclude] internal bool shuffle = true;
    [JsonInclude] internal bool showodds;
    [JsonInclude] internal bool alwaysshowstop;
    [JsonInclude] internal bool autooption;
    [JsonInclude] internal bool autoroll;
    [JsonInclude] internal bool autostop;
    [JsonInclude] internal bool backupconfigs = true;
    [JsonInclude] internal int w = 720;
    [JsonInclude] internal int h = 720;
    [JsonInclude] internal int fps;
    [JsonInclude] internal string music = @"resources\music.mp3";
    [JsonInclude] internal string roll = @"resources\roll.wav";
    [JsonInclude] internal string fail = @"resources\fail.wav";
    [JsonInclude] internal string score = @"resources\score.wav";
    [JsonInclude] internal string win = @"resources\win.wav";
    [JsonInclude] internal string backgroundcolor = "#C09060";
    [JsonInclude] internal string pegbordercolor = "#800000";
    [JsonInclude] internal string boardcolor = "#800000";
    [JsonInclude] internal string emptycolor = "#400000";
    [JsonInclude] internal string pipcolor = "#000000";
    [JsonInclude] internal string hilighttextcolor = "#FFFFFF";
    [JsonInclude] internal string rollcolor = "#00FF00";
    [JsonInclude] internal string stopcolor = "#FF0000";
    [JsonInclude] internal string textcolor = "#000000";
    [JsonInclude] internal string markercolor = "#C0C0C0";
    [JsonInclude] internal float musicvol = 1;
    [JsonInclude] internal float sfxvol = 1;

    public Config()
    {
        alwaysshowstop = false;
        autooption = false;
        autoroll = false;
        autostop = false;
        fps = 0;
    }
    internal Config(bool alwaysshowstop, bool autooption, bool autoroll, bool autostop, int fps)
    {
        this.alwaysshowstop = alwaysshowstop;
        this.autooption = autooption;
        this.autoroll = autoroll;
        this.autostop = autostop;
        this.fps = fps;
    }

    internal static Config? FromFile(string filename)
    {
        return JsonSerializer.Deserialize<Config>(new StreamReader(filename).ReadToEnd());
    }

    internal void AddPlayer()
    {
        players++;
        dirty = true;
    }

    internal void EditPlayer()
    {
        playernames[((EditPlayerData)Screens.Data).playerid] = ((EditPlayerData)Screens.Data).name; 
        playercolor[((EditPlayerData)Screens.Data).playerid] = ((EditPlayerData)Screens.Data).color;
        dirty = true;
    }

    internal void RemovePlayer(int p)
    {
        var tmpcolor = playercolor[p];
        var tmpname = playernames[p];

        for (var i = p; i < 3; i++)
        {
            playercolor[i] = playercolor[i + 1];
            playernames[i] = playernames[i + 1];
        }

        playercolor[3] = tmpcolor;
        playernames[3] = tmpname;

        players--;
        dirty = true;
    }

    internal void SaveChanges()
    {
        if (backupconfigs)
            // ReSharper disable once StringLiteralTypo
            File.Copy(Program.Configfile, Program.Configfile + ".backup_" + DateTime.Now.ToString("yyyyMMddHHmmss"));

        using (var fout = new StreamWriter(Program.Configfile))
            fout.Write(JsonSerializer.Serialize(this));
        dirty = false;
    }

    internal void ToggleOdds()
    {
        showodds = !showodds;
        dirty = true;
    }

    internal void ToggleShuffle()
    {
        shuffle = !shuffle;
        dirty = true;
    }
}
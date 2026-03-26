// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
namespace CantStop;

internal static class ScreenMain
{
    internal static Screen Get(GameEngine ge, Graphics gfx)
    {
        return new Screen
        {
            id = "Main",
            buttons = [
                new ButtonMap{
                    onclicked = ge.RollClicked,
                    isactive = () => ge is { showroll: true, replaylog: null },
                    draw = () => { gfx.buttons["BigButton"].draw("ROLL"); },
                    wasclicked = () => ge.replaylog is null && gfx.buttons["BigButton"].wasclicked(0)
                },

                new ButtonMap{
                    draw = () => { gfx.buttons["Stop"].draw("CLOSE"); },
                    isactive = () => ge.gameover,
                    wasclicked = () => gfx.buttons["Stop"].wasclicked(0),
                    onclicked = () => { Program.Terminate = true; }
                },

                new ButtonMap{
                    onclicked = ge.StopClicked,
                    isactive = () => ge is { showstop: true, replaylog: null },
                    draw = () => { gfx.buttons["Stop"].draw("STOP"); },
                    wasclicked = () => ge.replaylog is null && gfx.buttons["Stop"].wasclicked(0)
                },

                new ButtonMap
                {
                    onclicked = () => { ge.OptionClicked(0); },
                    isactive = () => ge is { startofturn: false, gameover: false, options.Count: > 0 },
                    draw = () => { gfx.buttons["Option"].draw(new Tuple<int,bool,string>(0,ge.optionselected == 0,ge.OptionString(0))); },
                    wasclicked = () => ge.replaylog is null && gfx.buttons["Option"].wasclicked(new Tuple<int,string>(0,ge.OptionString(0)))
                },

                new ButtonMap
                {
                    onclicked = () => { ge.OptionClicked(1); },
                    isactive = () => ge is { startofturn: false, gameover: false, options.Count: > 1 },
                    draw = () => { gfx.buttons["Option"].draw(new Tuple<int,bool,string>(1,ge.optionselected == 1,ge.OptionString(1))); },
                    wasclicked = () => ge.replaylog is null && gfx.buttons["Option"].wasclicked(new Tuple<int,string>(1,ge.OptionString(1)))
                },

                new ButtonMap
                {
                    onclicked = () => { ge.OptionClicked(2); },
                    isactive = () => ge is { startofturn: false, gameover: false, options.Count: > 2 },
                    draw = () => { gfx.buttons["Option"].draw(new Tuple<int,bool,string>(2,ge.optionselected == 2,ge.OptionString(2))); },
                    wasclicked = () => ge.replaylog is null && gfx.buttons["Option"].wasclicked(new Tuple<int,string>(2,ge.OptionString(2)))
                },

                new ButtonMap
                {
                    onclicked = () => { ge.OptionClicked(3); },
                    isactive = () => ge is { startofturn: false, gameover: false, options.Count: > 3 },
                    draw = () => { gfx.buttons["Option"].draw(new Tuple<int,bool,string>(3,ge.optionselected == 3,ge.OptionString(3))); },
                    wasclicked = () => ge.replaylog is null && gfx.buttons["Option"].wasclicked(new Tuple<int,string>(3,ge.OptionString(3)))
                },

                new ButtonMap
                {
                    onclicked = () => { ge.OptionClicked(4); },
                    isactive = () => ge is { startofturn: false, gameover: false, options.Count: > 4 },
                    draw = () => { gfx.buttons["Option"].draw(new Tuple<int,bool,string>(4,ge.optionselected == 4,ge.OptionString(4))); },
                    wasclicked = () => ge.replaylog is null && gfx.buttons["Option"].wasclicked(new Tuple<int,string>(4,ge.OptionString(4)))
                },

                new ButtonMap
                {
                    onclicked = () => { ge.OptionClicked(5); },
                    isactive = () => ge is { startofturn: false, gameover: false, options.Count: > 5 },
                    draw = () => { gfx.buttons["Option"].draw(new Tuple<int,bool,string>(5,ge.optionselected == 5,ge.OptionString(5))); },
                    wasclicked = () => ge.replaylog is null && gfx.buttons["Option"].wasclicked(new Tuple<int,string>(5,ge.OptionString(5)))
                },

                new ButtonMap
                {
                    onclicked = () => { Program.Terminate = true; },
                    isactive = () => ge is { gameover: false, replaylog: null },
                    draw = () => { gfx.buttons["Save"].draw(new Tuple<string, int>("Save&Exit",0)); },
                    wasclicked = () => gfx.buttons["Save"].wasclicked(new Tuple<string, int>("Save&Exit",0))
                },

                new ButtonMap{
                    onclicked = () => { if (ge.cfgbk is not null) Program.Cfg = ge.cfgbk; Screens.SetCurrent("Setup"); },
                    isactive = () => ge.gameover,
                    draw = () => { gfx.buttons["BigButton"].draw("NEW\nGAME"); },
                    wasclicked = () => gfx.buttons["BigButton"].wasclicked(0)
                },

                new ButtonMap
                {
                    onclicked = () => { ge.InitReplay(ge.gamelog); },
                    isactive = () => ge.gameover,
                    draw = () => { gfx.buttons["Replay"].draw(new Tuple<string, int>("View Replay",0)); },
                    wasclicked = () => gfx.buttons["Replay"].wasclicked(new Tuple<string, int>("View Replay",0))
                },

                new ButtonMap
                {
                    onclicked = ge.ReplayAll,
                    isactive = () => ge.replaylog is not null,
                    draw = () => { gfx.buttons["Stop"].draw("Skip to End"); },
                    wasclicked = () => gfx.buttons["Stop"].wasclicked(0)
                },

                new ButtonMap
                {
                    onclicked = () => { ge.showstats = !ge.showstats; },
                    isactive = () => ge.gameover,
                    draw = () => { gfx.buttons["Stats"].draw(new Tuple<string, int>(ge.showstats ? "Hide Stats" : "Show Stats",0)); },
                    wasclicked = () => gfx.buttons["Stats"].wasclicked(new Tuple<string, int>(ge.showstats ? "Hide Stats" : "Show Stats",0))
                }



            ],
            sounds = [
                new SoundMap{
                    wastriggered = ge.ReplayTimerUp,
                    play = ge.StepReplay
                },
                new SoundMap{
                    wastriggered = () => ge.triggerwin,
                    play = () => { gfx.PlayWin(); ge.triggerwin = false; }
                },
                new SoundMap{
                    wastriggered = () => ge.triggerscore,
                    play = () => { gfx.PlayScore(); ge.triggerscore = false; }
                },
                new SoundMap{
                    wastriggered = () => ge.triggerfail,
                    play = () => { gfx.PlayFail(); ge.triggerfail = false; }
                },
                new SoundMap{
                    wastriggered = () => ge.triggerroll,
                    play = () => { gfx.PlayRoll(); ge.triggerroll = false; }
                }
            ],
            displays = [
                new Display {
                    isactive = () => true,
                    draw = () => { gfx.DrawBoard(ge.board,ge.player,ge.GetScores()); }
                },

                new Display {
                    isactive = () => ge.startofturn || ge.gameover,
                    draw = () => { gfx.DrawMessage(ge.player,"" + Program.Cfg.playernames[ge.player] + (ge.startofturn ? "'s\nTurn" : "\nwins!")); }
                },

                new Display {
                    isactive = () => ge is { startofturn: false, gameover: false },
                    draw = () => { gfx.DrawDice(ge.dice,ge.player); }
                },

                new Display {
                    isactive = () => ge is { startofturn: false, gameover: false },
                    draw = () => gfx.DrawHilights(ge.nextmarkers, ge.player)
                },

                new Display {
                    isactive = () => ge is { startofturn: false, gameover: false },
                    draw = () => { gfx.DrawMarkers(ge.nextmarkers, ge.board, ge.player); }
                },

                new Display {
                    isactive = () => Program.Cfg.showodds && ge is { showroll: true, showstop: true, replaylog: null },
                    draw = () => { gfx.DrawOdds(ge.GetOdds()); }
                },

                new Display {
                    isactive = () => ge is { gameover: true, showstats: true },
                    draw = () => { gfx.DrawGameOver(ge.playerstats);  }
                }
            ]
        };
    }
}
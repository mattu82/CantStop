// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
namespace CantStop;

internal static class ScreenSetup
{
    internal static Screen Get(GameEngine ge, Graphics gfx)
    {
        return new Screen
        {
            id = "Setup",
            displays = [
                new Display {
                    isactive = () => true,
                    draw = gfx.DrawSetup
                }
            ],
            buttons = [
                new ButtonMap {
                    onclicked = () => {
                        if (Program.Cfg.dirty)
                            Program.Cfg.SaveChanges();

                        ge.NewGame();

                        Screens.SetCurrent("Main");
                    },
                    isactive = () => true,
                    draw = () => { gfx.buttons["BigButton"].draw("START"); },
                    wasclicked = () => gfx.buttons["BigButton"].wasclicked(0)
                },

                new ButtonMap {
                    onclicked = () => { Program.Cfg.RemovePlayer(0); },
                    isactive = () => Program.Cfg.players > 1,
                    draw = () => { gfx.buttons["RemovePlayer"].draw(new Tuple<string,int>("-",0)); },
                    wasclicked = () => gfx.buttons["RemovePlayer"].wasclicked(new Tuple<string,int>("-",0))
                },

                new ButtonMap {
                    onclicked = () => { Program.Cfg.RemovePlayer(1); },
                    isactive = () => Program.Cfg.players > 1,
                    draw = () => { gfx.buttons["RemovePlayer"].draw(new Tuple<string,int>("-",1)); },
                    wasclicked = () => gfx.buttons["RemovePlayer"].wasclicked(new Tuple<string,int>("-",1))
                },

                new ButtonMap {
                    onclicked = () => { Program.Cfg.RemovePlayer(2); },
                    isactive = () => Program.Cfg.players > 2,
                    draw = () => { gfx.buttons["RemovePlayer"].draw(new Tuple<string,int>("-",2)); },
                    wasclicked = () => gfx.buttons["RemovePlayer"].wasclicked(new Tuple<string,int>("-",2))
                },

                new ButtonMap {
                    onclicked = () => { Program.Cfg.RemovePlayer(3); },
                    isactive = () => Program.Cfg.players > 3,
                    draw = () => { gfx.buttons["RemovePlayer"].draw(new Tuple<string,int>("-",3)); },
                    wasclicked = () => gfx.buttons["RemovePlayer"].wasclicked(new Tuple<string,int>("-",3))
                },


                new ButtonMap {
                    onclicked = () => { Screens.SetCurrent("EditPlayer",new EditPlayerData(0)); },
                    isactive = () => true,
                    draw = () => { gfx.buttons["EditPlayer"].draw(new Tuple<string,int>(Program.Cfg.playernames[0],0)); },
                    wasclicked = () => gfx.buttons["EditPlayer"].wasclicked(new Tuple<string,int>(Program.Cfg.playernames[0],0))
                },

                new ButtonMap {
                    onclicked = () => { Screens.SetCurrent("EditPlayer",new EditPlayerData(1)); },
                    isactive = () => Program.Cfg.players > 1,
                    draw = () => { gfx.buttons["EditPlayer"].draw(new Tuple<string,int>(Program.Cfg.playernames[1],1)); },
                    wasclicked = () => gfx.buttons["EditPlayer"].wasclicked(new Tuple<string,int>(Program.Cfg.playernames[1],1))
                },

                new ButtonMap {
                    onclicked = () => { Screens.SetCurrent("EditPlayer",new EditPlayerData(2)); },
                    isactive = () => Program.Cfg.players > 2,
                    draw = () => { gfx.buttons["EditPlayer"].draw(new Tuple<string,int>(Program.Cfg.playernames[2],2)); },
                    wasclicked = () => gfx.buttons["EditPlayer"].wasclicked(new Tuple<string,int>(Program.Cfg.playernames[2],2))
                },

                new ButtonMap {
                    onclicked = () => { Screens.SetCurrent("EditPlayer",new EditPlayerData(3)); },
                    isactive = () => Program.Cfg.players > 3,
                    draw = () => { gfx.buttons["EditPlayer"].draw(new Tuple<string,int>(Program.Cfg.playernames[3],3)); },
                    wasclicked = () => gfx.buttons["EditPlayer"].wasclicked(new Tuple<string,int>(Program.Cfg.playernames[3],3))
                },

                new ButtonMap {
                    onclicked = Program.Cfg.AddPlayer,
                    isactive = () => Program.Cfg.players < 4,
                    draw = () => { gfx.buttons["AddPlayer"].draw(new Tuple<string,int>("+",Program.Cfg.players)); },
                    wasclicked = () => gfx.buttons["AddPlayer"].wasclicked(new Tuple<string,int>("+",Program.Cfg.players))
                },

                new ButtonMap{
                    onclicked = () => { ge.UpdateLogs(); Screens.SetCurrent("Title"); },
                    isactive = () => true,
                    draw = () => { gfx.buttons["Stop"].draw("<-back"); },
                    wasclicked = () => gfx.buttons["Stop"].wasclicked(0)
                },

                new ButtonMap {
                    onclicked = Program.Cfg.ToggleShuffle,
                    isactive = () => true,
                    draw = () => { gfx.buttons["Checklist"].draw(new Tuple<int, bool, string>(0,Program.Cfg.shuffle,"Shuffle Player Order")); },
                    wasclicked = () => gfx.buttons["Checklist"].wasclicked(new Tuple<int,string>(0,"Shuffle Player Order"))
                },

                new ButtonMap {
                    onclicked = Program.Cfg.ToggleOdds,
                    isactive = () => true,
                    draw = () => { gfx.buttons["Checklist"].draw(new Tuple<int, bool, string>(1,Program.Cfg.showodds,"Show Odds To Continue")); },
                    wasclicked = () => gfx.buttons["Checklist"].wasclicked(new Tuple<int,string>(1,"Show Odds To Continue"))
                },

                new ButtonMap {
                    onclicked = gfx.SetMusicVol,
                    isactive = () => true,
                    draw = () => { gfx.buttons["Music"].draw(new Tuple<string,double,string>("music ["+ Program.Cfg.musicvol.ToPercent() + "]",Program.Cfg.musicvol,Program.Cfg.hilighttextcolor)); },
                    wasclicked = () => gfx.buttons["Music"].wasclicked(0)
                },

                new ButtonMap {
                    onclicked = gfx.SetSfxVol,
                    isactive = () => true,
                    draw = () => { gfx.buttons["Sfx"].draw(new Tuple<string,double,string>("sfx ["+ Program.Cfg.sfxvol.ToPercent() + "]",Program.Cfg.sfxvol,Program.Cfg.hilighttextcolor)); },
                    wasclicked = () => gfx.buttons["Sfx"].wasclicked(0)
                }
            ]
        };
    }
}
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
namespace CantStop;

internal class EditPlayerData
{
    internal int playerid;
    internal string name;
    internal string color;

    internal EditPlayerData(int id)
    {
        playerid = id;
        name = Program.Cfg.playernames[id];
        color = Program.Cfg.playercolor[id];
    }
}

internal static class ScreenEditPlayer
{
    internal static Screen Get(Graphics gfx)
    {
        return new Screen
        {
            id = "EditPlayer",
            displays = [new Display
            {
                isactive = () => true,
                draw = gfx.DrawEditPlayerScreen
            }],
            buttons = [new ButtonMap {
                    isactive = () => true,
                    draw = () => { gfx.buttons["BigButton"].draw("confirm"); },
                    wasclicked = () => gfx.buttons["BigButton"].wasclicked(0),
                    onclicked = () => {
                        Program.Cfg.EditPlayer();
                        Screens.SetCurrent("Setup"); }
                },


                new ButtonMap {
                    isactive = () => true,
                    draw = () => { gfx.buttons["PlayerName"].draw("NAME: " + ((EditPlayerData)Screens.Data).name); },
                    wasclicked = () => gfx.buttons["PlayerName"].wasclicked(0),
                    onclicked = Graphics.OnKeyPressed
                },

                new ButtonMap {
                    isactive = () => true,
                    draw = () => { gfx.buttons["RSlider"].draw(new Tuple<string,double,string>("R [" + Graphics.RgbToDouble(Rgb.R,((EditPlayerData)Screens.Data).color).ToPercent() + "]",Graphics.RgbToDouble(Rgb.R,((EditPlayerData)Screens.Data).color),((EditPlayerData)Screens.Data).color)); },
                    wasclicked = () => gfx.buttons["RSlider"].wasclicked(0),
                    onclicked = () => { gfx.SetFromBar(Rgb.R); }
                },

                new ButtonMap {
                    isactive = () => true,
                    draw = () => { gfx.buttons["GSlider"].draw(new Tuple<string,double,string>("G [" + Graphics.RgbToDouble(Rgb.G,((EditPlayerData)Screens.Data).color).ToPercent() + "]",Graphics.RgbToDouble(Rgb.G,((EditPlayerData)Screens.Data).color),((EditPlayerData)Screens.Data).color)); },
                    wasclicked = () => gfx.buttons["GSlider"].wasclicked(0),
                    onclicked = () => { gfx.SetFromBar(Rgb.G); }
                },

                new ButtonMap {
                    isactive = () => true,
                    draw = () => { gfx.buttons["BSlider"].draw(new Tuple<string,double,string>("B [" + Graphics.RgbToDouble(Rgb.B,((EditPlayerData)Screens.Data).color).ToPercent() + "]",Graphics.RgbToDouble(Rgb.B,((EditPlayerData)Screens.Data).color),((EditPlayerData)Screens.Data).color)); },
                    wasclicked = () => gfx.buttons["BSlider"].wasclicked(0),
                    onclicked = () => { gfx.SetFromBar(Rgb.B); }
                },

                new ButtonMap{
                    onclicked = () => { Screens.SetCurrent("Setup"); },
                    isactive = () => true,
                    draw = () => { gfx.buttons["Stop"].draw("cancel"); },
                    wasclicked = () => gfx.buttons["Stop"].wasclicked(0)
                }



            ]
        };
    }
}
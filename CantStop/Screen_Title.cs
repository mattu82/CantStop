// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
namespace CantStop;

internal static class ScreenTitle
{
    internal static Screen Get(GameEngine ge, Graphics gfx)
    {
        return new Screen
        {
            id = "Title",
            displays = [new Display {
                isactive = () => true,
                draw = gfx.DrawTitle
            }],


            buttons = [
                new ButtonMap {
                    isactive = () => ge.continuelogs.Count > 0,
                    draw = () => { gfx.buttons["Menu"].draw(new Tuple<string,int>("Continue",0)); },
                    wasclicked = () => gfx.buttons["Menu"].wasclicked(new Tuple<string,int>("Continue",0)),
                    onclicked = () => { Screens.Data = 0; Screens.SetCurrent("Continue"); }},

                //Disabled For Now
                new ButtonMap {
                    isactive = () => false,
                    draw = () => { gfx.buttons["Menu"].draw(new Tuple<string,int>("How To Play",1)); },
                    wasclicked = () => gfx.buttons["Menu"].wasclicked(new Tuple<string,int>("How To Play",1)),
                    onclicked = () => { Screens.SetCurrent("TextFile",@"resources\howtoplay.txt"); }},

                new ButtonMap {
                    isactive = () => ge.replaylogs.Count > 0,
                    draw = () => { gfx.buttons["Menu"].draw(new Tuple<string,int>("Game Logs",2)); },
                    wasclicked = () => gfx.buttons["Menu"].wasclicked(new Tuple<string,int>("Game Logs",2)),
                    onclicked = () => { Screens.Data = 0; Screens.SetCurrent("GameLogs"); }},

                new ButtonMap {
                    isactive = () => true,
                    draw = () => { gfx.buttons["Menu"].draw(new Tuple<string,int>("Credits",3)); },
                    wasclicked = () => gfx.buttons["Menu"].wasclicked(new Tuple<string,int>("Credits",3)),
                    onclicked = () => { Screens.SetCurrent("TextFile",@"resources\credits.txt"); }},

                new ButtonMap {
                    isactive = () => true,
                    draw = () => { gfx.buttons["BigButton"].draw("NEW\nGAME"); },
                    wasclicked = () => gfx.buttons["BigButton"].wasclicked(0),
                    onclicked = () => { Screens.SetCurrent("Setup"); } },

                new ButtonMap {
                    isactive = () => true,
                    draw = () => { gfx.buttons["Stop"].draw("CLOSE"); },
                    wasclicked = () => gfx.buttons["Stop"].wasclicked(0),
                    onclicked = () => { Program.Terminate = true; } }
            ]
        };
    }
}
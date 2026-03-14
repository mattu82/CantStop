// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
//TODO:
//
//Github, vmware
//
//final refactor (use refresher?)
//----
//list button can be altered for checklist and radio list
//
//how to play
//
//advanced option & palette screen
//Config object positions
//
//player profiles
//
//AI
//texture graphics
//xl version
//online, twitch

using System.Reflection;
using static System.Environment;

namespace CantStop;

internal class Program
{
    internal static string Version = (Assembly.GetExecutingAssembly().GetName().Version ?? new Version()).ToString();
    internal static string Appdata = GetFolderPath(SpecialFolder.LocalApplicationData) + @"\CantStop";
    internal static string Configfile = "CantStop.config.json";
    internal static Config Cfg = Config.FromFile(Configfile) ?? new Config(false,false,false,false,0);
    internal static bool Terminate;
    internal static DateTime Errtime;

    private static void Main()
    {
        var ge = new GameEngine();
        var gfx = new Graphics();

        Screens.Add(ScreenTitle.Get(ge,gfx));
        Screens.Add(new Screen { id = "TextFile", displays = [new Display { isactive = () => true, draw = () => { gfx.DrawTextFile((string)Screens.Data); } }],
            buttons = [
                new ButtonMap { onclicked = () => { Screens.SetCurrent("Title"); },
                    isactive = () => true,
                    draw = () => { gfx.buttons["Stop"].draw("<-back"); },
                    wasclicked = () => gfx.buttons["Stop"].wasclicked(0)
                },
                new ButtonMap {
                    onclicked = () => { gfx.OpenTextFileHyperlink((string)Screens.Data); },
                    isactive = () => true,
                    draw = () => { },
                    wasclicked = Graphics.MouseWasClicked
                }
            ] });
        Screens.Add(ScreenSetup.Get(ge, gfx));
        Screens.Add(ScreenEditPlayer.Get(gfx));
        Screens.Add(ScreenMain.Get(ge, gfx));
        Screens.Add(new Screen {
            id = "GameLogs",
            buttons = {
                new ButtonMap { onclicked = () => { Screens.SetCurrent("Title"); },
                    isactive = () => true,
                    draw = () => { gfx.buttons["Stop"].draw("<-back"); },
                    wasclicked = () => gfx.buttons["Stop"].wasclicked(0)
                },

                new ButtonMap { onclicked = () => { ge.InitReplay(ge.replaylogs[(int)Screens.Data].file); Screens.SetCurrent("Main"); },
                    isactive = () => true,
                    draw = () => { gfx.buttons["GamelogMenu"].draw(ge.replaylogs.GetSummaries()); },
                    wasclicked = () => gfx.buttons["GamelogMenu"].wasclicked(ge.replaylogs.GetSummaries())
                }

            }
        });

        Screens.Add(new Screen
        {
            id = "Continue",
            buttons = {
                new ButtonMap { onclicked = () => { Screens.SetCurrent("Title"); },
                    isactive = () => true,
                    draw = () => { gfx.buttons["Stop"].draw("<-back"); },
                    wasclicked = () => gfx.buttons["Stop"].wasclicked(0)
                },

                new ButtonMap { onclicked = () => { ge.InitReplay(ge.continuelogs[(int)Screens.Data].file); ge.ReplayAll(); Screens.SetCurrent("Main"); },
                    isactive = () => true,
                    draw = () => { gfx.buttons["GamelogMenu"].draw(ge.continuelogs.GetSummaries()); },
                    wasclicked = () => gfx.buttons["GamelogMenu"].wasclicked(ge.continuelogs.GetSummaries())
                },

                new ButtonMap { onclicked = () => { 
                        File.Delete(ge.continuelogs[(int)Screens.Data].file);
                        ge.UpdateLogs();
                    },
                    isactive = () => true,
                    draw = () => { gfx.buttons["GamelogDelMenu"].draw(ge.continuelogs.GetDeletes()); },
                    wasclicked = () => gfx.buttons["GamelogDelMenu"].wasclicked(ge.continuelogs.GetDeletes())
                }
            }
        });

        Screens.AddButtonToAll(new ButtonMap {
            onclicked = Graphics.ExitFullscreen,
            isactive = () => Graphics.IsFullscreen,
            draw = () => { gfx.buttons["ExitFullscreen"].draw(0); },
            wasclicked = () => gfx.buttons["ExitFullscreen"].wasclicked(0)
        });

        Screens.SetCurrent("Title");

        Exception ? e = null;
        var loggederror = false;
        while (Graphics.Looping() && !Terminate)
        {
            if (e is not null)
            {
                if (!loggederror)
                {
                    using (var fout = new StreamWriter("ErrorLog.txt", true))
                    {
                        fout.WriteLine("--Error logged at " + DateTime.Now + "--");
                        fout.WriteLine(e.Message);
                        if (e.StackTrace is not null)
                            fout.WriteLine(e.StackTrace);
                        fout.WriteLine("--End log--");
                    }
                    Errtime = DateTime.Now;
                    loggederror = true;
                }
                Graphics.DrawError();
            }
            else
                try
                {
                    gfx.HandleResizing();
                    foreach (var b in Screens.Current.buttons.Where(b => b.isactive() && b.wasclicked())) b.onclicked();
                    gfx.PlayMusic();
                    foreach (var s in Screens.Current.sounds.Where(s => s.wastriggered())) s.play();
                    Graphics.BeginFrame();
                    foreach (var d in Screens.Current.displays.Where(d => d.isactive())) d.draw();
                    foreach (var b in Screens.Current.buttons.Where(b => b.isactive())) b.draw();
                    Graphics.EndFrame();
                }
                catch (Exception exception) { e = exception; }
        }

        if (ge.gameInProgress) ge.gamelog.Dump();
        Graphics.Close();
    }

}

internal static class Extensions
{
    internal static int ToPercent(this double i)
    {
        var o = (int)Math.Round(i * 100);
        if (o == 0 && i != 0) o = 1;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (o == 100 && i != 1) o = 99;
        return o;
    }
    internal static int ToPercent(this float i)
    {
        return ((double)i).ToPercent();
    }
}
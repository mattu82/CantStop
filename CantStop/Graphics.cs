// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Text;
using Raylib_cs;
using static Raylib_cs.Raylib;
using static System.Math;
using Color = Raylib_cs.Color;
// ReSharper disable PossibleLossOfFraction

namespace CantStop;

internal enum Rgb { R, G, B }


internal class WindowInfo
{
    internal int w;
    internal int h;
    internal int sz;
    internal int xo;
    internal int yo;

    internal WindowInfo()
    {
        w = GetScreenWidth();
        h = GetScreenHeight();
        sz = Min(w, h);
        xo = Max(w - h, 0) / 2;
        yo = Max(h - w, 0) / 2;
    }
}


internal class Graphics
{
    private readonly Music _music;
    private readonly Sound _win;
    private readonly Sound _fail;
    private readonly Sound _score;
    private readonly Sound _roll;
    private WindowInfo _wi;
    private static int _lastkey;

    internal Buttons buttons = new();
        

    internal Graphics()
    {
        InitAudioDevice();
        _music = LoadMusicStream(Program.Cfg.music);
        _win = LoadSound(Program.Cfg.win);
        _fail = LoadSound(Program.Cfg.fail);
        _score = LoadSound(Program.Cfg.score);
        _roll = LoadSound(Program.Cfg.roll);
        UpdateVolume();
        SetConfigFlags(ConfigFlags.ResizableWindow);
        InitWindow(Program.Cfg.w, Program.Cfg.h, "Can't Stop");
        SetExitKey(KeyboardKey.Null);
        SetTargetFPS(Program.Cfg.fps);
        SetWindowIcon(LoadImage(@"resources\Icon.png"));
        _wi = new WindowInfo();
        buttons.buttons = LoadButtons(); 
    }

    private void UpdateVolume()
    {
        SetMusicVolume(_music, Program.Cfg.musicvol);
        SetSoundVolume(_win, Program.Cfg.sfxvol);
        SetSoundVolume(_fail, Program.Cfg.sfxvol);
        SetSoundVolume(_score, Program.Cfg.sfxvol);
        SetSoundVolume(_roll, Program.Cfg.sfxvol);
    }

    private List<Button> LoadButtons()
    {
        return [
            ScaledButton("Menu",ButtonType.Text,80,171,18,18,[GetColor(Program.Cfg.hilighttextcolor)]),
            ScaledButton("BigButton",ButtonType.Button, 224, 224, 32, 32, [GetColor(Program.Cfg.rollcolor), GetColor(Program.Cfg.hilighttextcolor)]),
            ScaledButton("Stop",ButtonType.Stop,32, 224, 32, 32, [GetColor(Program.Cfg.stopcolor), GetColor(Program.Cfg.hilighttextcolor)]),
            ScaledButton("Option",ButtonType.Radio, 200, 8, 16, 16, [GetColor(Program.Cfg.hilighttextcolor), GetColor(Program.Cfg.textcolor)]),
            new Button("ExitFullscreen",ButtonType.ExitFullscreen,_wi.w*9/10,_wi.h*9/10,_wi.w/10,_wi.h/10,[GetColor(Program.Cfg.textcolor)]),
            ScaledButton("AddPlayer",ButtonType.Text,128,54,18,18,[GetColor(Program.Cfg.rollcolor)]),
            ScaledButton("EditPlayer",ButtonType.Text,92,54,18,18,[GetColor(Program.Cfg.textcolor)]),
            ScaledButton("RemovePlayer",ButtonType.Text,48,54,18,18,[GetColor(Program.Cfg.stopcolor)]),
            ScaledButton("Checklist",ButtonType.Checkbox,83,144,9,9,[GetColor(Program.Cfg.hilighttextcolor), GetColor(Program.Cfg.textcolor)]),
            ScaledButton("PlayerName",ButtonType.TextBox,18,36,18,18,[GetColor(Program.Cfg.textcolor)]),
            ScaledButton("RSlider",ButtonType.Slider,128,99,100,17,[GetColor(Program.Cfg.textcolor)]),
            ScaledButton("GSlider",ButtonType.Slider,128,117,100,17,[GetColor(Program.Cfg.textcolor)]),
            ScaledButton("BSlider",ButtonType.Slider,128,135,100,17,[GetColor(Program.Cfg.textcolor)]),
            ScaledButton("ContinueMenu",ButtonType.Text,10,128,236,10,[GetColor(Program.Cfg.textcolor)]),
            ScaledButton("Replay",ButtonType.Text,128,248,48,8,[GetColor(Program.Cfg.textcolor)]),
            ScaledButton("GamelogMenu",ButtonType.List,8,8,240,8,[GetColor(Program.Cfg.textcolor)]),
            ScaledButton("Save",ButtonType.Text,8,0,258,8,[GetColor(Program.Cfg.textcolor)]),
            ScaledButton("Music",ButtonType.Slider,87,167,100,9,[GetColor(Program.Cfg.textcolor)]),
            ScaledButton("Sfx",ButtonType.Slider,87,177,100,9,[GetColor(Program.Cfg.textcolor)]),
            ScaledButton("GamelogDelMenu",ButtonType.List,216,8,32,8,[GetColor(Program.Cfg.stopcolor)]),
            ScaledButton("Stats",ButtonType.Text,80, 248, 48, 8, [GetColor(Program.Cfg.textcolor)]),
            ScaledButton("Markers",ButtonType.Text,192,232,64,8,[GetColor(Program.Cfg.textcolor)])
        ];
    }

    private Button ScaledButton(string id, ButtonType type, int x, int y, int w, int h, Color[] colors)
    {
        return new Button(id, type, x * _wi.sz / 256 + _wi.xo, y * _wi.sz / 256 + _wi.yo, w * _wi.sz / 256, h * _wi.sz / 256, colors);
    }
        
    internal static bool IsFullscreen => IsWindowFullscreen();

    internal static void Close()
    {
        CloseWindow();
        CloseAudioDevice();
    }

    internal static void DrawError()
    {
        BeginDrawing();
        ClearBackground(Color.Black);

        var w = GetScreenWidth();

        var t = Program.Errtime + new TimeSpan(0,0,10) - DateTime.Now;

        var y = 0;
        y += DrawTextAtMaxWidth("Sorry, something unexpected happened.", 0, y, w, Color.White);
        y += DrawTextAtMaxWidth("see ErrorLog.txt for more information.", 0, y, w, Color.White);
        DrawTextAtMaxWidth("Program will close in " + t.Seconds + " seconds.", 0, y, w, Color.White);

        if (t < new TimeSpan(0,0,0))
            Program.Terminate = true;

        EndDrawing();
    }

    private static int DrawTextAtMaxWidth(string text, int x, int y, int maxWidth, Color color)
    {
        var s= 0;
        while(MeasureText(text,s) <= maxWidth)
            s++;

        DrawText(text,x,y,s-1,color);

        return s;
    }

    internal static Color GetColor(string s)
    {
        var c = ColorTranslator.FromHtml(s);
        return new Color(c.R,c.G,c.B);
    }

    internal static bool Looping()
    {
        return !WindowShouldClose();
    }

    private void DrawPeg(int x, int y, int stack, string color)
    {
        y = 13 - Abs(7 - x) - y;

        DrawScaledCircle(18*x+2,18*y+2-2*stack,8, Program.Cfg.pegbordercolor);
        DrawScaledCircle(18*x+2,18*y+2-2*stack,(float)7.5,color);


        if (y == Abs(7 - x) + 1 && x != 1)
            DrawScaledText("" + x, 18 * x - 3, 18 * y - 6, 16, Program.Cfg.hilighttextcolor);
    }

    private void DrawDie(int x, int y, int n, string color)
    {
        DrawScaledRectangle(x, y, 24, 24, color);

        if (n > 1)
        {
            DrawScaledCircle(x + 4, y + 4, 3, Program.Cfg.pipcolor);
            DrawScaledCircle(x + 20, y + 20, 3, Program.Cfg.pipcolor);
        }

        if (n == 6)
        {
            DrawScaledCircle(x + 12, y + 4, 3, Program.Cfg.pipcolor);
            DrawScaledCircle(x + 12, y + 20, 3, Program.Cfg.pipcolor);
        }

        if (n > 3)
        {
            DrawScaledCircle(x + 20, y + 4, 3, Program.Cfg.pipcolor);
            DrawScaledCircle(x + 4, y + 20, 3, Program.Cfg.pipcolor);
        }

        if (n % 2 == 1) DrawScaledCircle(x + 12, y + 12, 3, Program.Cfg.pipcolor);
    }

    internal void HandleResizing()
    {
        if (IsWindowMaximized() ^ (bool)IsWindowFullscreen())
        {
            if (!IsWindowFullscreen())
                SetWindowSize(GetMonitorWidth(GetCurrentMonitor()), GetMonitorHeight(GetCurrentMonitor()));

            ToggleFullscreen();
        }


        if (GetScreenWidth() == _wi.w && GetScreenHeight() == _wi.h) return;
        _wi = new WindowInfo();
        buttons.buttons = LoadButtons();

    }

    internal static void ExitFullscreen()
    {
        ToggleFullscreen();
        RestoreWindow();
    }

    internal void PlayWin()
    {
        PlaySound(_win);
    }

    internal void PlayScore()
    {
        PlaySound(_score);
    }

    internal void PlayFail()
    {
        PlaySound(_fail);
    }

    internal void PlayRoll()
    {
        PlaySound(_roll);
    }

    internal static void BeginFrame()
    {
        BeginDrawing();
        ClearBackground(GetColor(Program.Cfg.backgroundcolor));
    }

    internal static void EndFrame()
    {
        EndDrawing();
    }

    internal void DrawBoard(List<int>[][] board, int player, string scores)
    {
        // ReSharper disable once StringIndexOfIsCultureSpecific.1
        var iop = scores.IndexOf(Program.Cfg.playernames[player]);
        var ss = MeasureScaledText(scores[..iop]+" ",8) - MeasureScaledText(" ",8); //Not sure why adding then removing the space works better
        DrawScaledRectangle((256 - (int)MeasureScaledText(scores, 8)) / 2 + ss, 0, MeasureScaledText(Program.Cfg.playernames[player], 8), 8, Program.Cfg.playercolor[player]);


        DrawScaledText(scores, (256-(int)MeasureScaledText(scores,8))/2, 0, 8, Program.Cfg.textcolor);

        DrawScaledPoly(128, 128, 8, 128, (float)22.5, Program.Cfg.boardcolor);

        for (var i = 2; i < 13; i++)
        {
            for (var j = 2 * (6 - Abs(7 - i)); j >= 0; j--)
            {
                if (board[i][j].Count == 0)
                    DrawPeg(i, j, 0, Program.Cfg.emptycolor);
                else
                {
                    var s = 0;
                    foreach (var peg in board[i][j].OrderBy(p => (p - player + 3) % 4))
                    {
                        DrawPeg(i, j, s, Program.Cfg.playercolor[peg]);
                        s++;
                    }
                }
            }
        }
    }





    private double MeasureScaledText(string text, int size)
    {
        return MeasureText(text, size * _wi.sz / 256) * 256 / (double)_wi.sz;
    }

    internal void PlayMusic()
    {
        if (!IsMusicStreamPlaying(_music))
            PlayMusicStream(_music);
        UpdateMusicStream(_music);
    }

    internal void DrawMessage(int player, string message)
    {
        DrawScaledRectangle(8, 8, 56, 56, Program.Cfg.playercolor[player]);
        DrawScaledText(message, 16, 16, 8, Program.Cfg.pipcolor);
    }

    internal void DrawDice(List<int> dice, int player)
    {
        DrawDie(8,8, dice[0], Program.Cfg.playercolor[player]);
        DrawDie(40,8, dice[1], Program.Cfg.playercolor[player]);
        DrawDie(8,40, dice[2], Program.Cfg.playercolor[player]);
        DrawDie(40,40, dice[3], Program.Cfg.playercolor[player]);
    }

    internal void DrawMarkers(int[] nextmarkers, List<int>[][] board, int player)
    {
        int i;

        for (i = 2; i <= 12; i++)
        {
            if (nextmarkers[i] == -1) continue;
            DrawPeg(i, nextmarkers[i], board[i][nextmarkers[i]].Count, Program.Cfg.markercolor);

            var cp = 2 * (6 - Abs(7 - i));

            while (cp >= 0 && !board[i][cp].Contains(player)) cp--;

            DrawScaledLineEx(18 * i + 2, 18 * (13 - Abs(7 - i) - nextmarkers[i]) + 2 - 2 * board[i][nextmarkers[i]].Count, 18 * i + 2, 18 * (13 - Abs(7 - i) - cp) + 2 - 2 * ( (cp < 0 ? 1 : board[i][cp].Count) - 1), 2, Program.Cfg.playercolor[player]);
        }

        for (i = 0; i < 3 - nextmarkers.Count(x => x != -1); i++)
        {
            DrawPeg(1, 0, i, Program.Cfg.markercolor);
        }
    }

    internal void DrawOdds(string text)
    {
        DrawScaledText(text, 85, 248, 8, Program.Cfg.textcolor);
    }

    internal void DrawHilights(int[] nextmarkers, int player)
    {
        for (var i = 2; i <= 12; i++)
            if (nextmarkers[i] == 2 * (6 - Abs(7 - i)))
            {
                var c = GetColor(Program.Cfg.playercolor[player]);
                c.A = (byte)Abs(GetTime() * 510 % 510 - 255);
                DrawScaledRectangle(18 * i - 9 + 2, 0, 18, 256, c);
            }
    }



    internal void DrawTitle()
    {
        DrawScaledPoly(128, 128, 8, 128, (float)22.5, Program.Cfg.boardcolor);
        DrawScaledText("CAN'T\nSTOP", 64, 85, 42, Program.Cfg.hilighttextcolor);
        DrawScaledText("v" + Program.Version, 152, 240, 8, Program.Cfg.textcolor);
    }

    internal void DrawSetup()
    {
            
        DrawScaledText("NEW GAME\n\nPlayers:", 92, 0, 17, Program.Cfg.textcolor);

        for (var p = 0; p < Program.Cfg.players; p++)
        {
            DrawScaledRectangle(66, 53 + p * 18, 20, 20, Program.Cfg.boardcolor);
            DrawScaledCircle(76, 63 + p * 18, 8, Program.Cfg.pegbordercolor);
            DrawScaledCircle(76, 63 + p * 18, (float)7.5, Program.Cfg.playercolor[p]);
        }

    }


    private void DrawScaledCircle(int x, int y, float r, string color)
    {
        DrawCircle(x * _wi.sz / 256 + _wi.xo, y * _wi.sz / 256 + _wi.yo, r * _wi.sz / 256, GetColor(color));
    }

    private void DrawScaledRectangle(int x, int y, int w, int h, string color)
    {
        DrawScaledRectangle(x,y, (double)w,h,GetColor(color));
    }

    private void DrawScaledRectangle(int x, int y, int w, int h, Color color)
    {
        DrawScaledRectangle(x,y,(double)w,h,color);
    }

    private void DrawScaledRectangle(double x, int y, double w, int h, string color)
    {
        DrawScaledRectangle(x, y, w, h, GetColor(color));
    }

    private void DrawScaledRectangle(double x, int y, double w, int h, Color color)
    {
        DrawRectangle((int)(x * _wi.sz / 256 + _wi.xo), y * _wi.sz / 256 + _wi.yo, (int)(w * _wi.sz / 256), h * _wi.sz / 256, color);
    }


    private void DrawScaledText(string s, int x, int y, int size, string color)
    {
        DrawText(s, x * _wi.sz / 256 + _wi.xo, y * _wi.sz / 256 + _wi.yo, size * _wi.sz / 256, GetColor(color));
    }

    private void DrawScaledPoly(int x,int y,int sides,float size,float rotation,string color)
    {
        DrawPoly(new Vector2(x * _wi.sz / 256 + _wi.xo, y * _wi.sz / 256 + _wi.yo), sides, size * _wi.sz / 256, rotation, GetColor(color));
    }

    private void DrawScaledLineEx(int x1, int y1, int x2, int y2,float size,string color)
    {
        DrawLineEx(new Vector2(x1 * _wi.sz / 256 + _wi.xo, y1 * _wi.sz / 256 + _wi.yo), new Vector2(x2 * _wi.sz / 256 + _wi.xo, y2 * _wi.sz / 256 + _wi.yo), size * _wi.sz / 256, GetColor(color));
    }

    internal void DrawEditPlayerScreen()
    {
        DrawScaledText("Player Info:", 75, 0, 18, Program.Cfg.textcolor);

        DrawScaledText("COLOR:", 18, 72, 18, Program.Cfg.textcolor);
        DrawScaledRectangle(18, 90 , 20, 20, Program.Cfg.boardcolor);
        DrawScaledCircle(28, 100 , 8, Program.Cfg.pegbordercolor);
        DrawScaledCircle(28, 100 , (float)7.5, ((EditPlayerData)Screens.Data).color);
        DrawDie(40, 90, (int)(GetTime() * 6) % 6 + 1, ((EditPlayerData)Screens.Data).color);
    }

    internal static double RgbToDouble(Rgb rgb, string color)
    {
        return rgb switch
        {
            Rgb.R => GetColor(color).R / 255.0,
            Rgb.G => GetColor(color).G / 255.0,
            Rgb.B => GetColor(color).B / 255.0,
            _ => 0
        };
    }

    internal static bool KeyWasPressed()
    {
        _lastkey = GetKeyPressed();
        return _lastkey != 0;
    }

    internal static void OnKeyPressed()
    {
        if (_lastkey == 259 && ((EditPlayerData)Screens.Data).name.Length > 0)
            ((EditPlayerData)Screens.Data).name = ((EditPlayerData)Screens.Data).name[..^1];
        else switch ((char)_lastkey)
        {
            case >= 'A' when _lastkey <= 'Z':
            {
                if (IsKeyDown(KeyboardKey.LeftShift))
                    ((EditPlayerData)Screens.Data).name += (char)_lastkey;
                else
                    ((EditPlayerData)Screens.Data).name += (char)(_lastkey + 32);
                break;
            }
            case ' ':
            case >= '0' and <= '9':
                ((EditPlayerData)Screens.Data).name += (char)_lastkey;
                break;
        }
    }

    internal void SetFromBar(Rgb rgb)
    {
        var c = GetColor(((EditPlayerData)Screens.Data).color);
        var v = (byte)Min(255,Max(0,((GetMouseX()-_wi.xo) * 256 / _wi.sz - 128) * 255 / 100));

        switch (rgb)
        {
            case Rgb.R: c.R = v; break;
            case Rgb.G: c.G = v; break;
            case Rgb.B:
            default: c.B = v; break;
        }

        ((EditPlayerData)Screens.Data).color = GetColorString(c);
    }

    private static string GetColorString(Color c)
    {
        return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
    }

    internal void DrawTextFile(string filename)
    {
        var y = 4;

        foreach (var line in new StreamReader(filename).ReadToEnd().Split("\r\n"))
        {
            DrawScaledText(line, 4, y, 7, line.Contains("http") ? "#0000FF" : Program.Cfg.textcolor);
            y += 7;
        }

    }

    internal void OpenTextFileHyperlink(string filename)
    {
        var lines = new StreamReader(filename).ReadToEnd().Split("\r\n");


        var i = (GetMouseY() * 256 / _wi.sz - 4) / 7;

        if (0 > i || i >= lines.Length) return;
        if (lines[i].Contains("http"))
            Process.Start(new ProcessStartInfo("cmd", "/c start " + lines[i].Trim().Replace("--","")) { CreateNoWindow = true });
    }

    internal static bool MouseWasClicked()
    {
        return IsMouseButtonReleased(MouseButton.Left);
    }

    internal void DrawGameOver(Stats[] stats)
    {
        for (var p = 0; p < Program.Cfg.players; p++)
        {
            DrawScaledRectangle(56 + 72*(p%2), 56 + 72*(p/2), 72, 72, Program.Cfg.textcolor);
            DrawScaledRectangle(58 + 72 * (p % 2), 58 + 72 * (p / 2), 68, 68, Program.Cfg.playercolor[p]);

            var s = new StringBuilder();
            s.Append(Program.Cfg.playernames[p] + "\n\n");
            s.Append("Play Time : " + stats[p].playtime.ToString(@"mm\:ss\.ff") + "\n");
            s.Append("Rolls : " + stats[p].rolls + "\n");
            s.Append("Best Streak : " + stats[p].maxstreak + "\n");
            s.Append("Distance Moved : " + stats[p].distance + "\n");
            s.Append("Biggest Risk : " + Stats.BustcountToOdds(stats[p].maxbustcount) + "\n");
            s.Append("Busts : " + stats[p].failedrolls + "\n");

            DrawScaledText(s.ToString(), 60 + 72*(p%2), 60 + 72*(p/2), 6, Program.Cfg.textcolor);
        }


    }

    internal void SetMusicVol()
    {
        Program.Cfg.musicvol =  Min(1, Max(0, ((GetMouseX() - _wi.xo) * (float)256 / _wi.sz - 83) / 100));
        Program.Cfg.dirty = true;
        UpdateVolume();
    }

    internal void SetSfxVol()
    {
        Program.Cfg.sfxvol = Min(1, Max(0, ((GetMouseX() - _wi.xo) * (float)256 / _wi.sz - 83) / 100));
        Program.Cfg.dirty = true;
        UpdateVolume();

        if (!IsSoundPlaying(_roll))
            PlayRoll();
    }
}
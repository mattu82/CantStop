// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable PossibleLossOfFraction
// ReSharper disable UnusedParameter.Local

using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace CantStop;

internal enum ButtonType { Button, Text, List, Checkbox, Radio, Stop, TextBox, Slider, ExitFullscreen }

internal class Button
{
    internal string id;
    internal Action<object> draw;
    internal Func<object, bool> wasclicked;

    internal Button(string id, ButtonType type, int x, int y, int w, int h, Color[] colors)
    {
        this.id = id;

        switch (type)
        {
            case ButtonType.Button:
                draw = text =>
                {
                    DrawCircle(x, y, w, colors[0]);

                    var txt = (string)text;
                    var sz = 1;
                    while (MeasureText(txt, sz) < 2 * w) sz++;
                    sz = sz * (txt.Contains('\n') ? 12 : 15) / 16;
                    DrawText(txt, x - w* (txt.Contains('\n') ? 12 : 15) / 16, y - sz / (txt.Contains('\n') ? 1 : 2), sz, colors[1]);


                };
                wasclicked = o => IsMouseButtonReleased(MouseButton.Left) && IsMouseInCircle(x, y, w);
                break;
            case ButtonType.Radio:
            case ButtonType.Checkbox:
                draw = tuple =>
                {
                    var t = (Tuple<int, bool, string>)tuple;

                    if (type == ButtonType.Radio)
                    {
                        DrawCircle(x + w / 2, y + h * t.Item1 + h / 2, h / 4, colors[0]);
                    }
                    else
                    {
                        DrawRectangle(x, y + h * t.Item1, w, h, colors[0]);
                    }

                    if (t.Item2)
                    {
                        if (type == ButtonType.Radio)
                            DrawCircle(x + w / 2, y + h * t.Item1 + h / 2, h * 3 / 16, colors[1]);
                        else
                        {
                            DrawLineEx(new Vector2(x, y + h * t.Item1 + h / 2), new Vector2(x + w / 2, y + +h * t.Item1 + h), h / 8, colors[1]);
                            DrawLineEx(new Vector2(x + w / 2, y + h * t.Item1 + h), new Vector2(x + w, y + h * t.Item1), h / 8, colors[1]);
                        }
                    }

                    DrawText(t.Item3, x + w, y + h * t.Item1, h, colors[1]);
                };
                wasclicked = tuple =>
                {
                    var t = (Tuple<int, string>)tuple;
                    return IsMouseButtonReleased(MouseButton.Left) && IsMouseInBox(x, y + t.Item1 * h, w + MeasureText(t.Item2, h), h);
                };
                break;
            case ButtonType.Text:
                draw = tuple =>
                {
                    var t = (Tuple<string, int>)tuple;

                    //Hacky way to do dynamic color for now
                    if (t.Item1.StartsWith('0'))
                    {
                        DrawText(t.Item1.Replace("0", "No"), x, y + t.Item2 * h, h, Graphics.GetColor(Program.Cfg.stopcolor));
                    }
                    else
                    {
                        DrawText(t.Item1, x, y + t.Item2 * h, h, colors[0]);
                    }

                };
                wasclicked = tuple =>
                {
                    var t = (Tuple<string, int>)tuple;
                    return IsMouseButtonReleased(MouseButton.Left) && IsMouseInBox(x, y + t.Item2 * h, MeasureText(t.Item1, h), h);
                };
                break;
            case ButtonType.List:
                draw = list =>
                {
                    var l = (List<string>) list;

                    var line = 0;

                    foreach (var s in l)
                    {
                        DrawText(s, x, y + line * h, h, colors[0]);
                        line++;
                    }
                };
                wasclicked = list =>
                {
                    var l = (List<string>)list;

                    if (l.Count == 0 || !IsMouseButtonReleased(MouseButton.Left) || !IsMouseInBox(x, y, l.Max(s => MeasureText(s, h)), l.Count * h)) return false;

                    Screens.Data = (GetMouseY() - y) / h;
                    return true;
                };
                break;
            case ButtonType.Stop:
                draw = text =>
                {
                    DrawPoly(new Vector2(x, y), 8, w, (float)22.5, colors[0]);

                    var txt = (string)text;
                    var sz = 1;
                    while (MeasureText(txt, sz) < 2 * w) sz++;
                    sz = sz * (txt.Contains('\n') ? 12 : 15) / 16;
                    DrawText(txt, x - w * (txt.Contains('\n') ? 12 : 15) / 16, y - sz / (txt.Contains('\n') ? 1 : 2), sz, colors[1]);
                };
                wasclicked = o => IsMouseButtonReleased(MouseButton.Left) && IsMouseInCircle(x, y, w);
                break;
            case ButtonType.TextBox:
                draw = text =>
                {
                    DrawText((string)text + ((int)(GetTime() * 4) % 2 == 0 ? "|" : ""), x, y, h, colors[0]);
                };
                wasclicked = o => Graphics.KeyWasPressed();
                break;
            case ButtonType.Slider:
                draw = tuple =>
                {
                    var t = (Tuple<string, double, string>)tuple;
                    DrawText(t.Item1, x - MeasureText(t.Item1, h) - h / 2, y - h / 2, h, colors[0]);
                    DrawLineEx(new Vector2(x, y), new Vector2(x + w, y), h / 16, colors[0]);
                    DrawRectangle(x + (int)(t.Item2 * w) - h / 2, y - h / 2, h, h, Graphics.GetColor(t.Item3));
                };
                wasclicked = o => IsMouseButtonDown(MouseButton.Left) && IsMouseInBox(x - h / 2, y - h / 2, w + h, h);
                break;
            case ButtonType.ExitFullscreen:
                draw = o =>
                {
                    DrawLineEx(new Vector2(x, y + h),
                        new Vector2(x + 4 * w / 10, y + 6 * h / 10), h / 20, colors[0]);
                    DrawLineEx(new Vector2(x + 2 * w / 10, y + 6 * h / 10),
                        new Vector2(x + 4 * w / 10, y + 6 * h / 10), h / 20, colors[0]);
                    DrawLineEx(new Vector2(x + 4 * w / 10, y + 8 * h / 10),
                        new Vector2(x + 4 * w / 10, y + 6 * h / 10), h / 20, colors[0]);


                    DrawLineEx(new Vector2(x + 6 * w / 10, y + 4 * h / 10),
                        new Vector2(x + w, y), h / 20, colors[0]);
                    DrawLineEx(new Vector2(x + 6 * w / 10, y + 4 * h / 10),
                        new Vector2(x + 6 * w / 10, y + 2 * h / 10), h / 20, colors[0]);
                    DrawLineEx(new Vector2(x + 6 * w / 10, y + 4 * h / 10),
                        new Vector2(x + 8 * w / 10, y + 4 * h / 10), h / 20, colors[0]);
                };
                wasclicked = o => IsMouseButtonReleased(MouseButton.Left) && IsMouseInBox(x, y, w, h);
                break;
            default:
                draw = o => { };
                wasclicked = o => false;
                break;
        }
    }

    internal static bool IsMouseInBox(int x, int y, int w, int h)
    {
        return x <= GetMouseX() && GetMouseX() <= x + w && y <= GetMouseY() && GetMouseY() <= y + h;
    }

    internal static bool IsMouseInCircle(int x, int y, int r)
    {
        return (x - GetMouseX()) * (x - GetMouseX()) + (y - GetMouseY()) * (y - GetMouseY()) <= r * r;
    }

}

internal class Buttons
{
    internal List<Button> buttons = [];

    internal Button this[string id]
    {
        get
        {
            return buttons.First(b => b.id == id);
        }
    }
}
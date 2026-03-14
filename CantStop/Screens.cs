// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
namespace CantStop;

internal class ButtonMap
{
    internal required Action draw;
    internal required Func<bool> wasclicked;
    internal required Func<bool> isactive;
    internal required Action onclicked;
}

internal class Display
{
    internal required Func<bool> isactive;
    internal required Action draw;
}

internal class SoundMap
{
    internal required Func<bool> wastriggered;
    internal required Action play;
}

internal class Screen
{
    internal string id = string.Empty;
    internal List<ButtonMap> buttons = [];
    internal List<Display> displays = [];
    internal List<SoundMap> sounds = [];
}

internal static class Screens
{
    private static readonly List<Screen> ScreenList = [];
    private static int _currentid = -1;
    internal static object Data = 0;

    internal static Screen Current => ScreenList[_currentid];

    internal static void SetCurrent(string id)
    {
        _currentid = ScreenList.IndexOf(ScreenList.First(s => s.id == id));
    }

    internal static void SetCurrent(string id, object indata)
    {
        _currentid = ScreenList.IndexOf(ScreenList.First(s => s.id == id));
        Data = indata;
    }

    internal static void Add(Screen screen) { ScreenList.Add(screen); }

    internal static void AddButtonToAll(ButtonMap button)
    {
        foreach (var s in ScreenList)
            s.buttons.Add(button);
    }
}
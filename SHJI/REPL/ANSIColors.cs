namespace System.ANSIConsole;

public static class Colors
{
    private static string[] colors = new[] { "255;0;0", "255;128;0", "255;255;0", "0;255;0", "0;255;255", "0;0;255", "139;0;255" };

    public static string Black(this string text) => $"\x1b[30m{text}\x1b[0m";
    public static string Red(this string text) => $"\x1b[31m{text}\x1b[0m";
    public static string Green(this string text) => $"\x1b[32m{text}\x1b[0m";
    public static string Yellow(this string text) => $"\x1b[33m{text}\x1b[0m";
    public static string Blue(this string text) => $"\x1b[34m{text}\x1b[0m";
    public static string Magenta(this string text) => $"\x1b[35m{text}\x1b[0m";
    public static string Cyan(this string text) => $"\x1b[36m{text}\x1b[0m";
    public static string White(this string text) => $"\x1b[37m{text}\x1b[0m";
    public static string Color(this string text, string colorCode) => $"\x1b[38;2;{colorCode}m{text}\x1b[0m";
    public static string Bold(this string text) => $"\x1b[1m{text}\x1b[0m";
    public static string Dim(this string text) => $"\x1b[2m{text}\x1b[0m";
    public static string Italic(this string text) => $"\x1b[3m{text}\x1b[0m";
    public static string Underline(this string text) => $"\x1b[4m{text}\x1b[0m";
    public static string Inverse(this string text) => $"\x1b[7m{text}\x1b[0m";
    public static string Hidden(this string text) => $"\x1b[8m{text}\x1b[0m";
    public static string Strike(this string text) => $"\x1b[9m{text}\x1b[0m";
    public static string Rainbow(this string text) => string.Concat(text.Select((c, i) => $"\x1b[38;2;{colors[i % colors.Length]}m{c}\x1b[0m"));
    public static string Framed(this string text) => $"\x1b[51m{text}\x1b[0m";
    public static string Encircled(this string text) => $"\x1b[52m{text}\x1b[0m";
    public static string Overlined(this string text) => $"\x1b[53m{text}\x1b[0m";
    public static string Blink(this string text) => $"\x1b[5m{text}\x1b[0m";
    public static string RapidBlink(this string text) => $"\x1b[6m{text}\x1b[0m";
    public static string CrossedOut(this string text) => $"\x1b[9m{text}\x1b[0m";
    public static string Fraktur(this string text) => $"\x1b[20m{text}\x1b[0m";
    public static string BoldOff(this string text) => $"\x1b[21m{text}\x1b[0m";
    public static string ItalicOff(this string text) => $"\x1b[23m{text}\x1b[0m";
    public static string UnderlineOff(this string text) => $"\x1b[24m{text}\x1b[0m";
    public static string BlinkOff(this string text) => $"\x1b[25m{text}\x1b[0m";
    public static string InverseOff(this string text) => $"\x1b[27m{text}\x1b[0m";
    public static string FramedOff(this string text) => $"\x1b[51m{text}\x1b[0m";
    public static string EncircledOff(this string text) => $"\x1b[52m{text}\x1b[0m";
    public static string OverlinedOff(this string text) => $"\x1b[53m{text}\x1b[0m";
    public static string FrakturOff(this string text) => $"\x1b[23m{text}\x1b[0m";

    public static string Apply(string text, ConsoleColor c, int fs = 0) => $"\x1b[38;2;{(int)c:X};{(int)fs}m{text}\x1b[0m";
}

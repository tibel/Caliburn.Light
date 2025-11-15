namespace Caliburn.Light.WPF;

internal static class BooleanBoxes
{
    public static readonly object TrueBox = true;
    public static readonly object FalseBox = false;

    public static object Box(bool value)
    {
        return value ? TrueBox : FalseBox;
    }
}

namespace AutoMouseClick;

internal sealed class RecordedAction
{
    public int DelayMilliseconds { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public MouseButtonType MouseButton { get; set; }
    public ClickModeType ClickMode { get; set; }
}

internal enum MouseButtonType
{
    Left,
    Right
}

internal enum ClickModeType
{
    Single,
    Double
}

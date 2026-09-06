namespace AutoMouseClick;

internal sealed class RecordedAction
{
    public int DelayMilliseconds { get; set; }
    public RecordedActionType ActionType { get; set; } = RecordedActionType.Mouse;
    public int X { get; set; }
    public int Y { get; set; }
    public MouseButtonType MouseButton { get; set; } = MouseButtonType.Left;
    public ClickModeType ClickMode { get; set; } = ClickModeType.Single;
    public Keys KeyCode { get; set; } = Keys.None;
    public KeyActionType KeyAction { get; set; } = KeyActionType.KeyPress;
    public bool IsPlaceholder { get; set; }
}

internal enum RecordedActionType
{
    Mouse,
    Keyboard
}

internal enum MouseButtonType
{
    Left,
    Right,
    Middle
}

internal enum ClickModeType
{
    Single,
    Double
}

internal enum KeyActionType
{
    KeyDown,
    KeyUp,
    KeyPress
}

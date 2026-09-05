namespace AutoMouseClick;

internal sealed class AppSettings
{
    public int DefaultStepDelayMilliseconds { get; set; } = 1000;
    public bool RepeatUntilStopped { get; set; } = false;
    public int RepeatCount { get; set; } = 1;
    public bool MinimizeToTray { get; set; }
    public List<RecordedAction> RecordedActions { get; set; } = new();
}

using System.Runtime.InteropServices;
using System.Text.Json;

namespace AutoMouseClick
{
    public partial class AutoClickerForm : Form
    {
        private const int ReplayHotkeyId = 1001;
        private const int AddPlaceholderHotkeyId = 1002;
        private const int ToggleRecordingHotkeyId = 1003;
        private const int MinDelayMilliseconds = 1;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const int WM_HOTKEY = 0x0312;
        private const byte KEYEVENTF_KEYUP = 0x02;
        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_LBUTTONDBLCLK = 0x0203;
        private const int WM_RBUTTONDBLCLK = 0x0206;
        private const int WM_MBUTTONDBLCLK = 0x0209;

        private readonly string _settingsPath = Path.Combine(Application.StartupPath, "settings.json");
        private readonly List<RecordedAction> _recordedActions = new();
        private readonly LowLevelKeyboardProc _keyboardHookProc;
        private readonly LowLevelMouseProc _mouseHookProc;
        private CancellationTokenSource? _replayCancellationTokenSource;
        private IntPtr _keyboardHook = IntPtr.Zero;
        private IntPtr _mouseHook = IntPtr.Zero;
        private bool _allowClose;
        private bool _isRecording;
        private bool _isReplayingSequence;
        private bool _captureNextActionForEdit;
        private int _editCaptureIndex = -1;
        private int _pendingPlaceholderIndex = -1;
        private int _currentReplayStepIndex = -1;
        private Keys? _ignoredCaptureKey;
        private bool _ignoredCaptureKeyConsumed;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public nuint dwExtraInfo;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, nuint dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, nuint dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, Delegate lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string? lpModuleName);

        public AutoClickerForm()
        {
            InitializeComponent();
            MouseDown += AutoClickerForm_MouseDown;
            _keyboardHookProc = KeyboardHookCallback;
            _mouseHookProc = MouseHookCallback;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            RegisterHotKey(Handle, ReplayHotkeyId, 0, (uint)Keys.F6);
            RegisterHotKey(Handle, AddPlaceholderHotkeyId, 0, (uint)Keys.F7);
            RegisterHotKey(Handle, ToggleRecordingHotkeyId, 0, (uint)Keys.F8);
            InstallCaptureHooks();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            UnregisterHotKey(Handle, ReplayHotkeyId);
            UnregisterHotKey(Handle, AddPlaceholderHotkeyId);
            UnregisterHotKey(Handle, ToggleRecordingHotkeyId);
            UninstallCaptureHooks();
            _replayCancellationTokenSource?.Cancel();
            _replayCancellationTokenSource?.Dispose();
            base.OnHandleDestroyed(e);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                switch (m.WParam.ToInt32())
                {
                    case ReplayHotkeyId:
                        ToggleReplay();
                        return;
                    case AddPlaceholderHotkeyId:
                        TriggerStepCapture(Keys.F7);
                        return;
                    case ToggleRecordingHotkeyId:
                        ToggleRecording();
                        return;
                }
            }

            base.WndProc(ref m);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void AutoClickerForm_Load(object sender, EventArgs e)
        {
            LoadSettings();
            notifyIconApp.Icon = SystemIcons.Application;
            RefreshRecordedActionsList();
            UpdateReplayModeControls();
            UpdateUiState();
        }

        private void AutoClickerForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F6 || e.KeyCode == Keys.F7 || e.KeyCode == Keys.F8)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void AutoClickerForm_MouseDown(object? sender, MouseEventArgs e)
        {
            if (_captureNextActionForEdit)
            {
                var mouseButton = e.Button switch
                {
                    MouseButtons.Right => MouseButtonType.Right,
                    MouseButtons.Middle => MouseButtonType.Middle,
                    _ => MouseButtonType.Left
                };

                var clickMode = e.Clicks >= 2 ? ClickModeType.Double : ClickModeType.Single;
                UpdateSelectedStepFromMouse(Cursor.Position, mouseButton, clickMode);
                return;
            }

            if (_pendingPlaceholderIndex < 0)
            {
                return;
            }

            var pendingMouseButton = e.Button switch
            {
                MouseButtons.Right => MouseButtonType.Right,
                MouseButtons.Middle => MouseButtonType.Middle,
                _ => MouseButtonType.Left
            };

            var pendingClickMode = e.Clicks >= 2 ? ClickModeType.Double : ClickModeType.Single;
            FillPendingPlaceholderWithMouse(Cursor.Position, pendingMouseButton, pendingClickMode);
        }

        private void TriggerStepCapture(Keys? ignoredCaptureKey = null)
        {
            if (_captureNextActionForEdit || _pendingPlaceholderIndex >= 0)
            {
                return;
            }

            _ignoredCaptureKey = ignoredCaptureKey;
            _ignoredCaptureKeyConsumed = ignoredCaptureKey is null;

            if (_isRecording)
            {
                AddPlaceholderStep();
                return;
            }

            if (lstRecordedActions.SelectedIndex >= 0)
            {
                ArmCaptureForSelectedStep();
            }
        }

        private void UpdateSelectedStepFromMouse(Point position, MouseButtonType mouseButton, ClickModeType clickMode)
        {
            if (_editCaptureIndex < 0 || _editCaptureIndex >= _recordedActions.Count)
            {
                _captureNextActionForEdit = false;
                _editCaptureIndex = -1;
                _ignoredCaptureKey = null;
                _ignoredCaptureKeyConsumed = false;
                UpdateUiState();
                return;
            }

            var existing = _recordedActions[_editCaptureIndex];
            existing.IsPlaceholder = false;
            existing.ActionType = RecordedActionType.Mouse;
            existing.X = position.X;
            existing.Y = position.Y;
            existing.MouseButton = mouseButton;
            existing.ClickMode = clickMode;
            _recordedActions[_editCaptureIndex] = existing;
            _captureNextActionForEdit = false;
            var selectedIndex = _editCaptureIndex;
            _editCaptureIndex = -1;
            _ignoredCaptureKey = null;
            _ignoredCaptureKeyConsumed = false;
            RefreshRecordedActionsList();
            lstRecordedActions.SelectedIndex = selectedIndex;
            SaveSettings();
            UpdateUiState();
        }

        private void FillPendingPlaceholderWithMouse(Point position, MouseButtonType mouseButton, ClickModeType clickMode)
        {
            if (_pendingPlaceholderIndex < 0 || _pendingPlaceholderIndex >= _recordedActions.Count)
            {
                return;
            }

            var action = _recordedActions[_pendingPlaceholderIndex];
            action.IsPlaceholder = false;
            action.ActionType = RecordedActionType.Mouse;
            action.X = position.X;
            action.Y = position.Y;
            action.MouseButton = mouseButton;
            action.ClickMode = clickMode;
            _recordedActions[_pendingPlaceholderIndex] = action;
            var selectedIndex = _pendingPlaceholderIndex;
            _pendingPlaceholderIndex = -1;
            _ignoredCaptureKey = null;
            _ignoredCaptureKeyConsumed = false;
            RefreshRecordedActionsList();
            lstRecordedActions.SelectedIndex = selectedIndex;
            SaveSettings();
            UpdateUiState();
        }

        private void FillPendingPlaceholderWithKeyboard(Keys keyCode)
        {
            if (_pendingPlaceholderIndex < 0 || _pendingPlaceholderIndex >= _recordedActions.Count)
            {
                return;
            }

            var action = _recordedActions[_pendingPlaceholderIndex];
            action.IsPlaceholder = false;
            action.ActionType = RecordedActionType.Keyboard;
            action.KeyCode = keyCode;
            action.KeyAction = KeyActionType.KeyPress;
            _recordedActions[_pendingPlaceholderIndex] = action;
            var selectedIndex = _pendingPlaceholderIndex;
            _pendingPlaceholderIndex = -1;
            _ignoredCaptureKey = null;
            _ignoredCaptureKeyConsumed = false;
            RefreshRecordedActionsList();
            lstRecordedActions.SelectedIndex = selectedIndex;
            SaveSettings();
            UpdateUiState();
        }

        private void CaptureEditedKeyboardAction(Keys keyCode)
        {
            if (_editCaptureIndex < 0 || _editCaptureIndex >= _recordedActions.Count)
            {
                _captureNextActionForEdit = false;
                _editCaptureIndex = -1;
                _ignoredCaptureKey = null;
                _ignoredCaptureKeyConsumed = false;
                UpdateUiState();
                return;
            }

            var existing = _recordedActions[_editCaptureIndex];
            existing.IsPlaceholder = false;
            existing.ActionType = RecordedActionType.Keyboard;
            existing.KeyCode = keyCode;
            existing.KeyAction = KeyActionType.KeyPress;
            _recordedActions[_editCaptureIndex] = existing;
            _captureNextActionForEdit = false;
            var selectedIndex = _editCaptureIndex;
            _editCaptureIndex = -1;
            _ignoredCaptureKey = null;
            _ignoredCaptureKeyConsumed = false;
            RefreshRecordedActionsList();
            lstRecordedActions.SelectedIndex = selectedIndex;
            SaveSettings();
            UpdateUiState();
        }

        private void btnStartStop_Click(object sender, EventArgs e) => ToggleReplay();
        private void btnStartRecording_Click(object sender, EventArgs e) => StartRecording();
        private void btnStopRecording_Click(object sender, EventArgs e) => StopRecording();
        private void btnRecordStep_Click(object sender, EventArgs e) => TriggerStepCapture();
        private void btnEditStep_Click(object sender, EventArgs e) => EditSelectedStep();
        private void btnRemoveStep_Click(object sender, EventArgs e) => RemoveSelectedStep();

        private void btnClearSequence_Click(object sender, EventArgs e)
        {
            _recordedActions.Clear();
            _pendingPlaceholderIndex = -1;
            RefreshRecordedActionsList();
            SaveSettings();
            UpdateUiState();
        }

        private async void btnReplaySequence_Click(object sender, EventArgs e) => await ReplayRecordedSequenceAsync();
        private void lstRecordedActions_SelectedIndexChanged(object sender, EventArgs e) => UpdateUiState();
        private void lstRecordedActions_DoubleClick(object sender, EventArgs e) => EditSelectedStep();

        private void lstRecordedActions_MouseDown(object sender, MouseEventArgs e)
        {
            var index = lstRecordedActions.IndexFromPoint(e.Location);
            if (index >= 0)
            {
                lstRecordedActions.SelectedIndex = index;
            }
        }

        private void editStepMenuItem_Click(object sender, EventArgs e) => EditSelectedStep();
        private void removeStepMenuItem_Click(object sender, EventArgs e) => RemoveSelectedStep();
        private void trayOpenMenuItem_Click(object sender, EventArgs e) => ShowFromTray();
        private void trayStartStopMenuItem_Click(object sender, EventArgs e) => ToggleReplay();

        private void trayExitMenuItem_Click(object sender, EventArgs e)
        {
            _allowClose = true;
            Close();
        }

        private void notifyIconApp_DoubleClick(object sender, EventArgs e) => ShowFromTray();
        private void clickTimer_Tick(object sender, EventArgs e) { }
        private void ReplayModeChanged(object sender, EventArgs e) => UpdateReplayModeControls();

        private void AutoClickerForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_allowClose)
            {
                return;
            }

            if (chkMinimizeToTray.Checked)
            {
                e.Cancel = true;
                HideToTray();
            }
        }

        private void AutoClickerForm_Resize(object? sender, EventArgs e)
        {
            if (chkMinimizeToTray.Checked && WindowState == FormWindowState.Minimized)
            {
                HideToTray();
            }
        }

        private void ToggleRecording()
        {
            if (_isRecording)
            {
                StopRecording();
            }
            else
            {
                StartRecording();
            }
        }

        private void StartRecording()
        {
            if (_isReplayingSequence)
            {
                StopReplay();
            }

            _isRecording = true;
            UpdateReplayModeControls();
            UpdateUiState();
        }

        private void StopRecording()
        {
            _isRecording = false;
            _pendingPlaceholderIndex = -1;
            SaveSettings();
            UpdateReplayModeControls();
            UpdateUiState();
        }

        private void ToggleReplay()
        {
            if (_isReplayingSequence)
            {
                StopReplay();
                UpdateUiState();
                return;
            }

            _ = ReplayRecordedSequenceAsync();
        }

        private void AddPlaceholderStep()
        {
            if (!_isRecording || _pendingPlaceholderIndex >= 0)
            {
                return;
            }

            var action = new RecordedAction
            {
                DelayMilliseconds = Math.Max(MinDelayMilliseconds, (int)nudDefaultDelay.Value),
                IsPlaceholder = true
            };

            _recordedActions.Add(action);
            _pendingPlaceholderIndex = _recordedActions.Count - 1;
            RefreshRecordedActionsList();
            lstRecordedActions.SelectedIndex = _pendingPlaceholderIndex;
            SaveSettings();
            UpdateUiState();
        }

        private void ArmCaptureForSelectedStep()
        {
            if (lstRecordedActions.SelectedIndex < 0)
            {
                return;
            }

            _captureNextActionForEdit = true;
            _editCaptureIndex = lstRecordedActions.SelectedIndex;
            UpdateUiState();
        }

        private async Task ReplayRecordedSequenceAsync()
        {
            var replayActions = _recordedActions
                .Where(action => !action.IsPlaceholder)
                .Select(action => new RecordedAction
                {
                    DelayMilliseconds = Math.Max(MinDelayMilliseconds, action.DelayMilliseconds),
                    ActionType = action.ActionType,
                    X = action.X,
                    Y = action.Y,
                    MouseButton = action.MouseButton,
                    ClickMode = action.ClickMode,
                    KeyCode = action.KeyCode,
                    KeyAction = action.KeyAction,
                    IsPlaceholder = false
                })
                .ToList();

            if (replayActions.Count == 0)
            {
                MessageBox.Show(this, "There are no completed steps to replay.", "No sequence", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_isReplayingSequence)
            {
                return;
            }

            _replayCancellationTokenSource?.Dispose();
            _replayCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _replayCancellationTokenSource.Token;

            _isReplayingSequence = true;
            _isRecording = false;
            _pendingPlaceholderIndex = -1;
            _currentReplayStepIndex = -1;
            UpdateReplayModeControls();
            UpdateUiState();
            SaveSettings();

            try
            {
                var loopCount = rbReplayUntilStopped.Checked ? int.MaxValue : rbReplayFixed.Checked ? (int)nudReplayCount.Value : 1;

                for (var loopIndex = 0; loopIndex < loopCount; loopIndex++)
                {
                    foreach (var pair in replayActions.Select((action, index) => new { action, index }))
                    {
                        _currentReplayStepIndex = indexOfCompletedStep(pair.action);
                        if (_currentReplayStepIndex >= 0)
                        {
                            lstRecordedActions.SelectedIndex = _currentReplayStepIndex;
                        }
                        UpdateUiState();

                        cancellationToken.ThrowIfCancellationRequested();
                        await Task.Delay(pair.action.DelayMilliseconds, cancellationToken);
                        cancellationToken.ThrowIfCancellationRequested();
                        ExecuteRecordedAction(pair.action);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Replay failed: {ex.Message}", "Replay error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _currentReplayStepIndex = -1;
                _isReplayingSequence = false;
                _replayCancellationTokenSource?.Dispose();
                _replayCancellationTokenSource = null;
                UpdateReplayModeControls();
                UpdateUiState();
            }

            int indexOfCompletedStep(RecordedAction action)
            {
                for (var i = 0; i < _recordedActions.Count; i++)
                {
                    var candidate = _recordedActions[i];
                    if (!candidate.IsPlaceholder
                        && candidate.ActionType == action.ActionType
                        && candidate.DelayMilliseconds == action.DelayMilliseconds
                        && candidate.X == action.X
                        && candidate.Y == action.Y
                        && candidate.MouseButton == action.MouseButton
                        && candidate.ClickMode == action.ClickMode
                        && candidate.KeyCode == action.KeyCode
                        && candidate.KeyAction == action.KeyAction)
                    {
                        return i;
                    }
                }

                return -1;
            }
        }

        private void StopReplay()
        {
            _isReplayingSequence = false;
            _replayCancellationTokenSource?.Cancel();
            _replayCancellationTokenSource?.Dispose();
            _replayCancellationTokenSource = null;
            _currentReplayStepIndex = -1;
        }

        private void ExecuteRecordedAction(RecordedAction action)
        {
            if (action.ActionType == RecordedActionType.Keyboard)
            {
                ExecuteKeyboardAction(action);
                return;
            }

            ExecuteMouseAction(action);
        }

        private void ExecuteMouseAction(RecordedAction action)
        {
            Cursor.Position = new Point(action.X, action.Y);
            ExecuteMouseClick(action.MouseButton);

            if (action.ClickMode == ClickModeType.Double)
            {
                Thread.Sleep(80);
                ExecuteMouseClick(action.MouseButton);
            }
        }

        private void ExecuteKeyboardAction(RecordedAction action)
        {
            if (action.KeyCode == Keys.None)
            {
                return;
            }

            var key = (byte)action.KeyCode;
            switch (action.KeyAction)
            {
                case KeyActionType.KeyDown:
                    keybd_event(key, 0, 0, 0);
                    break;
                case KeyActionType.KeyUp:
                    keybd_event(key, 0, KEYEVENTF_KEYUP, 0);
                    break;
                default:
                    keybd_event(key, 0, 0, 0);
                    keybd_event(key, 0, KEYEVENTF_KEYUP, 0);
                    break;
            }
        }

        private void ExecuteMouseClick(MouseButtonType mouseButton)
        {
            switch (mouseButton)
            {
                case MouseButtonType.Right:
                    mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                    mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                    break;
                case MouseButtonType.Middle:
                    mouse_event(MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, 0);
                    mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
                    break;
                default:
                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    break;
            }
        }

        private void EditSelectedStep()
        {
            if (lstRecordedActions.SelectedIndex < 0)
            {
                return;
            }

            var index = lstRecordedActions.SelectedIndex;
            using var dialog = new StepEditForm(_recordedActions[index]);
            var result = dialog.ShowDialog(this);
            if (result == DialogResult.Retry && dialog.CaptureRequested)
            {
                _captureNextActionForEdit = true;
                _editCaptureIndex = index;
                UpdateUiState();
                return;
            }

            if (result != DialogResult.OK)
            {
                return;
            }

            _recordedActions[index] = dialog.EditedAction;
            RefreshRecordedActionsList();
            lstRecordedActions.SelectedIndex = index;
            SaveSettings();
            UpdateUiState();
        }

        private void RemoveSelectedStep()
        {
            if (lstRecordedActions.SelectedIndex < 0)
            {
                return;
            }

            var index = lstRecordedActions.SelectedIndex;
            _recordedActions.RemoveAt(index);
            if (_pendingPlaceholderIndex == index)
            {
                _pendingPlaceholderIndex = -1;
            }
            else if (_pendingPlaceholderIndex > index)
            {
                _pendingPlaceholderIndex--;
            }

            RefreshRecordedActionsList();

            if (_recordedActions.Count > 0)
            {
                lstRecordedActions.SelectedIndex = Math.Min(index, _recordedActions.Count - 1);
            }

            SaveSettings();
            UpdateUiState();
        }

        private void RefreshRecordedActionsList()
        {
            var selectedIndex = lstRecordedActions.SelectedIndex;
            lstRecordedActions.Items.Clear();

            for (var index = 0; index < _recordedActions.Count; index++)
            {
                var action = _recordedActions[index];
                var text = action.IsPlaceholder
                    ? $"{index + 1}. [Pending] wait {action.DelayMilliseconds} ms, capture next action"
                    : action.ActionType == RecordedActionType.Keyboard
                        ? $"{index + 1}. wait {action.DelayMilliseconds} ms, key {action.KeyCode} ({action.KeyAction})"
                        : $"{index + 1}. wait {action.DelayMilliseconds} ms, {action.MouseButton} {action.ClickMode} at ({action.X}, {action.Y})";
                lstRecordedActions.Items.Add(text);
            }

            if (_recordedActions.Count > 0)
            {
                lstRecordedActions.SelectedIndex = selectedIndex >= 0
                    ? Math.Min(selectedIndex, _recordedActions.Count - 1)
                    : 0;
            }
        }

        private void UpdateReplayModeControls()
        {
            var fixedEnabled = rbReplayFixed.Checked && !_isRecording && !_isReplayingSequence;
            nudReplayCount.Enabled = fixedEnabled;
            lblReplayCount.Enabled = fixedEnabled;
            rbReplayOnce.Enabled = !_isRecording && !_isReplayingSequence;
            rbReplayFixed.Enabled = !_isRecording && !_isReplayingSequence;
            rbReplayUntilStopped.Enabled = !_isRecording && !_isReplayingSequence;
            nudDefaultDelay.Enabled = !_isReplayingSequence;
            lblDefaultDelay.Enabled = !_isReplayingSequence;
        }

        private void UpdateUiState()
        {
            var hasSelection = lstRecordedActions.SelectedIndex >= 0;
            btnStartStop.Text = _isReplayingSequence ? "Stop replay (F6)" : "Replay sequence (F6)";
            lblStatus.Text = _captureNextActionForEdit
                ? $"Step {_editCaptureIndex + 1}: perform next action to update it"
                : _pendingPlaceholderIndex >= 0
                    ? $"Step {_pendingPlaceholderIndex + 1} pending: perform next mouse or key action"
                    : _isRecording
                        ? $"Recording armed ({_recordedActions.Count} steps)"
                        : _isReplayingSequence
                            ? _currentReplayStepIndex >= 0
                                ? $"Replaying step {_currentReplayStepIndex + 1}/{_recordedActions.Count}"
                                : "Replaying sequence"
                            : "Stopped";
            lblStatus.ForeColor = _isRecording || _isReplayingSequence || _captureNextActionForEdit || _pendingPlaceholderIndex >= 0 ? Color.DarkGreen : Color.DarkRed;
            trayStartStopMenuItem.Text = _isReplayingSequence ? "Stop replay" : "Replay";
            notifyIconApp.Text = _captureNextActionForEdit
                ? "Auto Mouse Click - Updating selected step"
                : _pendingPlaceholderIndex >= 0
                    ? "Auto Mouse Click - Waiting for next action"
                    : _isRecording
                        ? "Auto Mouse Click - Recording armed"
                        : _isReplayingSequence
                            ? "Auto Mouse Click - Replaying sequence"
                            : "Auto Mouse Click - Stopped";
            btnReplaySequence.Enabled = _recordedActions.Any(x => !x.IsPlaceholder) && !_isReplayingSequence && !_captureNextActionForEdit && _pendingPlaceholderIndex < 0;
            btnRecordStep.Enabled = (_isRecording || (!_isRecording && hasSelection)) && !_captureNextActionForEdit && _pendingPlaceholderIndex < 0;
            btnRecordStep.Text = !_isRecording && hasSelection ? "Capture into selected (F7)" : "Add pending step (F7)";
            btnClearSequence.Enabled = _recordedActions.Count > 0 && !_isRecording && !_isReplayingSequence && !_captureNextActionForEdit && _pendingPlaceholderIndex < 0;
            btnStartRecording.Enabled = !_isRecording && !_isReplayingSequence && !_captureNextActionForEdit;
            btnStopRecording.Enabled = _isRecording && !_captureNextActionForEdit;
            btnEditStep.Enabled = hasSelection && !_isRecording && !_isReplayingSequence && !_captureNextActionForEdit && _pendingPlaceholderIndex < 0;
            btnRemoveStep.Enabled = hasSelection && !_isRecording && !_isReplayingSequence && !_captureNextActionForEdit;
            chkMinimizeToTray.Enabled = !_isReplayingSequence && !_captureNextActionForEdit;
        }

        private void HideToTray()
        {
            notifyIconApp.Visible = true;
            Hide();
            ShowInTaskbar = false;
        }

        private void ShowFromTray()
        {
            Show();
            WindowState = FormWindowState.Normal;
            ShowInTaskbar = true;
            notifyIconApp.Visible = false;
            Activate();
        }

        private void LoadSettings()
        {
            if (!File.Exists(_settingsPath))
            {
                return;
            }

            try
            {
                var settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_settingsPath));
                if (settings is null)
                {
                    return;
                }

                chkMinimizeToTray.Checked = settings.MinimizeToTray;
                nudDefaultDelay.Value = Math.Min(nudDefaultDelay.Maximum, Math.Max(nudDefaultDelay.Minimum, settings.DefaultStepDelayMilliseconds));
                rbReplayUntilStopped.Checked = settings.RepeatUntilStopped;
                rbReplayFixed.Checked = !settings.RepeatUntilStopped && settings.RepeatCount > 1;
                rbReplayOnce.Checked = !settings.RepeatUntilStopped && settings.RepeatCount <= 1;
                nudReplayCount.Value = Math.Min(nudReplayCount.Maximum, Math.Max(nudReplayCount.Minimum, settings.RepeatCount));
                _recordedActions.Clear();
                _recordedActions.AddRange(settings.RecordedActions ?? new List<RecordedAction>());
            }
            catch
            {
            }

            UpdateReplayModeControls();
        }

        private void SaveSettings()
        {
            var settings = new AppSettings
            {
                DefaultStepDelayMilliseconds = (int)nudDefaultDelay.Value,
                RepeatUntilStopped = rbReplayUntilStopped.Checked,
                RepeatCount = rbReplayOnce.Checked ? 1 : (int)nudReplayCount.Value,
                MinimizeToTray = chkMinimizeToTray.Checked,
                RecordedActions = _recordedActions.ToList()
            };

            File.WriteAllText(_settingsPath, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
        }

        private void InstallCaptureHooks()
        {
            if (_keyboardHook != IntPtr.Zero || _mouseHook != IntPtr.Zero)
            {
                return;
            }

            using var process = System.Diagnostics.Process.GetCurrentProcess();
            using var module = process.MainModule;
            var moduleHandle = GetModuleHandle(module?.ModuleName);
            _keyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardHookProc, moduleHandle, 0);
            _mouseHook = SetWindowsHookEx(WH_MOUSE_LL, _mouseHookProc, moduleHandle, 0);
        }

        private void UninstallCaptureHooks()
        {
            if (_keyboardHook != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_keyboardHook);
                _keyboardHook = IntPtr.Zero;
            }

            if (_mouseHook != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_mouseHook);
                _mouseHook = IntPtr.Zero;
            }
        }

        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                var keyCode = (Keys)Marshal.ReadInt32(lParam);
                if (_ignoredCaptureKey.HasValue && keyCode == _ignoredCaptureKey.Value && !_ignoredCaptureKeyConsumed)
                {
                    _ignoredCaptureKeyConsumed = true;
                    return CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
                }

                if (_captureNextActionForEdit || _pendingPlaceholderIndex >= 0)
                {
                    BeginInvoke(() =>
                    {
                        if (_captureNextActionForEdit)
                        {
                            CaptureEditedKeyboardAction(keyCode);
                        }
                        else if (_pendingPlaceholderIndex >= 0)
                        {
                            FillPendingPlaceholderWithKeyboard(keyCode);
                        }
                    });

                    return (IntPtr)1;
                }
            }

            return CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (_captureNextActionForEdit || _pendingPlaceholderIndex >= 0))
            {
                var message = wParam.ToInt32();
                if (TryGetMouseCapture(message, lParam, out var position, out var mouseButton, out var clickMode))
                {
                    BeginInvoke(() =>
                    {
                        if (_captureNextActionForEdit)
                        {
                            UpdateSelectedStepFromMouse(position, mouseButton, clickMode);
                        }
                        else if (_pendingPlaceholderIndex >= 0)
                        {
                            FillPendingPlaceholderWithMouse(position, mouseButton, clickMode);
                        }
                    });

                    return (IntPtr)1;
                }
            }

            return CallNextHookEx(_mouseHook, nCode, wParam, lParam);
        }

        private static bool TryGetMouseCapture(int message, IntPtr lParam, out Point position, out MouseButtonType mouseButton, out ClickModeType clickMode)
        {
            position = Point.Empty;
            mouseButton = MouseButtonType.Left;
            clickMode = ClickModeType.Single;

            if (lParam == IntPtr.Zero)
            {
                return false;
            }

            var hookData = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
            position = new Point(hookData.pt.X, hookData.pt.Y);

            switch (message)
            {
                case WM_LBUTTONDOWN:
                    mouseButton = MouseButtonType.Left;
                    clickMode = ClickModeType.Single;
                    return true;
                case WM_RBUTTONDOWN:
                    mouseButton = MouseButtonType.Right;
                    clickMode = ClickModeType.Single;
                    return true;
                case WM_MBUTTONDOWN:
                    mouseButton = MouseButtonType.Middle;
                    clickMode = ClickModeType.Single;
                    return true;
                case WM_LBUTTONDBLCLK:
                    mouseButton = MouseButtonType.Left;
                    clickMode = ClickModeType.Double;
                    return true;
                case WM_RBUTTONDBLCLK:
                    mouseButton = MouseButtonType.Right;
                    clickMode = ClickModeType.Double;
                    return true;
                case WM_MBUTTONDBLCLK:
                    mouseButton = MouseButtonType.Middle;
                    clickMode = ClickModeType.Double;
                    return true;
                default:
                    return false;
            }
        }
    }
}

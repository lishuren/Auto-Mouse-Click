using System.Runtime.InteropServices;
using System.Text.Json;

namespace AutoMouseClick
{
    public partial class AutoClickerForm : Form
    {
        private const int ReplayHotkeyId = 1001;
        private const int CaptureStepHotkeyId = 1002;
        private const int ToggleRecordingHotkeyId = 1003;
        private const int MinDelayMilliseconds = 1;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const int WM_HOTKEY = 0x0312;

        private readonly string _settingsPath = Path.Combine(Application.StartupPath, "settings.json");
        private readonly List<RecordedAction> _recordedActions = new();
        private CancellationTokenSource? _replayCancellationTokenSource;
        private bool _allowClose;
        private bool _isRecording;
        private bool _isReplayingSequence;
        private int _currentReplayStepIndex = -1;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, nuint dwExtraInfo);

        public AutoClickerForm()
        {
            InitializeComponent();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            RegisterHotKey(Handle, ReplayHotkeyId, 0, (uint)Keys.F6);
            RegisterHotKey(Handle, CaptureStepHotkeyId, 0, (uint)Keys.F7);
            RegisterHotKey(Handle, ToggleRecordingHotkeyId, 0, (uint)Keys.F8);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            UnregisterHotKey(Handle, ReplayHotkeyId);
            UnregisterHotKey(Handle, CaptureStepHotkeyId);
            UnregisterHotKey(Handle, ToggleRecordingHotkeyId);
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
                    case CaptureStepHotkeyId:
                        CaptureRecordedStep();
                        return;
                    case ToggleRecordingHotkeyId:
                        ToggleRecording();
                        return;
                }
            }

            base.WndProc(ref m);
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
            if (e.KeyCode == Keys.F6)
            {
                ToggleReplay();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F7)
            {
                CaptureRecordedStep();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F8)
            {
                ToggleRecording();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void AutoClickerForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized && chkMinimizeToTray.Checked)
            {
                HideToTray();
            }
        }

        private void AutoClickerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_allowClose && chkMinimizeToTray.Checked && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                HideToTray();
                return;
            }

            StopReplay();
            SaveSettings();
            notifyIconApp.Visible = false;
        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            ToggleReplay();
        }

        private void btnStartRecording_Click(object sender, EventArgs e)
        {
            StartRecording();
        }

        private void btnStopRecording_Click(object sender, EventArgs e)
        {
            StopRecording();
        }

        private void btnRecordStep_Click(object sender, EventArgs e)
        {
            CaptureRecordedStep();
        }

        private void btnEditStep_Click(object sender, EventArgs e)
        {
            EditSelectedStep();
        }

        private void btnRemoveStep_Click(object sender, EventArgs e)
        {
            RemoveSelectedStep();
        }

        private void btnClearSequence_Click(object sender, EventArgs e)
        {
            _recordedActions.Clear();
            RefreshRecordedActionsList();
            SaveSettings();
            UpdateUiState();
        }

        private async void btnReplaySequence_Click(object sender, EventArgs e)
        {
            await ReplayRecordedSequenceAsync();
        }

        private void lstRecordedActions_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateUiState();
        }

        private void lstRecordedActions_DoubleClick(object sender, EventArgs e)
        {
            EditSelectedStep();
        }

        private void lstRecordedActions_MouseDown(object sender, MouseEventArgs e)
        {
            var index = lstRecordedActions.IndexFromPoint(e.Location);
            if (index >= 0)
            {
                lstRecordedActions.SelectedIndex = index;
            }
        }

        private void editStepMenuItem_Click(object sender, EventArgs e)
        {
            EditSelectedStep();
        }

        private void removeStepMenuItem_Click(object sender, EventArgs e)
        {
            RemoveSelectedStep();
        }

        private void trayOpenMenuItem_Click(object sender, EventArgs e)
        {
            ShowFromTray();
        }

        private void trayStartStopMenuItem_Click(object sender, EventArgs e)
        {
            ToggleReplay();
        }

        private void trayExitMenuItem_Click(object sender, EventArgs e)
        {
            _allowClose = true;
            Close();
        }

        private void notifyIconApp_DoubleClick(object sender, EventArgs e)
        {
            ShowFromTray();
        }

        private void clickTimer_Tick(object sender, EventArgs e)
        {
        }

        private void ReplayModeChanged(object sender, EventArgs e)
        {
            UpdateReplayModeControls();
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
            SaveSettings();
            UpdateReplayModeControls();
            UpdateUiState();
        }

        private void ToggleReplay()
        {
            if (_isRecording)
            {
                CaptureRecordedStepAndClick();
                return;
            }

            if (_isReplayingSequence)
            {
                StopReplay();
                UpdateUiState();
                return;
            }

            _ = ReplayRecordedSequenceAsync();
        }

        private void CaptureRecordedStep()
        {
            if (!_isRecording)
            {
                return;
            }

            var delay = Math.Max(MinDelayMilliseconds, (int)nudDefaultDelay.Value);
            var targetPosition = Cursor.Position;
            AddRecordedAction(targetPosition, MouseButtonType.Left, ClickModeType.Single, delay);
        }

        private void CaptureRecordedStepAndClick()
        {
            if (!_isRecording)
            {
                return;
            }

            var delay = Math.Max(MinDelayMilliseconds, (int)nudDefaultDelay.Value);
            var targetPosition = Cursor.Position;
            AddRecordedAction(targetPosition, MouseButtonType.Left, ClickModeType.Single, delay);
            ExecuteClick(targetPosition, MouseButtonType.Left, ClickModeType.Single);
        }

        private void AddRecordedAction(Point position, MouseButtonType mouseButton, ClickModeType clickMode, int delayMilliseconds)
        {
            _recordedActions.Add(new RecordedAction
            {
                DelayMilliseconds = Math.Max(MinDelayMilliseconds, delayMilliseconds),
                X = position.X,
                Y = position.Y,
                MouseButton = mouseButton,
                ClickMode = clickMode
            });

            RefreshRecordedActionsList();
            lstRecordedActions.SelectedIndex = _recordedActions.Count - 1;
            SaveSettings();
            UpdateUiState();
        }

        private async Task ReplayRecordedSequenceAsync()
        {
            if (_recordedActions.Count == 0)
            {
                MessageBox.Show(this, "There are no recorded steps to replay.", "No sequence", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_isReplayingSequence)
            {
                return;
            }

            var replayActions = _recordedActions
                .Select(action => new RecordedAction
                {
                    DelayMilliseconds = Math.Max(MinDelayMilliseconds, action.DelayMilliseconds),
                    X = action.X,
                    Y = action.Y,
                    MouseButton = action.MouseButton,
                    ClickMode = action.ClickMode
                })
                .ToList();

            _replayCancellationTokenSource?.Dispose();
            _replayCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _replayCancellationTokenSource.Token;

            _isReplayingSequence = true;
            _isRecording = false;
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
                        _currentReplayStepIndex = pair.index;
                        lstRecordedActions.SelectedIndex = pair.index;
                        UpdateUiState();

                        cancellationToken.ThrowIfCancellationRequested();
                        await Task.Delay(pair.action.DelayMilliseconds, cancellationToken);
                        cancellationToken.ThrowIfCancellationRequested();
                        ExecuteClick(new Point(pair.action.X, pair.action.Y), pair.action.MouseButton, pair.action.ClickMode);
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
        }

        private void StopReplay()
        {
            _isReplayingSequence = false;
            _replayCancellationTokenSource?.Cancel();
            _replayCancellationTokenSource?.Dispose();
            _replayCancellationTokenSource = null;
            _currentReplayStepIndex = -1;
        }

        private void ExecuteClick(Point targetPosition, MouseButtonType mouseButton, ClickModeType clickMode)
        {
            Cursor.Position = targetPosition;
            ExecuteMouseClick(mouseButton);

            if (clickMode == ClickModeType.Double)
            {
                Thread.Sleep(80);
                ExecuteMouseClick(mouseButton);
            }
        }

        private void ExecuteMouseClick(MouseButtonType mouseButton)
        {
            if (mouseButton == MouseButtonType.Left)
            {
                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            }
            else
            {
                mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
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
            if (dialog.ShowDialog(this) != DialogResult.OK)
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
                lstRecordedActions.Items.Add($"{index + 1}. wait {action.DelayMilliseconds} ms, {action.MouseButton} {action.ClickMode} at ({action.X}, {action.Y})");
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
            lblStatus.Text = _isRecording
                ? $"Recording armed ({_recordedActions.Count} steps)"
                : _isReplayingSequence
                    ? _currentReplayStepIndex >= 0
                        ? $"Replaying step {_currentReplayStepIndex + 1}/{_recordedActions.Count}"
                        : "Replaying sequence"
                    : "Stopped";
            lblStatus.ForeColor = _isRecording || _isReplayingSequence ? Color.DarkGreen : Color.DarkRed;
            trayStartStopMenuItem.Text = _isReplayingSequence ? "Stop replay" : "Replay";
            notifyIconApp.Text = _isRecording
                ? "Auto Mouse Click - Recording armed"
                : _isReplayingSequence
                    ? "Auto Mouse Click - Replaying sequence"
                    : "Auto Mouse Click - Stopped";
            btnReplaySequence.Enabled = _recordedActions.Count > 0 && !_isReplayingSequence;
            btnRecordStep.Enabled = _isRecording;
            btnClearSequence.Enabled = _recordedActions.Count > 0 && !_isRecording && !_isReplayingSequence;
            btnStartRecording.Enabled = !_isRecording && !_isReplayingSequence;
            btnStopRecording.Enabled = _isRecording;
            btnEditStep.Enabled = hasSelection && !_isRecording && !_isReplayingSequence;
            btnRemoveStep.Enabled = hasSelection && !_isRecording && !_isReplayingSequence;
            chkMinimizeToTray.Enabled = !_isReplayingSequence;
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
    }
}

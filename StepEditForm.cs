namespace AutoMouseClick;

internal sealed class StepEditForm : Form
{
    private readonly NumericUpDown _nudDelay;
    private readonly NumericUpDown _nudX;
    private readonly NumericUpDown _nudY;
    private readonly ComboBox _cmbActionType;
    private readonly ComboBox _cmbButton;
    private readonly ComboBox _cmbClickMode;
    private readonly ComboBox _cmbKeyAction;
    private readonly TextBox _txtKey;
    private readonly Button _btnCapture;

    public RecordedAction EditedAction { get; private set; }
    public bool CaptureRequested { get; private set; }

    public StepEditForm(RecordedAction action)
    {
        Text = "Edit Step";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(320, 320);

        Controls.Add(new Label { Left = 20, Top = 20, Text = "Delay (ms)", AutoSize = true });
        _nudDelay = new NumericUpDown { Left = 140, Top = 18, Width = 140, Minimum = 1, Maximum = 600000, Value = action.DelayMilliseconds };
        Controls.Add(_nudDelay);

        Controls.Add(new Label { Left = 20, Top = 55, Text = "Action type", AutoSize = true });
        _cmbActionType = new ComboBox { Left = 140, Top = 53, Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
        _cmbActionType.Items.AddRange(Enum.GetNames<RecordedActionType>());
        _cmbActionType.SelectedItem = action.ActionType.ToString();
        _cmbActionType.SelectedIndexChanged += (_, _) => UpdateControlState();
        Controls.Add(_cmbActionType);

        Controls.Add(new Label { Left = 20, Top = 90, Text = "X", AutoSize = true });
        _nudX = new NumericUpDown { Left = 140, Top = 88, Width = 140, Minimum = 0, Maximum = 10000, Value = action.X };
        Controls.Add(_nudX);

        Controls.Add(new Label { Left = 20, Top = 125, Text = "Y", AutoSize = true });
        _nudY = new NumericUpDown { Left = 140, Top = 123, Width = 140, Minimum = 0, Maximum = 10000, Value = action.Y };
        Controls.Add(_nudY);

        Controls.Add(new Label { Left = 20, Top = 160, Text = "Mouse button", AutoSize = true });
        _cmbButton = new ComboBox { Left = 140, Top = 158, Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
        _cmbButton.Items.AddRange(Enum.GetNames<MouseButtonType>());
        _cmbButton.SelectedItem = action.MouseButton.ToString();
        Controls.Add(_cmbButton);

        Controls.Add(new Label { Left = 20, Top = 195, Text = "Click mode", AutoSize = true });
        _cmbClickMode = new ComboBox { Left = 140, Top = 193, Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
        _cmbClickMode.Items.AddRange(Enum.GetNames<ClickModeType>());
        _cmbClickMode.SelectedItem = action.ClickMode.ToString();
        Controls.Add(_cmbClickMode);

        Controls.Add(new Label { Left = 20, Top = 230, Text = "Key", AutoSize = true });
        _txtKey = new TextBox { Left = 140, Top = 228, Width = 140, Text = action.KeyCode == Keys.None ? string.Empty : action.KeyCode.ToString(), ReadOnly = true };
        Controls.Add(_txtKey);

        Controls.Add(new Label { Left = 20, Top = 265, Text = "Key action", AutoSize = true });
        _cmbKeyAction = new ComboBox { Left = 140, Top = 263, Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
        _cmbKeyAction.Items.AddRange(Enum.GetNames<KeyActionType>());
        _cmbKeyAction.SelectedItem = action.KeyAction.ToString();
        Controls.Add(_cmbKeyAction);

        _btnCapture = new Button { Text = "Capture next action", Left = 20, Top = 292, Width = 130 };
        Controls.Add(_btnCapture);

        var btnOk = new Button { Text = "OK", Left = 165, Width = 55, Top = 292, DialogResult = DialogResult.OK };
        var btnCancel = new Button { Text = "Cancel", Left = 225, Width = 55, Top = 292, DialogResult = DialogResult.Cancel };
        Controls.Add(btnOk);
        Controls.Add(btnCancel);

        AcceptButton = btnOk;
        CancelButton = btnCancel;

        EditedAction = action;

        _btnCapture.Click += (_, _) =>
        {
            CaptureRequested = true;
            DialogResult = DialogResult.Retry;
            Close();
        };

        btnOk.Click += (_, _) =>
        {
            EditedAction = new RecordedAction
            {
                DelayMilliseconds = (int)_nudDelay.Value,
                ActionType = Enum.Parse<RecordedActionType>(_cmbActionType.SelectedItem!.ToString()!),
                X = (int)_nudX.Value,
                Y = (int)_nudY.Value,
                MouseButton = Enum.Parse<MouseButtonType>(_cmbButton.SelectedItem!.ToString()!),
                ClickMode = Enum.Parse<ClickModeType>(_cmbClickMode.SelectedItem!.ToString()!),
                KeyCode = string.IsNullOrWhiteSpace(_txtKey.Text) ? Keys.None : Enum.Parse<Keys>(_txtKey.Text),
                KeyAction = Enum.Parse<KeyActionType>(_cmbKeyAction.SelectedItem!.ToString()!)
            };
        };

        UpdateControlState();
    }

    public void ApplyCapturedAction(RecordedAction action)
    {
        _cmbActionType.SelectedItem = action.ActionType.ToString();
        _nudX.Value = action.X;
        _nudY.Value = action.Y;
        _cmbButton.SelectedItem = action.MouseButton.ToString();
        _cmbClickMode.SelectedItem = action.ClickMode.ToString();
        _txtKey.Text = action.KeyCode == Keys.None ? string.Empty : action.KeyCode.ToString();
        _cmbKeyAction.SelectedItem = action.KeyAction.ToString();
        UpdateControlState();
    }

    private void UpdateControlState()
    {
        var isMouse = (_cmbActionType.SelectedItem?.ToString() ?? string.Empty) == nameof(RecordedActionType.Mouse);
        _nudX.Enabled = isMouse;
        _nudY.Enabled = isMouse;
        _cmbButton.Enabled = isMouse;
        _cmbClickMode.Enabled = isMouse;
        _txtKey.Enabled = !isMouse;
        _cmbKeyAction.Enabled = !isMouse;
    }
}

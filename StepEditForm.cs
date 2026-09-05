namespace AutoMouseClick;

internal sealed class StepEditForm : Form
{
    private readonly NumericUpDown _nudDelay;
    private readonly NumericUpDown _nudX;
    private readonly NumericUpDown _nudY;
    private readonly ComboBox _cmbButton;
    private readonly ComboBox _cmbClickMode;

    public RecordedAction EditedAction { get; private set; }

    public StepEditForm(RecordedAction action)
    {
        Text = "Edit Step";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(280, 230);

        Controls.Add(new Label { Left = 20, Top = 20, Text = "Delay (ms)", AutoSize = true });
        _nudDelay = new NumericUpDown { Left = 120, Top = 18, Width = 120, Minimum = 1, Maximum = 600000, Value = action.DelayMilliseconds };
        Controls.Add(_nudDelay);

        Controls.Add(new Label { Left = 20, Top = 55, Text = "X", AutoSize = true });
        _nudX = new NumericUpDown { Left = 120, Top = 53, Width = 120, Minimum = 0, Maximum = 10000, Value = action.X };
        Controls.Add(_nudX);

        Controls.Add(new Label { Left = 20, Top = 90, Text = "Y", AutoSize = true });
        _nudY = new NumericUpDown { Left = 120, Top = 88, Width = 120, Minimum = 0, Maximum = 10000, Value = action.Y };
        Controls.Add(_nudY);

        Controls.Add(new Label { Left = 20, Top = 125, Text = "Button", AutoSize = true });
        _cmbButton = new ComboBox { Left = 120, Top = 123, Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
        _cmbButton.Items.AddRange(Enum.GetNames<MouseButtonType>());
        _cmbButton.SelectedItem = action.MouseButton.ToString();
        Controls.Add(_cmbButton);

        Controls.Add(new Label { Left = 20, Top = 160, Text = "Click mode", AutoSize = true });
        _cmbClickMode = new ComboBox { Left = 120, Top = 158, Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
        _cmbClickMode.Items.AddRange(Enum.GetNames<ClickModeType>());
        _cmbClickMode.SelectedItem = action.ClickMode.ToString();
        Controls.Add(_cmbClickMode);

        var btnOk = new Button { Text = "OK", Left = 84, Width = 75, Top = 192, DialogResult = DialogResult.OK };
        var btnCancel = new Button { Text = "Cancel", Left = 165, Width = 75, Top = 192, DialogResult = DialogResult.Cancel };
        Controls.Add(btnOk);
        Controls.Add(btnCancel);

        AcceptButton = btnOk;
        CancelButton = btnCancel;

        EditedAction = action;

        btnOk.Click += (_, _) =>
        {
            EditedAction = new RecordedAction
            {
                DelayMilliseconds = (int)_nudDelay.Value,
                X = (int)_nudX.Value,
                Y = (int)_nudY.Value,
                MouseButton = Enum.Parse<MouseButtonType>(_cmbButton.SelectedItem!.ToString()!),
                ClickMode = Enum.Parse<ClickModeType>(_cmbClickMode.SelectedItem!.ToString()!)
            };
        };
    }
}

namespace AutoMouseClick
{
    partial class AutoClickerForm
    {
        private System.ComponentModel.IContainer components = null;

        private CheckBox chkMinimizeToTray;
        private Button btnStartStop;
        private Label lblStatusCaption;
        private Label lblStatus;
        private Label lblHotkey;
        private NotifyIcon notifyIconApp;
        private ContextMenuStrip trayMenu;
        private ToolStripMenuItem trayOpenMenuItem;
        private ToolStripMenuItem trayStartStopMenuItem;
        private ToolStripMenuItem trayExitMenuItem;
        private GroupBox grpSequence;
        private Button btnStartRecording;
        private Button btnStopRecording;
        private Button btnRecordStep;
        private Button btnEditStep;
        private Button btnRemoveStep;
        private Button btnClearSequence;
        private Button btnReplaySequence;
        private Label lblRecordingHelp;
        private Label lblRecordedActions;
        private ListBox lstRecordedActions;
        private System.Windows.Forms.Timer clickTimer;
        private Label lblDefaultDelay;
        private NumericUpDown nudDefaultDelay;
        private GroupBox grpReplayMode;
        private RadioButton rbReplayOnce;
        private RadioButton rbReplayFixed;
        private RadioButton rbReplayUntilStopped;
        private NumericUpDown nudReplayCount;
        private Label lblReplayCount;
        private ContextMenuStrip sequenceMenu;
        private ToolStripMenuItem editStepMenuItem;
        private ToolStripMenuItem removeStepMenuItem;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            chkMinimizeToTray = new CheckBox();
            btnStartStop = new Button();
            lblStatusCaption = new Label();
            lblStatus = new Label();
            lblHotkey = new Label();
            trayMenu = new ContextMenuStrip(components);
            trayOpenMenuItem = new ToolStripMenuItem();
            trayStartStopMenuItem = new ToolStripMenuItem();
            trayExitMenuItem = new ToolStripMenuItem();
            notifyIconApp = new NotifyIcon(components);
            grpSequence = new GroupBox();
            grpReplayMode = new GroupBox();
            lblReplayCount = new Label();
            nudReplayCount = new NumericUpDown();
            rbReplayUntilStopped = new RadioButton();
            rbReplayFixed = new RadioButton();
            rbReplayOnce = new RadioButton();
            lblDefaultDelay = new Label();
            nudDefaultDelay = new NumericUpDown();
            btnReplaySequence = new Button();
            btnClearSequence = new Button();
            btnRemoveStep = new Button();
            btnEditStep = new Button();
            btnRecordStep = new Button();
            btnStopRecording = new Button();
            btnStartRecording = new Button();
            lblRecordingHelp = new Label();
            lblRecordedActions = new Label();
            lstRecordedActions = new ListBox();
            sequenceMenu = new ContextMenuStrip(components);
            editStepMenuItem = new ToolStripMenuItem();
            removeStepMenuItem = new ToolStripMenuItem();
            clickTimer = new System.Windows.Forms.Timer(components);
            trayMenu.SuspendLayout();
            grpSequence.SuspendLayout();
            grpReplayMode.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudReplayCount).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudDefaultDelay).BeginInit();
            sequenceMenu.SuspendLayout();
            SuspendLayout();
            // 
            // chkMinimizeToTray
            // 
            chkMinimizeToTray.AutoSize = true;
            chkMinimizeToTray.Location = new Point(20, 20);
            chkMinimizeToTray.Name = "chkMinimizeToTray";
            chkMinimizeToTray.Size = new Size(121, 21);
            chkMinimizeToTray.TabIndex = 0;
            chkMinimizeToTray.Text = "Minimize to tray";
            chkMinimizeToTray.UseVisualStyleBackColor = true;
            // 
            // btnStartStop
            // 
            btnStartStop.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnStartStop.Location = new Point(20, 52);
            btnStartStop.Name = "btnStartStop";
            btnStartStop.Size = new Size(500, 48);
            btnStartStop.TabIndex = 1;
            btnStartStop.Text = "Replay sequence (F6)";
            btnStartStop.UseVisualStyleBackColor = true;
            btnStartStop.Click += btnStartStop_Click;
            // 
            // lblStatusCaption
            // 
            lblStatusCaption.AutoSize = true;
            lblStatusCaption.Location = new Point(20, 113);
            lblStatusCaption.Name = "lblStatusCaption";
            lblStatusCaption.Size = new Size(46, 17);
            lblStatusCaption.TabIndex = 2;
            lblStatusCaption.Text = "Status:";
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.ForeColor = Color.DarkRed;
            lblStatus.Location = new Point(68, 113);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(58, 17);
            lblStatus.TabIndex = 3;
            lblStatus.Text = "Stopped";
            // 
            // lblHotkey
            // 
            lblHotkey.AutoSize = true;
            lblHotkey.Location = new Point(180, 113);
            lblHotkey.Name = "lblHotkey";
            lblHotkey.Size = new Size(442, 17);
            lblHotkey.TabIndex = 4;
            lblHotkey.Text = "Hotkeys: F6 replay / capture+click, F7 capture step, F8 arm/stop recording";
            // 
            // trayMenu
            // 
            trayMenu.Items.AddRange(new ToolStripItem[] { trayOpenMenuItem, trayStartStopMenuItem, trayExitMenuItem });
            trayMenu.Name = "trayMenu";
            trayMenu.Size = new Size(116, 70);
            // 
            // trayOpenMenuItem
            // 
            trayOpenMenuItem.Name = "trayOpenMenuItem";
            trayOpenMenuItem.Size = new Size(115, 22);
            trayOpenMenuItem.Text = "Open";
            trayOpenMenuItem.Click += trayOpenMenuItem_Click;
            // 
            // trayStartStopMenuItem
            // 
            trayStartStopMenuItem.Name = "trayStartStopMenuItem";
            trayStartStopMenuItem.Size = new Size(115, 22);
            trayStartStopMenuItem.Text = "Replay";
            trayStartStopMenuItem.Click += trayStartStopMenuItem_Click;
            // 
            // trayExitMenuItem
            // 
            trayExitMenuItem.Name = "trayExitMenuItem";
            trayExitMenuItem.Size = new Size(115, 22);
            trayExitMenuItem.Text = "Exit";
            trayExitMenuItem.Click += trayExitMenuItem_Click;
            // 
            // notifyIconApp
            // 
            notifyIconApp.ContextMenuStrip = trayMenu;
            notifyIconApp.Text = "Auto Mouse Click";
            notifyIconApp.DoubleClick += notifyIconApp_DoubleClick;
            // 
            // grpSequence
            // 
            grpSequence.Controls.Add(grpReplayMode);
            grpSequence.Controls.Add(lblDefaultDelay);
            grpSequence.Controls.Add(nudDefaultDelay);
            grpSequence.Controls.Add(btnReplaySequence);
            grpSequence.Controls.Add(btnClearSequence);
            grpSequence.Controls.Add(btnRemoveStep);
            grpSequence.Controls.Add(btnEditStep);
            grpSequence.Controls.Add(btnRecordStep);
            grpSequence.Controls.Add(btnStopRecording);
            grpSequence.Controls.Add(btnStartRecording);
            grpSequence.Controls.Add(lblRecordingHelp);
            grpSequence.Controls.Add(lblRecordedActions);
            grpSequence.Controls.Add(lstRecordedActions);
            grpSequence.Location = new Point(20, 145);
            grpSequence.Name = "grpSequence";
            grpSequence.Size = new Size(500, 465);
            grpSequence.TabIndex = 5;
            grpSequence.TabStop = false;
            grpSequence.Text = "Sequence recorder";
            // 
            // grpReplayMode
            // 
            grpReplayMode.Controls.Add(lblReplayCount);
            grpReplayMode.Controls.Add(nudReplayCount);
            grpReplayMode.Controls.Add(rbReplayUntilStopped);
            grpReplayMode.Controls.Add(rbReplayFixed);
            grpReplayMode.Controls.Add(rbReplayOnce);
            grpReplayMode.Location = new Point(14, 181);
            grpReplayMode.Name = "grpReplayMode";
            grpReplayMode.Size = new Size(470, 93);
            grpReplayMode.TabIndex = 10;
            grpReplayMode.TabStop = false;
            grpReplayMode.Text = "Replay mode";
            // 
            // lblReplayCount
            // 
            lblReplayCount.AutoSize = true;
            lblReplayCount.Location = new Point(179, 58);
            lblReplayCount.Name = "lblReplayCount";
            lblReplayCount.Size = new Size(39, 17);
            lblReplayCount.TabIndex = 3;
            lblReplayCount.Text = "times";
            // 
            // nudReplayCount
            // 
            nudReplayCount.Location = new Point(103, 53);
            nudReplayCount.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudReplayCount.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudReplayCount.Name = "nudReplayCount";
            nudReplayCount.Size = new Size(70, 23);
            nudReplayCount.TabIndex = 2;
            nudReplayCount.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // rbReplayUntilStopped
            // 
            rbReplayUntilStopped.AutoSize = true;
            rbReplayUntilStopped.Location = new Point(260, 27);
            rbReplayUntilStopped.Name = "rbReplayUntilStopped";
            rbReplayUntilStopped.Size = new Size(130, 21);
            rbReplayUntilStopped.TabIndex = 4;
            rbReplayUntilStopped.Text = "Until stopped (F6)";
            rbReplayUntilStopped.UseVisualStyleBackColor = true;
            rbReplayUntilStopped.CheckedChanged += ReplayModeChanged;
            // 
            // rbReplayFixed
            // 
            rbReplayFixed.AutoSize = true;
            rbReplayFixed.Location = new Point(14, 56);
            rbReplayFixed.Name = "rbReplayFixed";
            rbReplayFixed.Size = new Size(97, 21);
            rbReplayFixed.TabIndex = 1;
            rbReplayFixed.Text = "Replay fixed";
            rbReplayFixed.UseVisualStyleBackColor = true;
            rbReplayFixed.CheckedChanged += ReplayModeChanged;
            // 
            // rbReplayOnce
            // 
            rbReplayOnce.AutoSize = true;
            rbReplayOnce.Checked = true;
            rbReplayOnce.Location = new Point(14, 27);
            rbReplayOnce.Name = "rbReplayOnce";
            rbReplayOnce.Size = new Size(97, 21);
            rbReplayOnce.TabIndex = 0;
            rbReplayOnce.TabStop = true;
            rbReplayOnce.Text = "Replay once";
            rbReplayOnce.UseVisualStyleBackColor = true;
            rbReplayOnce.CheckedChanged += ReplayModeChanged;
            // 
            // lblDefaultDelay
            // 
            lblDefaultDelay.AutoSize = true;
            lblDefaultDelay.Location = new Point(14, 145);
            lblDefaultDelay.Name = "lblDefaultDelay";
            lblDefaultDelay.Size = new Size(145, 17);
            lblDefaultDelay.TabIndex = 7;
            lblDefaultDelay.Text = "Default step delay (ms):";
            // 
            // nudDefaultDelay
            // 
            nudDefaultDelay.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            nudDefaultDelay.Location = new Point(156, 141);
            nudDefaultDelay.Maximum = new decimal(new int[] { 600000, 0, 0, 0 });
            nudDefaultDelay.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudDefaultDelay.Name = "nudDefaultDelay";
            nudDefaultDelay.Size = new Size(90, 23);
            nudDefaultDelay.TabIndex = 8;
            nudDefaultDelay.Value = new decimal(new int[] { 1000, 0, 0, 0 });
            // 
            // btnReplaySequence
            // 
            btnReplaySequence.Location = new Point(334, 138);
            btnReplaySequence.Name = "btnReplaySequence";
            btnReplaySequence.Size = new Size(150, 34);
            btnReplaySequence.TabIndex = 9;
            btnReplaySequence.Text = "Replay sequence";
            btnReplaySequence.UseVisualStyleBackColor = true;
            btnReplaySequence.Click += btnReplaySequence_Click;
            // 
            // btnClearSequence
            // 
            btnClearSequence.Location = new Point(334, 97);
            btnClearSequence.Name = "btnClearSequence";
            btnClearSequence.Size = new Size(150, 34);
            btnClearSequence.TabIndex = 6;
            btnClearSequence.Text = "Clear sequence";
            btnClearSequence.UseVisualStyleBackColor = true;
            btnClearSequence.Click += btnClearSequence_Click;
            // 
            // btnRemoveStep
            // 
            btnRemoveStep.Location = new Point(174, 97);
            btnRemoveStep.Name = "btnRemoveStep";
            btnRemoveStep.Size = new Size(150, 34);
            btnRemoveStep.TabIndex = 5;
            btnRemoveStep.Text = "Remove selected";
            btnRemoveStep.UseVisualStyleBackColor = true;
            btnRemoveStep.Click += btnRemoveStep_Click;
            // 
            // btnEditStep
            // 
            btnEditStep.Location = new Point(14, 97);
            btnEditStep.Name = "btnEditStep";
            btnEditStep.Size = new Size(150, 34);
            btnEditStep.TabIndex = 4;
            btnEditStep.Text = "Edit selected";
            btnEditStep.UseVisualStyleBackColor = true;
            btnEditStep.Click += btnEditStep_Click;
            // 
            // btnRecordStep
            // 
            btnRecordStep.Location = new Point(334, 57);
            btnRecordStep.Name = "btnRecordStep";
            btnRecordStep.Size = new Size(150, 34);
            btnRecordStep.TabIndex = 3;
            btnRecordStep.Text = "Capture step (F6)";
            btnRecordStep.UseVisualStyleBackColor = true;
            btnRecordStep.Click += btnRecordStep_Click;
            // 
            // btnStopRecording
            // 
            btnStopRecording.Location = new Point(174, 57);
            btnStopRecording.Name = "btnStopRecording";
            btnStopRecording.Size = new Size(150, 34);
            btnStopRecording.TabIndex = 2;
            btnStopRecording.Text = "Stop recording";
            btnStopRecording.UseVisualStyleBackColor = true;
            btnStopRecording.Click += btnStopRecording_Click;
            // 
            // btnStartRecording
            // 
            btnStartRecording.Location = new Point(14, 57);
            btnStartRecording.Name = "btnStartRecording";
            btnStartRecording.Size = new Size(150, 34);
            btnStartRecording.TabIndex = 1;
            btnStartRecording.Text = "Arm recording";
            btnStartRecording.UseVisualStyleBackColor = true;
            btnStartRecording.Click += btnStartRecording_Click;
            // 
            // lblRecordingHelp
            // 
            lblRecordingHelp.AutoSize = true;
            lblRecordingHelp.Location = new Point(14, 27);
            lblRecordingHelp.Name = "lblRecordingHelp";
            lblRecordingHelp.Size = new Size(575, 17);
            lblRecordingHelp.TabIndex = 0;
            lblRecordingHelp.Text = "1. Press F8 to arm recording  2. Move cursor  3. Press F6 to capture and click  4. Press F8 to stop";
            // 
            // lblRecordedActions
            // 
            lblRecordedActions.AutoSize = true;
            lblRecordedActions.Location = new Point(14, 286);
            lblRecordedActions.Name = "lblRecordedActions";
            lblRecordedActions.Size = new Size(228, 17);
            lblRecordedActions.TabIndex = 11;
            lblRecordedActions.Text = "Recorded steps (delay before action):";
            // 
            // lstRecordedActions
            // 
            lstRecordedActions.ContextMenuStrip = sequenceMenu;
            lstRecordedActions.FormattingEnabled = true;
            lstRecordedActions.HorizontalScrollbar = true;
            lstRecordedActions.IntegralHeight = false;
            lstRecordedActions.ItemHeight = 17;
            lstRecordedActions.Location = new Point(14, 311);
            lstRecordedActions.Name = "lstRecordedActions";
            lstRecordedActions.Size = new Size(470, 135);
            lstRecordedActions.TabIndex = 12;
            lstRecordedActions.SelectedIndexChanged += lstRecordedActions_SelectedIndexChanged;
            lstRecordedActions.DoubleClick += lstRecordedActions_DoubleClick;
            lstRecordedActions.MouseDown += lstRecordedActions_MouseDown;
            // 
            // sequenceMenu
            // 
            sequenceMenu.Items.AddRange(new ToolStripItem[] { editStepMenuItem, removeStepMenuItem });
            sequenceMenu.Name = "sequenceMenu";
            sequenceMenu.Size = new Size(205, 48);
            // 
            // editStepMenuItem
            // 
            editStepMenuItem.Name = "editStepMenuItem";
            editStepMenuItem.Size = new Size(204, 22);
            editStepMenuItem.Text = "Edit selected step";
            editStepMenuItem.Click += editStepMenuItem_Click;
            // 
            // removeStepMenuItem
            // 
            removeStepMenuItem.Name = "removeStepMenuItem";
            removeStepMenuItem.Size = new Size(204, 22);
            removeStepMenuItem.Text = "Remove selected step";
            removeStepMenuItem.Click += removeStepMenuItem_Click;
            // 
            // clickTimer
            // 
            clickTimer.Tick += clickTimer_Tick;
            // 
            // AutoClickerForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(540, 629);
            Controls.Add(grpSequence);
            Controls.Add(lblHotkey);
            Controls.Add(lblStatus);
            Controls.Add(lblStatusCaption);
            Controls.Add(btnStartStop);
            Controls.Add(chkMinimizeToTray);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            KeyPreview = true;
            MaximizeBox = false;
            MinimumSize = new Size(556, 668);
            Name = "AutoClickerForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Auto Mouse Click";
            FormClosing += AutoClickerForm_FormClosing;
            Load += AutoClickerForm_Load;
            KeyDown += AutoClickerForm_KeyDown;
            Resize += AutoClickerForm_Resize;
            trayMenu.ResumeLayout(false);
            grpSequence.ResumeLayout(false);
            grpSequence.PerformLayout();
            grpReplayMode.ResumeLayout(false);
            grpReplayMode.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudReplayCount).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudDefaultDelay).EndInit();
            sequenceMenu.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}

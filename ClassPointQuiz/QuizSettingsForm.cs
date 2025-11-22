using System;
using System.Drawing;
using System.Windows.Forms;

namespace ClassPointQuiz
{
    public class QuizSettingsForm : Form
    {
        // Public properties for settings
        public bool StartWithSlide { get; set; }
        public bool MinimizeWindow { get; set; }
        public int AutoCloseMinutes { get; set; }

        // UI Controls
        private Label lblTitle;
        private Label lblQuizInfo;
        private CheckBox chkStartWithSlide;
        private CheckBox chkMinimizeWindow;
        private CheckBox chkAutoClose;
        private ComboBox cmbAutoCloseTime;
        private Button btnRun;
        private Button btnCancel;

        private string quizTitle;
        private int numChoices;
        private bool allowMultiple;
        private bool hasCorrect;
        private bool competitionMode;

        public QuizSettingsForm(string quizTitle, int numChoices, bool allowMultiple, bool hasCorrect, bool competitionMode, int autoCloseMinutes)
        {
            this.quizTitle = quizTitle;
            this.numChoices = numChoices;
            this.allowMultiple = allowMultiple;
            this.hasCorrect = hasCorrect;
            this.competitionMode = competitionMode;
            this.AutoCloseMinutes = autoCloseMinutes;

            // Form setup
            this.Text = "Quiz Settings";
            this.Size = new Size(500, 480);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            SetupUI();
        }

        private void SetupUI()
        {
            int y = 20;

            // Title
            lblTitle = new Label
            {
                Text = "Quiz Settings",
                Location = new Point(20, y),
                Width = 440,
                Height = 35,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            this.Controls.Add(lblTitle);
            y += 50;

            // Quiz Info Panel
            var infoPanel = new Panel
            {
                Location = new Point(20, y),
                Width = 440,
                Height = 120,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(236, 240, 241)
            };

            var lblInfoTitle = new Label
            {
                Text = "📋 Quiz Information",
                Location = new Point(10, 10),
                Width = 420,
                Height = 25,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            infoPanel.Controls.Add(lblInfoTitle);

            lblQuizInfo = new Label
            {
                Text = $"Title: {quizTitle}\n" +
                       $"Number of Choices: {numChoices}\n" +
                       $"Allow Multiple: {(allowMultiple ? "Yes" : "No")}\n" +
                       $"Quiz Mode: {(competitionMode ? "Competition" : "Normal")}",
                Location = new Point(10, 40),
                Width = 420,
                Height = 70,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            infoPanel.Controls.Add(lblQuizInfo);

            this.Controls.Add(infoPanel);
            y += 140;

            // Settings Section
            var lblSettings = new Label
            {
                Text = "⚙️ Play Options",
                Location = new Point(20, y),
                Width = 440,
                Height = 30,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            this.Controls.Add(lblSettings);
            y += 40;

            // Start with slide checkbox
            chkStartWithSlide = new CheckBox
            {
                Text = "Start quiz automatically with this slide",
                Location = new Point(30, y),
                Width = 400,
                Height = 25,
                Font = new Font("Segoe UI", 10),
                Checked = this.StartWithSlide
            };
            this.Controls.Add(chkStartWithSlide);
            y += 35;

            // Minimize window checkbox
            chkMinimizeWindow = new CheckBox
            {
                Text = "Minimize PowerPoint when quiz opens",
                Location = new Point(30, y),
                Width = 400,
                Height = 25,
                Font = new Font("Segoe UI", 10),
                Checked = this.MinimizeWindow
            };
            this.Controls.Add(chkMinimizeWindow);
            y += 35;

            // Auto-close checkbox
            chkAutoClose = new CheckBox
            {
                Text = "Auto-close submission after:",
                Location = new Point(30, y),
                Width = 220,
                Height = 25,
                Font = new Font("Segoe UI", 10),
                Checked = true
            };
            chkAutoClose.CheckedChanged += (s, e) => cmbAutoCloseTime.Enabled = chkAutoClose.Checked;
            this.Controls.Add(chkAutoClose);

            // Auto-close time dropdown
            cmbAutoCloseTime = new ComboBox
            {
                Location = new Point(260, y - 3),
                Width = 180,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cmbAutoCloseTime.Items.AddRange(new object[] {
                "1 minute", "2 minutes", "3 minutes", "5 minutes", "10 minutes"
            });

            // Set selected index based on AutoCloseMinutes
            SetAutoCloseComboBox(AutoCloseMinutes);

            this.Controls.Add(cmbAutoCloseTime);
            y += 60;

            // Buttons Panel
            var btnPanel = new Panel
            {
                Location = new Point(20, y),
                Width = 440,
                Height = 50
            };

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(240, 0),
                Width = 90,
                Height = 40,
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            btnPanel.Controls.Add(btnCancel);

            btnRun = new Button
            {
                Text = "▶️ Run Quiz",
                Location = new Point(340, 0),
                Width = 100,
                Height = 40,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRun.FlatAppearance.BorderSize = 0;
            btnRun.Click += BtnRun_Click;
            btnPanel.Controls.Add(btnRun);

            this.Controls.Add(btnPanel);
        }

        private void SetAutoCloseComboBox(int minutes)
        {
            switch (minutes)
            {
                case 1:
                    cmbAutoCloseTime.SelectedIndex = 0;
                    break;
                case 2:
                    cmbAutoCloseTime.SelectedIndex = 1;
                    break;
                case 3:
                    cmbAutoCloseTime.SelectedIndex = 2;
                    break;
                case 5:
                    cmbAutoCloseTime.SelectedIndex = 3;
                    break;
                case 10:
                    cmbAutoCloseTime.SelectedIndex = 4;
                    break;
                default:
                    cmbAutoCloseTime.SelectedIndex = 3; // Default to 5 minutes
                    break;
            }
        }

        private void BtnRun_Click(object sender, EventArgs e)
        {
            // Save settings
            StartWithSlide = chkStartWithSlide.Checked;
            MinimizeWindow = chkMinimizeWindow.Checked;

            // Parse auto-close minutes from selected text
            if (chkAutoClose.Checked && cmbAutoCloseTime.SelectedItem != null)
            {
                string selectedText = cmbAutoCloseTime.SelectedItem.ToString();
                string[] parts = selectedText.Split(' ');
                if (parts.Length > 0 && int.TryParse(parts[0], out int parsedMinutes))
                {
                    AutoCloseMinutes = parsedMinutes;
                }
            }
            else
            {
                AutoCloseMinutes = 0; // Disabled
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}

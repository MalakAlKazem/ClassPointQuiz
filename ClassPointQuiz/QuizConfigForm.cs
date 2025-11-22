using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ClassPointQuiz
{
    public partial class QuizConfigForm : Form
    {
        // Public properties to store user input
        public string QuestionText { get; set; }
        public int NumChoices { get; set; }
        public bool AllowMultiple { get; set; }
        public bool HasCorrect { get; set; }
        public int CorrectAnswerIndex { get; set; }
        public string QuizMode { get; set; }
        public bool StartWithSlide { get; set; }
        public bool MinimizeWindow { get; set; }
        public int AutoCloseMinutes { get; set; }

        // Add this property to store the answers
        public List<string> Answers { get; private set; }

        private TextBox txtQuestion;
        private List<TextBox> answerTextBoxes;
        private ComboBox cmbCorrectAnswer;
        private CheckBox chkMultiple;
        private CheckBox chkHasCorrect;
        private ComboBox cmbQuizMode;
        private CheckBox chkStartWithSlide;
        private CheckBox chkMinimize;
        private NumericUpDown numAutoClose;
        private FlowLayoutPanel choicesPanel;
        private Panel mainPanel;

        public QuizConfigForm()
        {
            // Manual UI initialization - no designer file needed
            this.Text = "Interactive Quiz";
            this.Size = new Size(520, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            SetupCustomUI();
            NumChoices = 4; // Default
        }

        private void SetupCustomUI()
        {
            this.BackColor = Color.White;

            // Main scrollable panel
            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20),
                BackColor = Color.White
            };

            int y = 10;

            // Title
            var lblTitle = new Label
            {
                Text = "Multiple Choice",
                Location = new Point(20, y),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                AutoSize = true
            };
            mainPanel.Controls.Add(lblTitle);
            y += 50;

            // Number of choices
            var lblChoices = new Label
            {
                Text = "Number of choices",
                Location = new Point(20, y),
                Font = new Font("Segoe UI", 10),
                AutoSize = true
            };
            mainPanel.Controls.Add(lblChoices);
            y += 30;

            // Choice buttons (2-8)
            choicesPanel = new FlowLayoutPanel
            {
                Location = new Point(20, y),
                Width = 460,
                Height = 50,
                FlowDirection = FlowDirection.LeftToRight
            };

            for (int i = 2; i <= 8; i++)
            {
                var btn = new Button
                {
                    Text = i.ToString(),
                    Width = 50,
                    Height = 40,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = i == 4 ? Color.FromArgb(52, 152, 219) : Color.LightGray,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    Tag = i,
                    Margin = new Padding(2),
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += ChoiceButton_Click;
                choicesPanel.Controls.Add(btn);
            }
            mainPanel.Controls.Add(choicesPanel);
            y += 60;

            // Allow multiple
            chkMultiple = new CheckBox
            {
                Text = "Allow selecting multiple choices",
                Location = new Point(20, y),
                Width = 300,
                Font = new Font("Segoe UI", 10),
                Checked = false
            };
            mainPanel.Controls.Add(chkMultiple);
            y += 35;

            // Has correct answer
            var correctPanel = new FlowLayoutPanel
            {
                Location = new Point(20, y),
                Width = 460,
                Height = 35,
                FlowDirection = FlowDirection.LeftToRight
            };

            chkHasCorrect = new CheckBox
            {
                Text = "Has correct answer(s)",
                Checked = true,
                Width = 180,
                Font = new Font("Segoe UI", 10)
            };
            correctPanel.Controls.Add(chkHasCorrect);

            cmbCorrectAnswer = new ComboBox
            {
                Width = 100,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cmbCorrectAnswer.Items.AddRange(new object[] { "A", "B", "C", "D" });
            cmbCorrectAnswer.SelectedIndex = 0;
            correctPanel.Controls.Add(cmbCorrectAnswer);

            mainPanel.Controls.Add(correctPanel);
            y += 45;

            // Quiz mode
            var modePanel = new FlowLayoutPanel
            {
                Location = new Point(20, y),
                Width = 460,
                Height = 35,
                FlowDirection = FlowDirection.LeftToRight
            };

            var lblMode = new Label
            {
                Text = "Quiz mode",
                Width = 100,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleLeft
            };
            modePanel.Controls.Add(lblMode);

            cmbQuizMode = new ComboBox
            {
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cmbQuizMode.Items.AddRange(new object[] { "Easy", "Medium", "Hard" });
            cmbQuizMode.SelectedIndex = 0;
            modePanel.Controls.Add(cmbQuizMode);

            mainPanel.Controls.Add(modePanel);
            y += 50;

            // Play Options
            var lblPlayOptions = new Label
            {
                Text = "Play Options",
                Location = new Point(20, y),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true
            };
            mainPanel.Controls.Add(lblPlayOptions);
            y += 35;

            chkStartWithSlide = new CheckBox
            {
                Text = "Start activity with slide",
                Location = new Point(20, y),
                Width = 300,
                Font = new Font("Segoe UI", 10),
                Checked = true
            };
            mainPanel.Controls.Add(chkStartWithSlide);
            y += 30;

            chkMinimize = new CheckBox
            {
                Text = "Minimize activity window after activity starts",
                Location = new Point(20, y),
                Width = 400,
                Font = new Font("Segoe UI", 10),
                Checked = false
            };
            mainPanel.Controls.Add(chkMinimize);
            y += 30;

            // Auto-close
            var closePanel = new FlowLayoutPanel
            {
                Location = new Point(20, y),
                Width = 460,
                Height = 35,
                FlowDirection = FlowDirection.LeftToRight
            };

            var lblAutoClose = new Label
            {
                Text = "Auto-close submission after",
                Width = 200,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleLeft
            };
            closePanel.Controls.Add(lblAutoClose);

            numAutoClose = new NumericUpDown
            {
                Width = 60,
                Minimum = 1,
                Maximum = 60,
                Value = 1,
                Font = new Font("Segoe UI", 10)
            };
            closePanel.Controls.Add(numAutoClose);

            var lblMinutes = new Label
            {
                Text = "minute(s)",
                Width = 70,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleLeft
            };
            closePanel.Controls.Add(lblMinutes);

            mainPanel.Controls.Add(closePanel);
            y += 50;

            // Question input
            var lblQuestion = new Label
            {
                Text = "Question:",
                Location = new Point(20, y),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true
            };
            mainPanel.Controls.Add(lblQuestion);
            y += 25;

            txtQuestion = new TextBox
            {
                Location = new Point(20, y),
                Width = 460,
                Height = 80,
                Multiline = true,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            // Set placeholder text differently for older .NET versions
            try
            {
                txtQuestion.GetType().GetProperty("PlaceholderText")?.SetValue(txtQuestion, "Enter your question here...");
            }
            catch { }

            mainPanel.Controls.Add(txtQuestion);
            y += 90;

            // Answers
            var lblAnswers = new Label
            {
                Text = "Answers:",
                Location = new Point(20, y),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true
            };
            mainPanel.Controls.Add(lblAnswers);
            y += 25;

            answerTextBoxes = new List<TextBox>();
            for (int i = 0; i < 4; i++)
            {
                var txt = new TextBox
                {
                    Location = new Point(20, y),
                    Width = 460,
                    Font = new Font("Segoe UI", 10),
                    BorderStyle = BorderStyle.FixedSingle,
                    Name = $"txtAnswer{i}"
                };

                // Set placeholder text
                try
                {
                    txt.GetType().GetProperty("PlaceholderText")?.SetValue(txt, $"Answer {(char)('A' + i)}");
                }
                catch { }

                answerTextBoxes.Add(txt);
                mainPanel.Controls.Add(txt);
                y += 35;
            }

            y += 20;

            // Buttons
            var btnOK = new Button
            {
                Text = "Create Quiz & Start Session",
                Location = new Point(20, y),
                Width = 460,
                Height = 45,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnOK.FlatAppearance.BorderSize = 0;
            btnOK.Click += BtnOK_Click;
            mainPanel.Controls.Add(btnOK);

            y += 55;

            var btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(20, y),
                Width = 460,
                Height = 35,
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            mainPanel.Controls.Add(btnCancel);

            this.Controls.Add(mainPanel);
        }

        private void ChoiceButton_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            int numChoices = (int)btn.Tag;

            // Update button colors
            foreach (Button b in choicesPanel.Controls)
            {
                b.BackColor = (int)b.Tag == numChoices ?
                    Color.FromArgb(52, 152, 219) : Color.LightGray;
            }

            NumChoices = numChoices;
            UpdateAnswerTextBoxes(numChoices);
            UpdateCorrectAnswerDropdown(numChoices);
        }

        private void UpdateAnswerTextBoxes(int count)
        {
            // Show/hide existing textboxes
            for (int i = 0; i < answerTextBoxes.Count && i < 8; i++)
            {
                if (i < count)
                {
                    answerTextBoxes[i].Visible = true;
                }
                else
                {
                    answerTextBoxes[i].Visible = false;
                }
            }

            // Add more if needed
            if (count > answerTextBoxes.Count)
            {
                int startY = answerTextBoxes[answerTextBoxes.Count - 1].Location.Y + 35;

                for (int i = answerTextBoxes.Count; i < count; i++)
                {
                    var txt = new TextBox
                    {
                        Location = new Point(20, startY + (35 * (i - answerTextBoxes.Count))),
                        Width = 460,
                        Font = new Font("Segoe UI", 10),
                        BorderStyle = BorderStyle.FixedSingle,
                        Name = $"txtAnswer{i}"
                    };

                    try
                    {
                        txt.GetType().GetProperty("PlaceholderText")?.SetValue(txt, $"Answer {(char)('A' + i)}");
                    }
                    catch { }

                    answerTextBoxes.Add(txt);
                    mainPanel.Controls.Add(txt);
                }
            }
        }

        private void UpdateCorrectAnswerDropdown(int count)
        {
            cmbCorrectAnswer.Items.Clear();
            for (int i = 0; i < count; i++)
            {
                cmbCorrectAnswer.Items.Add(((char)('A' + i)).ToString());
            }
            cmbCorrectAnswer.SelectedIndex = 0;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            // Validate question
            if (string.IsNullOrWhiteSpace(txtQuestion.Text))
            {
                MessageBox.Show("Please enter a question.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate answers
            Answers = new List<string>();
            for (int i = 0; i < NumChoices; i++)
            {
                if (string.IsNullOrWhiteSpace(answerTextBoxes[i].Text))
                {
                    MessageBox.Show($"Please enter Answer {(char)('A' + i)}.",
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                Answers.Add(answerTextBoxes[i].Text);
            }

            // Set all properties
            QuestionText = txtQuestion.Text;
            CorrectAnswerIndex = cmbCorrectAnswer.SelectedIndex;
            AllowMultiple = chkMultiple.Checked;
            HasCorrect = chkHasCorrect.Checked;
            QuizMode = cmbQuizMode.SelectedItem.ToString().ToLower();
            StartWithSlide = chkStartWithSlide.Checked;
            MinimizeWindow = chkMinimize.Checked;
            AutoCloseMinutes = (int)numAutoClose.Value;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // QuizConfigForm
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "QuizConfigForm";
            this.Load += new System.EventHandler(this.QuizConfigForm_Load);
            this.ResumeLayout(false);

        }

        private void QuizConfigForm_Load(object sender, EventArgs e)
        {

        }
    }
}
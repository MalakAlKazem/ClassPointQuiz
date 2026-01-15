using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

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
        public List<int> CorrectAnswerIndices { get; set; }
        public string QuizMode { get; set; }
        public bool StartWithSlide { get; set; }
        public bool MinimizeWindow { get; set; }
        public int AutoCloseMinutes { get; set; }
        public List<string> Answers { get; private set; }

        private TextBox txtQuestion;
        private List<TextBox> answerTextBoxes;
        private List<CheckBox> correctAnswerCheckboxes;
        private CheckBox chkMultiple;
        private CheckBox chkHasCorrect;
        private ComboBox cmbQuizMode;
        private CheckBox chkStartWithSlide;
        private CheckBox chkMinimize;
        private NumericUpDown numAutoClose;
        private FlowLayoutPanel choicesPanel;
        private Panel mainPanel;
        private Panel correctAnswersPanel;
        private Label lblCorrectAnswersHint; // ✅ NEW: Hint label

        public QuizConfigForm()
        {
            this.Text = "Interactive Quiz";
            this.Size = new Size(520, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            SetupCustomUI();
            NumChoices = 4;
            CorrectAnswerIndices = new List<int>();
        }

        private void SetupCustomUI()
        {
            this.BackColor = Color.White;

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

            // ✅ Allow multiple - with event handler
            chkMultiple = new CheckBox
            {
                Text = "Allow selecting multiple choices",
                Location = new Point(20, y),
                Width = 300,
                Font = new Font("Segoe UI", 10),
                Checked = false
            };
            chkMultiple.CheckedChanged += ChkMultiple_CheckedChanged; // ✅ ADD EVENT HANDLER
            mainPanel.Controls.Add(chkMultiple);
            y += 35;

            // Has correct answer
            chkHasCorrect = new CheckBox
            {
                Text = "Has correct answer(s)",
                Checked = true,
                Location = new Point(20, y),
                Width = 300,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 152, 219)
            };
            chkHasCorrect.CheckedChanged += (s, e) =>
            {
                if (correctAnswersPanel != null)
                {
                    correctAnswersPanel.Visible = chkHasCorrect.Checked;
                }
                if (lblCorrectAnswersHint != null)
                {
                    lblCorrectAnswersHint.Visible = chkHasCorrect.Checked;
                }
            };
            mainPanel.Controls.Add(chkHasCorrect);
            y += 35;

            // ✅ NEW: Hint label for correct answers
            lblCorrectAnswersHint = new Label
            {
                Text = "Select correct answer(s):",
                Location = new Point(40, y),
                Width = 400,
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.Gray,
                AutoSize = true
            };
            mainPanel.Controls.Add(lblCorrectAnswersHint);
            y += 25;

            correctAnswersPanel = new Panel
            {
                Location = new Point(40, y),
                Width = 440,
                Height = 120,
                BackColor = Color.FromArgb(245, 247, 250),
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true
            };

            correctAnswerCheckboxes = new List<CheckBox>();
            for (int i = 0; i < 4; i++)
            {
                var chk = new CheckBox
                {
                    Text = $"Answer {(char)('A' + i)} is correct",
                    Location = new Point(10, 10 + (i * 28)),
                    Width = 400,
                    Font = new Font("Segoe UI", 10),
                    Tag = i,
                    Checked = (i == 0)
                };
                chk.CheckedChanged += CorrectAnswerCheckBox_CheckedChanged; // ✅ ADD EVENT
                correctAnswerCheckboxes.Add(chk);
                correctAnswersPanel.Controls.Add(chk);
            }

            mainPanel.Controls.Add(correctAnswersPanel);
            y += 130;

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

        // ✅ NEW: Handle "Allow multiple" checkbox change
        private void ChkMultiple_CheckedChanged(object sender, EventArgs e)
        {
            if (chkMultiple.Checked)
            {
                lblCorrectAnswersHint.Text = "✅ Multiple selection enabled - Select 2 or more correct answers:";
                lblCorrectAnswersHint.ForeColor = Color.FromArgb(46, 204, 113);
                lblCorrectAnswersHint.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            }
            else
            {
                lblCorrectAnswersHint.Text = "Select correct answer(s):";
                lblCorrectAnswersHint.ForeColor = Color.Gray;
                lblCorrectAnswersHint.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            }
        }

        // ✅ NEW: Auto-detect if multiple correct answers are selected
        private void CorrectAnswerCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            int checkedCount = correctAnswerCheckboxes.Count(chk => chk.Checked);
            
            // Auto-enable "Allow multiple" if more than 1 correct answer is selected
            if (checkedCount >= 2 && !chkMultiple.Checked)
            {
                chkMultiple.Checked = true;
            }
        }

        private void ChoiceButton_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            int numChoices = (int)btn.Tag;

            foreach (Button b in choicesPanel.Controls)
            {
                b.BackColor = (int)b.Tag == numChoices ?
                    Color.FromArgb(52, 152, 219) : Color.LightGray;
            }

            NumChoices = numChoices;
            UpdateAnswerTextBoxes(numChoices);
            UpdateCorrectAnswerCheckboxes(numChoices);
        }

        private void UpdateAnswerTextBoxes(int count)
        {
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
                RepositionButtons();
            }
        }

        private void UpdateCorrectAnswerCheckboxes(int count)
        {
            correctAnswersPanel.Controls.Clear();
            correctAnswerCheckboxes.Clear();

            correctAnswersPanel.Height = Math.Min(120, 10 + (count * 28) + 10);

            for (int i = 0; i < count; i++)
            {
                var chk = new CheckBox
                {
                    Text = $"Answer {(char)('A' + i)} is correct",
                    Location = new Point(10, 10 + (i * 28)),
                    Width = 400,
                    Font = new Font("Segoe UI", 10),
                    Tag = i,
                    Checked = (i == 0 && chkHasCorrect.Checked)
                };
                chk.CheckedChanged += CorrectAnswerCheckBox_CheckedChanged; // ✅ ADD EVENT
                correctAnswerCheckboxes.Add(chk);
                correctAnswersPanel.Controls.Add(chk);
            }
        }

        private void RepositionButtons()
        {
            int lastAnswerY = 0;
            foreach (var txt in answerTextBoxes)
            {
                if (txt.Visible && txt.Location.Y > lastAnswerY)
                {
                    lastAnswerY = txt.Location.Y;
                }
            }

            int newButtonY = lastAnswerY + 55;

            foreach (Control ctrl in mainPanel.Controls)
            {
                if (ctrl is Button btn)
                {
                    if (btn.Text.Contains("Create Quiz"))
                    {
                        btn.Location = new Point(20, newButtonY);
                    }
                    else if (btn.Text == "Cancel")
                    {
                        btn.Location = new Point(20, newButtonY + 55);
                    }
                }
            }

            mainPanel.PerformLayout();
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

            // Get all selected correct answers
            CorrectAnswerIndices = new List<int>();
            foreach (var chk in correctAnswerCheckboxes)
            {
                if (chk.Checked)
                {
                    CorrectAnswerIndices.Add((int)chk.Tag);
                }
            }

            // Validate correct answers
            if (chkHasCorrect.Checked && CorrectAnswerIndices.Count == 0)
            {
                MessageBox.Show("Please select at least one correct answer.", 
                    "Validation Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning);
                return;
            }

            // ✅ NEW: Warn if "Allow multiple" is checked but only 1 correct answer
            if (chkMultiple.Checked && CorrectAnswerIndices.Count < 2)
            {
                var result = MessageBox.Show(
                    "You enabled 'Allow multiple choices' but only selected 1 correct answer.\n\n" +
                    "Students will need to select ALL correct answers to get points.\n\n" +
                    "Do you want to continue?",
                    "Multiple Choice Notice",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                
                if (result == DialogResult.No)
                {
                    return;
                }
            }

            // Set first correct answer for backwards compatibility
            CorrectAnswerIndex = CorrectAnswerIndices.Count > 0 ? CorrectAnswerIndices[0] : 0;

            // Set all properties
            QuestionText = txtQuestion.Text;
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
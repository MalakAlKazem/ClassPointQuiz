using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ClassPointQuiz
{
    public class QuizListForm : Form
    {
        private PowerPointService pptService;
        private Panel mainPanel;

        public QuizListForm(List<ApiClient.QuizItem> quizzes, PowerPointService pptService)
        {
            this.pptService = pptService;

            // Form setup
            this.Text = "My Quizzes";
            this.Size = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            SetupUI(quizzes);
        }

        private void SetupUI(List<ApiClient.QuizItem> quizzes)
        {
            // Header
            var lblHeader = new Label
            {
                Text = "📋 My Quizzes",
                Location = new Point(20, 20),
                Width = 640,
                Height = 40,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            this.Controls.Add(lblHeader);

            var lblSubheader = new Label
            {
                Text = $"You have {quizzes.Count} quiz(es)",
                Location = new Point(20, 65),
                Width = 640,
                Height = 25,
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.Gray
            };
            this.Controls.Add(lblSubheader);

            // Scrollable panel for quizzes
            mainPanel = new Panel
            {
                Location = new Point(20, 100),
                Width = 640,
                Height = 420,
                AutoScroll = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(mainPanel);

            // Add quiz cards
            int y = 10;
            foreach (var quiz in quizzes)
            {
                AddQuizCard(quiz, ref y);
                y += 10; // Spacing between cards
            }

            // Close button
            var btnClose = new Button
            {
                Text = "Close",
                Location = new Point(540, 530),
                Width = 120,
                Height = 40,
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }

        private void AddQuizCard(ApiClient.QuizItem quiz, ref int y)
        {
            // Card panel
            var cardPanel = new Panel
            {
                Location = new Point(10, y),
                Width = 600,
                Height = 120,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 250)
            };

            // Quiz title
            var lblTitle = new Label
            {
                Text = quiz.title,
                Location = new Point(15, 15),
                Width = 400,
                Height = 30,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            cardPanel.Controls.Add(lblTitle);

            // Quiz ID
            var lblId = new Label
            {
                Text = $"Quiz ID: {quiz.quiz_id}",
                Location = new Point(15, 50),
                Width = 150,
                Height = 20,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray
            };
            cardPanel.Controls.Add(lblId);

            // Number of choices
            var lblChoices = new Label
            {
                Text = $"Choices: {quiz.num_choices}",
                Location = new Point(170, 50),
                Width = 100,
                Height = 20,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray
            };
            cardPanel.Controls.Add(lblChoices);

            // Created date
            var lblDate = new Label
            {
                Text = $"Created: {quiz.created_at:MMM dd, yyyy}",
                Location = new Point(15, 75),
                Width = 200,
                Height = 20,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray
            };
            cardPanel.Controls.Add(lblDate);

            // Run button
            var btnRun = new Button
            {
                Text = "▶️ Run Quiz",
                Location = new Point(450, 35),
                Width = 130,
                Height = 50,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Tag = quiz
            };
            btnRun.FlatAppearance.BorderSize = 0;
            btnRun.Click += BtnRun_Click;
            cardPanel.Controls.Add(btnRun);

            mainPanel.Controls.Add(cardPanel);
            y += 130;
        }

        private async void BtnRun_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            var quiz = (ApiClient.QuizItem)btn.Tag;

            try
            {
                // Disable button
                btn.Enabled = false;
                btn.Text = "Loading...";

                // Get full quiz details
                var quizDetails = await ApiClient.GetQuizDetailsAsync(quiz.quiz_id);

                if (quizDetails == null || quizDetails.question == null)
                {
                    MessageBox.Show("Failed to load quiz details!", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btn.Enabled = true;
                    btn.Text = "▶️ Run Quiz";
                    return;
                }

                // Extract answers
                var answerTexts = new List<string>();
                int correctIndex = 0;

                for (int i = 0; i < quizDetails.answers.Count; i++)
                {
                    answerTexts.Add(quizDetails.answers[i].answer_text);
                    if (quizDetails.answers[i].is_correct)
                    {
                        correctIndex = i;
                    }
                }

                // Store quiz ID globally
                ThisAddIn.CurrentQuizId = quiz.quiz_id;
                ThisAddIn.AutoCloseMinutes = quizDetails.quiz.close_submission_after;

                // Insert button to slide
                pptService.InsertQuizButtonToSlide(
                    quizDetails.question.question_text,
                    answerTexts,
                    correctIndex,
                    quiz.quiz_id
                );

                MessageBox.Show(
                    $"✅ Quiz Added to Slide!\n\n" +
                    $"A 'Run Quiz' button has been added to your current slide.\n\n" +
                    $"📌 To start the quiz:\n" +
                    $"1. Start your presentation (F5)\n" +
                    $"2. Click the green 'Run Quiz' button\n" +
                    $"3. A class code will be generated\n" +
                    $"4. Students join at: https://quizapp-joinclass.streamlit.app\n" +
                    $"5. Click button again to see live results",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // Close the form
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                btn.Enabled = true;
                btn.Text = "▶️ Run Quiz";
            }
        }
    }
}
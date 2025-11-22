using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace ClassPointQuiz
{
    public class LiveResultsDialog : Form
    {
        private int sessionId;
        private string classCode;
        private int autoCloseMinutes;
        private Timer refreshTimer;
        private Timer countdownTimer;
        private Label lblClassCode;
        private Label lblParticipants;
        private Label lblCountdown;
        private FlowLayoutPanel resultsPanel;
        private Button btnClose;
        private DateTime sessionStartTime;
        private bool sessionClosed = false;

        public LiveResultsDialog(int sessionId, string classCode, int autoCloseMinutes)
        {
            this.sessionId = sessionId;
            this.classCode = classCode;
            this.autoCloseMinutes = autoCloseMinutes;

            // ✅ FIX: Get session start time from the session (not current time)
            this.sessionStartTime = DateTime.Now; // Will be updated from API

            InitializeUI();
            LoadSessionStartTime(); // Get actual start time
            StartTimers();
            LoadResults();
        }

        private async void LoadSessionStartTime()
        {
            try
            {
                // Get session details to get actual start time
                var sessionInfo = await ApiClient.GetSessionInfoAsync(sessionId);
                if (sessionInfo != null && sessionInfo.started_at != DateTime.MinValue)
                {
                    this.sessionStartTime = sessionInfo.started_at;
                    System.Diagnostics.Debug.WriteLine($"Session started at: {sessionStartTime}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Could not load session start time: {ex.Message}");
                // Use current time as fallback
            }
        }

        private void InitializeUI()
        {
            this.Text = "Live Quiz Results";
            this.Size = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = Color.White;

            // Header Panel
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = Color.FromArgb(52, 152, 219),
                Padding = new Padding(20)
            };

            // Class Code Label
            lblClassCode = new Label
            {
                Text = $"📱 Class Code: {classCode}",
                Location = new Point(20, 20),
                Width = 640,
                Height = 40,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };
            headerPanel.Controls.Add(lblClassCode);

            // Participants Count
            lblParticipants = new Label
            {
                Text = "👥 0 participants",
                Location = new Point(20, 65),
                Width = 300,
                Height = 30,
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.White
            };
            headerPanel.Controls.Add(lblParticipants);

            // Countdown Timer
            lblCountdown = new Label
            {
                Text = $"⏱️ {autoCloseMinutes}:00 remaining",
                Location = new Point(340, 65),
                Width = 300,
                Height = 30,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 243, 205),
                TextAlign = ContentAlignment.TopRight
            };
            headerPanel.Controls.Add(lblCountdown);

            this.Controls.Add(headerPanel);

            // Results Panel (scrollable)
            resultsPanel = new FlowLayoutPanel
            {
                Location = new Point(20, 140),
                Width = 640,
                Height = 360,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(resultsPanel);

            // Close Button
            btnClose = new Button
            {
                Text = "Close Session",
                Location = new Point(270, 520),
                Width = 160,
                Height = 45,
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += BtnClose_Click;
            this.Controls.Add(btnClose);

            // Update countdown immediately
            UpdateCountdown();
        }

        private void StartTimers()
        {
            // Refresh results every 2 seconds
            refreshTimer = new Timer();
            refreshTimer.Interval = 2000;
            refreshTimer.Tick += async (s, e) => await LoadResults();
            refreshTimer.Start();

            // Update countdown every second
            countdownTimer = new Timer();
            countdownTimer.Interval = 1000;
            countdownTimer.Tick += (s, e) => UpdateCountdown();
            countdownTimer.Start();
        }

        private void UpdateCountdown()
        {
            if (sessionClosed)
            {
                lblCountdown.Text = "⏱️ Session Closed";
                lblCountdown.ForeColor = Color.FromArgb(231, 76, 60);
                return;
            }

            // ✅ FIX: Calculate time elapsed from session START time (not from dialog open)
            TimeSpan elapsed = DateTime.Now - sessionStartTime;
            TimeSpan total = TimeSpan.FromMinutes(autoCloseMinutes);
            TimeSpan remaining = total - elapsed;

            if (remaining.TotalSeconds <= 0)
            {
                // Time's up! Close session automatically
                lblCountdown.Text = "⏱️ Time's Up!";
                lblCountdown.ForeColor = Color.FromArgb(231, 76, 60);

                if (!sessionClosed)
                {
                    sessionClosed = true;
                    CloseSession();
                }
            }
            else
            {
                int minutes = (int)remaining.TotalMinutes;
                int seconds = remaining.Seconds;
                lblCountdown.Text = $"⏱️ {minutes}:{seconds:D2} remaining";

                // Change color when time is running out
                if (remaining.TotalMinutes < 1)
                {
                    lblCountdown.ForeColor = Color.FromArgb(231, 76, 60); // Red
                }
                else if (remaining.TotalMinutes < 2)
                {
                    lblCountdown.ForeColor = Color.FromArgb(241, 196, 15); // Yellow
                }
                else
                {
                    lblCountdown.ForeColor = Color.FromArgb(255, 243, 205); // Light yellow
                }
            }
        }

        private async Task LoadResults()
        {
            try
            {
                var results = await ApiClient.GetResultsAsync(sessionId);

                if (results == null) return;

                // Update participant count
                lblParticipants.Text = $"👥 {results.participant_count} participant(s) • {results.total_responses} response(s)";

                // Clear and rebuild results
                resultsPanel.Controls.Clear();

                if (results.results.Count == 0)
                {
                    var noResults = new Label
                    {
                        Text = "Waiting for responses...\n\n" +
                               "Students can join using the class code above.",
                        Width = 600,
                        Height = 100,
                        Font = new Font("Segoe UI", 14),
                        ForeColor = Color.Gray,
                        TextAlign = ContentAlignment.MiddleCenter
                    };
                    resultsPanel.Controls.Add(noResults);
                    return;
                }

                foreach (var result in results.results)
                {
                    AddResultCard(result);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading results: {ex.Message}");
            }
        }

        private void AddResultCard(ApiClient.ResultItem result)
        {
            var card = new Panel
            {
                Width = 600,
                Height = 80,
                Margin = new Padding(5),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = result.is_correct ?
                    Color.FromArgb(212, 237, 218) :
                    Color.FromArgb(248, 249, 250)
            };

            // Answer text with letter
            var lblAnswer = new Label
            {
                Text = $"{(char)('A' + result.answer_order)}. {result.answer_text}",
                Location = new Point(15, 15),
                Width = 400,
                Height = 50,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = result.is_correct ?
                    Color.FromArgb(40, 167, 69) :
                    Color.FromArgb(52, 73, 94)
            };
            card.Controls.Add(lblAnswer);

            // Correct indicator
            if (result.is_correct)
            {
                var lblCorrect = new Label
                {
                    Text = "✓",
                    Location = new Point(420, 15),
                    Width = 40,
                    Height = 50,
                    Font = new Font("Segoe UI", 24, FontStyle.Bold),
                    ForeColor = Color.FromArgb(40, 167, 69)
                };
                card.Controls.Add(lblCorrect);
            }

            // Count and percentage
            var lblStats = new Label
            {
                Text = $"{result.count} ({result.percentage:F1}%)",
                Location = new Point(470, 25),
                Width = 110,
                Height = 30,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 152, 219),
                TextAlign = ContentAlignment.MiddleRight
            };
            card.Controls.Add(lblStats);

            // Progress bar
            var progressBar = new Panel
            {
                Location = new Point(15, 60),
                Width = (int)(570 * (result.percentage / 100.0)),
                Height = 8,
                BackColor = result.is_correct ?
                    Color.FromArgb(40, 167, 69) :
                    Color.FromArgb(52, 152, 219)
            };
            card.Controls.Add(progressBar);

            resultsPanel.Controls.Add(card);
        }

        private async void BtnClose_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                "Are you sure you want to close this quiz session?\n\n" +
                "Students will no longer be able to submit answers.",
                "Close Session",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                await CloseSession();
                this.Close();
            }
        }

        private async Task CloseSession()
        {
            try
            {
                await ApiClient.CloseSessionAsync(sessionId);

                // Reset global state
                ThisAddIn.CurrentSessionId = 0;
                ThisAddIn.CurrentClassCode = null;

                sessionClosed = true;

                MessageBox.Show(
                    "Session closed successfully!\n\n" +
                    "Students can no longer submit answers.",
                    "Session Closed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error closing session: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // Stop timers
            if (refreshTimer != null)
            {
                refreshTimer.Stop();
                refreshTimer.Dispose();
            }

            if (countdownTimer != null)
            {
                countdownTimer.Stop();
                countdownTimer.Dispose();
            }
        }
    }
}
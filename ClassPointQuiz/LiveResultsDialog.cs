using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq;

namespace ClassPointQuiz
{
    public class LiveResultsDialog : Form
    {
        private int sessionId;
        private string classCode;
        private int autoCloseMinutes;
        private bool minimizeOnStart;
        private Timer refreshTimer;
        private Timer countdownTimer;
        private Label lblClassCode;
        private Label lblParticipants;
        private Label lblCountdown;
        private Panel resultsPanel;
        private Button btnClose;
        private Button btnViewStudents;
        private Button btnAddToSlides; // Add this field
        private DateTime sessionStartTime;
        private bool sessionClosed = false;
        private List<ApiClient.ResultItem> currentResults;

        public LiveResultsDialog(int sessionId, string classCode, int autoCloseMinutes, bool minimizeOnStart = false)
        {
            this.sessionId = sessionId;
            this.classCode = classCode;
            this.autoCloseMinutes = autoCloseMinutes;
            this.minimizeOnStart = minimizeOnStart;

            InitializeUI();

            this.sessionStartTime = DateTime.Now;

            System.Diagnostics.Debug.WriteLine("=== INITIALIZING LIVE RESULTS ===");
            System.Diagnostics.Debug.WriteLine($"Session ID: {sessionId}");
            System.Diagnostics.Debug.WriteLine($"Auto Close: {autoCloseMinutes} minutes");
            System.Diagnostics.Debug.WriteLine("================================");

            if (minimizeOnStart)
            {
                this.Shown += (s, e) =>
                {
                    this.WindowState = FormWindowState.Minimized;
                    System.Diagnostics.Debug.WriteLine("Results window minimized");
                };
            }

            this.Load += async (s, e) =>
            {
                await LoadSessionStartTimeAsync();
                StartTimers();
            };
        }

        private async Task LoadSessionStartTimeAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Fetching session info for session {sessionId}...");

                var sessionInfo = await ApiClient.GetSessionInfoAsync(sessionId);

                if (sessionInfo != null && sessionInfo.started_at != DateTime.MinValue)
                {
                    this.sessionStartTime = sessionInfo.started_at.Kind == DateTimeKind.Utc
                        ? sessionInfo.started_at.ToLocalTime()
                        : sessionInfo.started_at;

                    System.Diagnostics.Debug.WriteLine($"Session start time SET: {sessionStartTime}");

                    this.Invoke((MethodInvoker)delegate {
                        UpdateCountdown();
                    });
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Using fallback time");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            }
        }

        private void InitializeUI()
        {
            this.Text = "Live Quiz Results";
            this.Size = new Size(900, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.ShowInTaskbar = true;
            this.MinimizeBox = true;
            this.Font = new Font("Segoe UI", 9F);

            // Header Panel with gradient
            var headerPanel = new GradientPanel
            {
                Dock = DockStyle.Top,
                Height = 140,
                GradientStartColor = Color.FromArgb(41, 128, 185),
                GradientEndColor = Color.FromArgb(52, 152, 219),
                Padding = new Padding(30, 20, 30, 20)
            };

            // Class Code Label
            lblClassCode = new Label
            {
                Text = "Class Code: " + classCode,
                Location = new Point(30, 25),
                Width = 820,
                Height = 45,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            headerPanel.Controls.Add(lblClassCode);

            // Info Container Panel
            var infoPanel = new Panel
            {
                Location = new Point(30, 75),
                Width = 820,
                Height = 50,
                BackColor = Color.Transparent
            };

            // Participants Count with icon panel
            var participantsContainer = new RoundedPanel
            {
                Location = new Point(0, 0),
                Width = 400,
                Height = 50,
                BackColor = Color.FromArgb(50, 255, 255, 255),
                CornerRadius = 10
            };

            lblParticipants = new Label
            {
                Text = "0 participants",
                Location = new Point(15, 0),
                Width = 370,
                Height = 50,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };
            participantsContainer.Controls.Add(lblParticipants);
            infoPanel.Controls.Add(participantsContainer);

            // Countdown Timer with rounded background
            var countdownContainer = new RoundedPanel
            {
                Location = new Point(420, 0),
                Width = 400,
                Height = 50,
                BackColor = Color.FromArgb(50, 255, 255, 255),
                CornerRadius = 10
            };

            lblCountdown = new Label
            {
                Text = $"{autoCloseMinutes}:00 remaining",
                Location = new Point(15, 0),
                Width = 370,
                Height = 50,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 243, 205),
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = Color.Transparent
            };
            countdownContainer.Controls.Add(lblCountdown);
            infoPanel.Controls.Add(countdownContainer);

            headerPanel.Controls.Add(infoPanel);
            this.Controls.Add(headerPanel);

            // Results Container Panel with shadow effect
            var resultsContainer = new RoundedPanel
            {
                Location = new Point(30, 160),
                Width = 820,
                Height = 450,
                BackColor = Color.White,
                CornerRadius = 15
            };

            // Results Panel for BAR CHARTS
            resultsPanel = new Panel
            {
                Location = new Point(15, 15),
                Width = 790,
                Height = 420,
                AutoScroll = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White
            };
            resultsContainer.Controls.Add(resultsPanel);
            this.Controls.Add(resultsContainer);

            // Button Panel
            var buttonPanel = new Panel
            {
                Location = new Point(30, 630),
                Width = 820,
                Height = 55,
                BorderStyle = BorderStyle.None,
                BackColor = Color.Transparent
            };

            // Calculate button width and spacing for 3 buttons
            int buttonWidth = 260;
            int buttonSpacing = 20;
            int totalButtonsWidth = (buttonWidth * 3) + (buttonSpacing * 2);
            int startX = (820 - totalButtonsWidth) / 2;

            // View Students Button (LEFT)
            btnViewStudents = new ModernButton
            {
                Text = "View Student Details",
                Location = new Point(startX, 0),
                Width = buttonWidth,
                Height = 55,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                HoverColor = Color.FromArgb(41, 128, 185)
            };
            btnViewStudents.FlatAppearance.BorderSize = 0;
            btnViewStudents.Click += BtnViewStudents_Click;
            buttonPanel.Controls.Add(btnViewStudents);

            // Add to Slides Button (MIDDLE)
            btnAddToSlides = new ModernButton
            {
                Text = "Add to Slides",
                Location = new Point(startX + buttonWidth + buttonSpacing, 0),
                Width = buttonWidth,
                Height = 55,
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                HoverColor = Color.FromArgb(142, 68, 173),
                Enabled = false // Start disabled
            };
            btnAddToSlides.FlatAppearance.BorderSize = 0;
            btnAddToSlides.Click += BtnAddToSlides_Click;
            buttonPanel.Controls.Add(btnAddToSlides);

            // Close Session Button (RIGHT)
            btnClose = new ModernButton
            {
                Text = "Close Session",
                Location = new Point(startX + (buttonWidth + buttonSpacing) * 2, 0),
                Width = buttonWidth,
                Height = 55,
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                HoverColor = Color.FromArgb(192, 57, 43)
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += BtnClose_Click;
            buttonPanel.Controls.Add(btnClose);

            this.Controls.Add(buttonPanel);
        }

        private void StartTimers()
        {
            refreshTimer = new Timer();
            refreshTimer.Interval = 2000;
            refreshTimer.Tick += async (s, e) => await LoadResults();
            refreshTimer.Start();

            countdownTimer = new Timer();
            countdownTimer.Interval = 1000;
            countdownTimer.Tick += (s, e) => UpdateCountdown();
            countdownTimer.Start();

            System.Diagnostics.Debug.WriteLine("Timers started");

            UpdateCountdown();
            Task.Run(async () => await LoadResults());
        }

        private void UpdateCountdown()
        {
            if (sessionClosed)
            {
                lblCountdown.Text = "Session Closed";
                lblCountdown.ForeColor = Color.FromArgb(231, 76, 60);
                
                // Enable add to slides button when session is closed
                if (btnAddToSlides != null)
                {
                    btnAddToSlides.Enabled = true;
                }
                return;
            }

            if (sessionStartTime == DateTime.MinValue)
            {
                lblCountdown.Text = $"{autoCloseMinutes}:00 remaining";
                return;
            }

            TimeSpan elapsed = DateTime.Now - sessionStartTime;
            TimeSpan total = TimeSpan.FromMinutes(autoCloseMinutes);
            TimeSpan remaining = total - elapsed;

            System.Diagnostics.Debug.WriteLine($"Countdown - Elapsed: {elapsed.TotalMinutes:F2}m, Remaining: {remaining.TotalMinutes:F2}m");

            if (remaining.TotalSeconds <= 0)
            {
                lblCountdown.Text = "Time's Up!";
                lblCountdown.ForeColor = Color.FromArgb(231, 76, 60);

                // Enable add to slides button when time is up
                if (btnAddToSlides != null)
                {
                    btnAddToSlides.Enabled = true;
                }

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
                lblCountdown.Text = $"{minutes}:{seconds:D2} remaining";

                // Keep button disabled while time is remaining
                if (btnAddToSlides != null)
                {
                    btnAddToSlides.Enabled = false;
                }

                if (remaining.TotalMinutes < 1)
                    lblCountdown.ForeColor = Color.FromArgb(231, 76, 60);
                else if (remaining.TotalMinutes < 2)
                    lblCountdown.ForeColor = Color.FromArgb(241, 196, 15);
                else
                    lblCountdown.ForeColor = Color.FromArgb(255, 243, 205);
            }
        }

        private async Task LoadResults()
        {
            try
            {
                var results = await ApiClient.GetResultsAsync(sessionId);

                if (results == null) return;

                currentResults = results.results;

                lblParticipants.Text = $"{results.participant_count} student(s)  |  {results.total_responses} response(s)";

                resultsPanel.Controls.Clear();

                if (results.results.Count == 0)
                {
                    var noResults = new Label
                    {
                        Text = "Waiting for responses...\n\nStudents can join using the class code above.",
                        Dock = DockStyle.Fill,
                        Font = new Font("Segoe UI", 15),
                        ForeColor = Color.FromArgb(149, 165, 166),
                        TextAlign = ContentAlignment.MiddleCenter,
                        BackColor = Color.White
                    };
                    resultsPanel.Controls.Add(noResults);
                    return;
                }

                DrawBarCharts(results.results);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading results: {ex.Message}");
            }
        }

        private void DrawBarCharts(List<ApiClient.ResultItem> results)
        {
            resultsPanel.Controls.Clear();

            if (results == null || results.Count == 0) return;

            int chartWidth = 760;
            int chartHeight = 360;
            int barAreaHeight = 280;
            int barWidth = (chartWidth - 100) / results.Count;
            int maxBarWidth = 120;
            if (barWidth > maxBarWidth) barWidth = maxBarWidth;

            int spacing = (chartWidth - 100 - (barWidth * results.Count)) / (results.Count + 1);

            int maxCount = results.Max(r => r.count);
            if (maxCount == 0) maxCount = 1;

            var chartPanel = new Panel
            {
                Location = new Point(15, 15),
                Width = chartWidth,
                Height = chartHeight,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White
            };

            // Draw grid lines with better styling
            for (int i = 0; i <= 5; i++)
            {
                int y = 10 + (barAreaHeight * i / 5);

                var gridLine = new Panel
                {
                    Location = new Point(50, y),
                    Width = chartWidth - 70,
                    Height = 1,
                    BackColor = i == 5 ? Color.FromArgb(189, 195, 199) : Color.FromArgb(236, 240, 241)
                };
                chartPanel.Controls.Add(gridLine);

                var lblGrid = new Label
                {
                    Text = $"{100 - (i * 20)}%",
                    Location = new Point(5, y - 10),
                    Width = 40,
                    Height = 20,
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.FromArgb(127, 140, 141),
                    TextAlign = ContentAlignment.MiddleRight
                };
                chartPanel.Controls.Add(lblGrid);
            }

            // Draw vertical bars with rounded tops
            int x = 50 + spacing;

            foreach (var result in results)
            {
                int barHeight = (int)(barAreaHeight * (result.percentage / 100.0));
                if (barHeight < 3 && result.count > 0) barHeight = 3;

                int barY = 10 + barAreaHeight - barHeight;

                // Rounded bar panel
                var bar = new RoundedPanel
                {
                    Location = new Point(x, barY),
                    Width = barWidth - 10,
                    Height = barHeight,
                    BackColor = result.is_correct ?
                        Color.FromArgb(46, 204, 113) :
                        Color.FromArgb(52, 152, 219),
                    CornerRadius = 8
                };
                chartPanel.Controls.Add(bar);

                // Add gradient effect to bars
                bar.Paint += (s, e) =>
                {
                    using (LinearGradientBrush brush = new LinearGradientBrush(
                        bar.ClientRectangle,
                        result.is_correct ? Color.FromArgb(46, 204, 113) : Color.FromArgb(52, 152, 219),
                        result.is_correct ? Color.FromArgb(39, 174, 96) : Color.FromArgb(41, 128, 185),
                        90F))
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        using (GraphicsPath path = GetRoundedRect(bar.ClientRectangle, 8))
                        {
                            e.Graphics.FillPath(brush, path);
                        }
                    }
                };

                // Percentage label
                if (barHeight > 30)
                {
                    var lblPercent = new Label
                    {
                        Text = $"{result.percentage:F1}%",
                        Location = new Point(x, barY + 8),
                        Width = barWidth - 10,
                        Height = 22,
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        ForeColor = Color.White,
                        BackColor = Color.Transparent,
                        TextAlign = ContentAlignment.TopCenter
                    };
                    chartPanel.Controls.Add(lblPercent);
                    lblPercent.BringToFront();
                }
                else
                {
                    var lblPercent = new Label
                    {
                        Text = $"{result.percentage:F1}%",
                        Location = new Point(x, barY - 22),
                        Width = barWidth - 10,
                        Height = 20,
                        Font = new Font("Segoe UI", 9, FontStyle.Bold),
                        ForeColor = result.is_correct ?
                            Color.FromArgb(46, 204, 113) :
                            Color.FromArgb(52, 152, 219),
                        BackColor = Color.Transparent,
                        TextAlign = ContentAlignment.BottomCenter
                    };
                    chartPanel.Controls.Add(lblPercent);
                }

                // Count label
                var lblCount = new Label
                {
                    Text = $"({result.count})",
                    Location = new Point(x, barY - 45),
                    Width = barWidth - 10,
                    Height = 20,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.FromArgb(52, 73, 94),
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.BottomCenter
                };
                chartPanel.Controls.Add(lblCount);

                // Answer letter with circle background
                var answerBg = new RoundedPanel
                {
                    Location = new Point(x + (barWidth - 10 - 40) / 2, barAreaHeight + 15),
                    Width = 40,
                    Height = 40,
                    BackColor = result.is_correct ?
                        Color.FromArgb(46, 204, 113) :
                        Color.FromArgb(236, 240, 241),
                    CornerRadius = 20
                };
                chartPanel.Controls.Add(answerBg);

                var lblAnswer = new Label
                {
                    Text = $"{(char)('A' + result.answer_order)}",
                    Location = new Point(0, 0),
                    Width = 40,
                    Height = 40,
                    Font = new Font("Segoe UI", 16, FontStyle.Bold),
                    ForeColor = result.is_correct ?
                        Color.White :
                        Color.FromArgb(52, 73, 94),
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                answerBg.Controls.Add(lblAnswer);

                x += barWidth + spacing;
            }

            resultsPanel.Controls.Add(chartPanel);

            // Legend with improved styling
            int legendY = chartHeight + 25;

            foreach (var result in results)
            {
                var legendPanel = new RoundedPanel
                {
                    Location = new Point(25, legendY),
                    Width = 710,
                    Height = 45,
                    BackColor = Color.FromArgb(250, 251, 252),
                    CornerRadius = 8
                };

                var colorBox = new RoundedPanel
                {
                    Location = new Point(15, 12),
                    Width = 22,
                    Height = 22,
                    BackColor = result.is_correct ?
                        Color.FromArgb(46, 204, 113) :
                        Color.FromArgb(52, 152, 219),
                    CornerRadius = 4
                };
                legendPanel.Controls.Add(colorBox);

                var lblLegend = new Label
                {
                    Text = $"{(char)('A' + result.answer_order)}. {result.answer_text}",
                    Location = new Point(50, 0),
                    Width = 480,
                    Height = 45,
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    ForeColor = result.is_correct ?
                        Color.FromArgb(39, 174, 96) :
                        Color.FromArgb(52, 73, 94),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                legendPanel.Controls.Add(lblLegend);

                if (result.is_correct)
                {
                    var checkBg = new RoundedPanel
                    {
                        Location = new Point(540, 8),
                        Width = 30,
                        Height = 30,
                        BackColor = Color.FromArgb(46, 204, 113),
                        CornerRadius = 15
                    };
                    legendPanel.Controls.Add(checkBg);

                    var lblCheck = new Label
                    {
                        Text = "✓",
                        Location = new Point(0, 0),
                        Width = 30,
                        Height = 30,
                        Font = new Font("Segoe UI", 14, FontStyle.Bold),
                        ForeColor = Color.White,
                        TextAlign = ContentAlignment.MiddleCenter,
                        BackColor = Color.Transparent
                    };
                    checkBg.Controls.Add(lblCheck);
                }

                var lblStats = new Label
                {
                    Text = $"{result.percentage:F1}% ({result.count})",
                    Location = new Point(580, 0),
                    Width = 120,
                    Height = 45,
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    ForeColor = Color.FromArgb(127, 140, 141),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                legendPanel.Controls.Add(lblStats);

                resultsPanel.Controls.Add(legendPanel);
                legendY += 52;
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            // Top left arc
            path.AddArc(arc, 180, 90);

            // Top right arc
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // Bottom right arc
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // Bottom left arc
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        private async void BtnViewStudents_Click(object sender, EventArgs e)
        {
            try
            {
                var studentDetailsForm = new StudentDetailsDialog(sessionId, classCode);
                studentDetailsForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAddToSlides_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if time is up before allowing
                if (sessionStartTime != DateTime.MinValue && !sessionClosed)
                {
                    TimeSpan elapsed = DateTime.Now - sessionStartTime;
                    TimeSpan total = TimeSpan.FromMinutes(autoCloseMinutes);
                    TimeSpan remaining = total - elapsed;

                    if (remaining.TotalSeconds > 0)
                    {
                        MessageBox.Show(
                            $"Cannot add results to slides yet!\n\n" +
                            $"Please wait until the quiz time is up.\n" +
                            $"Time remaining: {(int)remaining.TotalMinutes}:{remaining.Seconds:D2}",
                            "Quiz In Progress",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }
                }

                if (currentResults == null || currentResults.Count == 0)
                {
                    MessageBox.Show(
                        "No results available to add to slides.\n\nPlease wait for students to submit responses.",
                        "No Results",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                // Get PowerPoint Application
                var powerPointApp = Globals.ThisAddIn.Application;

                if (powerPointApp.Presentations.Count == 0)
                {
                    MessageBox.Show(
                        "No PowerPoint presentation is currently open.\n\nPlease open a presentation first.",
                        "No Presentation",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                var presentation = powerPointApp.ActivePresentation;

                // Get slide dimensions
                float slideWidth = presentation.PageSetup.SlideWidth;
                float slideHeight = presentation.PageSetup.SlideHeight;

                // Add a new slide with blank layout
                var slide = presentation.Slides.Add(
                    presentation.Slides.Count + 1,
                    Microsoft.Office.Interop.PowerPoint.PpSlideLayout.ppLayoutBlank);

                // Calculate margins and dimensions based on slide size
                float marginX = slideWidth * 0.05f;
                float marginY = slideHeight * 0.04f;
                float contentWidth = slideWidth - (2 * marginX);

                // Add title
                var titleShape = slide.Shapes.AddTextbox(
                    Microsoft.Office.Core.MsoTextOrientation.msoTextOrientationHorizontal,
                    marginX, marginY, contentWidth, slideHeight * 0.08f);

                titleShape.TextFrame.TextRange.Text = $"📊 Quiz Results - {classCode}";
                titleShape.TextFrame.TextRange.Font.Size = (int)(slideHeight * 0.050f);
                titleShape.TextFrame.TextRange.Font.Bold = Microsoft.Office.Core.MsoTriState.msoTrue;
                titleShape.TextFrame.TextRange.Font.Color.RGB = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(52, 152, 219));
                titleShape.TextFrame.TextRange.ParagraphFormat.Alignment =
                    Microsoft.Office.Interop.PowerPoint.PpParagraphAlignment.ppAlignCenter;
                titleShape.Fill.Visible = Microsoft.Office.Core.MsoTriState.msoFalse;
                titleShape.Line.Visible = Microsoft.Office.Core.MsoTriState.msoFalse;

                // Add subtitle with total responses
                float subtitleY = marginY + slideHeight * 0.09f;
                var subtitleShape = slide.Shapes.AddTextbox(
                    Microsoft.Office.Core.MsoTextOrientation.msoTextOrientationHorizontal,
                    marginX, subtitleY, contentWidth, slideHeight * 0.04f);

                int totalResponses = currentResults.Sum(r => r.count);
                subtitleShape.TextFrame.TextRange.Text = $"Total Responses: {totalResponses}";
                subtitleShape.TextFrame.TextRange.Font.Size = (int)(slideHeight * 0.025f);
                subtitleShape.TextFrame.TextRange.Font.Color.RGB = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(127, 140, 141));
                subtitleShape.TextFrame.TextRange.ParagraphFormat.Alignment =
                    Microsoft.Office.Interop.PowerPoint.PpParagraphAlignment.ppAlignCenter;
                subtitleShape.Fill.Visible = Microsoft.Office.Core.MsoTriState.msoFalse;
                subtitleShape.Line.Visible = Microsoft.Office.Core.MsoTriState.msoFalse;

                // Calculate chart dimensions
                float chartStartY = marginY + slideHeight * 0.16f;
                float chartWidth = contentWidth * 0.95f;
                float barAreaHeight = slideHeight * 0.35f;

                // Calculate bar dimensions
                int barCount = currentResults.Count;
                float maxBarWidth = slideWidth * 0.12f;
                float barWidth = Math.Min(chartWidth / barCount * 0.7f, maxBarWidth);
                float spacing = (chartWidth - (barWidth * barCount)) / (barCount + 1);

                float x = marginX + spacing;

                // Draw bars for each answer
                foreach (var result in currentResults)
                {
                    float barHeight = barAreaHeight * (float)(result.percentage / 100.0);
                    if (barHeight < 5 && result.count > 0) barHeight = 5;

                    float barY = chartStartY + barAreaHeight - barHeight;

                    // Create bar
                    var bar = slide.Shapes.AddShape(
                        Microsoft.Office.Core.MsoAutoShapeType.msoShapeRoundedRectangle,
                        x, barY, barWidth * 0.85f, barHeight);

                    bar.Fill.ForeColor.RGB = System.Drawing.ColorTranslator.ToOle(
                        result.is_correct ?
                            Color.FromArgb(46, 204, 113) :
                            Color.FromArgb(52, 152, 219));
                    bar.Fill.Solid();
                    bar.Line.Visible = Microsoft.Office.Core.MsoTriState.msoFalse;

                    // Add percentage label above bar
                    var percentLabel = slide.Shapes.AddTextbox(
                        Microsoft.Office.Core.MsoTextOrientation.msoTextOrientationHorizontal,
                        x, barY - slideHeight * 0.040f, barWidth * 0.85f, slideHeight * 0.035f);
                    percentLabel.TextFrame.TextRange.Text = $"{result.percentage:F1}%";
                    percentLabel.TextFrame.TextRange.Font.Size = (int)(slideHeight * 0.020f);
                    percentLabel.TextFrame.TextRange.Font.Bold = Microsoft.Office.Core.MsoTriState.msoTrue;
                    percentLabel.TextFrame.TextRange.Font.Color.RGB =
                        System.Drawing.ColorTranslator.ToOle(Color.FromArgb(52, 73, 94));
                    percentLabel.TextFrame.TextRange.ParagraphFormat.Alignment =
                        Microsoft.Office.Interop.PowerPoint.PpParagraphAlignment.ppAlignCenter;
                    percentLabel.Fill.Visible = Microsoft.Office.Core.MsoTriState.msoFalse;
                    percentLabel.Line.Visible = Microsoft.Office.Core.MsoTriState.msoFalse;

                    // Add count label
                    var countLabel = slide.Shapes.AddTextbox(
                        Microsoft.Office.Core.MsoTextOrientation.msoTextOrientationHorizontal,
                        x, barY - slideHeight * 0.075f, barWidth * 0.85f, slideHeight * 0.030f);
                    countLabel.TextFrame.TextRange.Text = $"({result.count})";
                    countLabel.TextFrame.TextRange.Font.Size = (int)(slideHeight * 0.016f);
                    countLabel.TextFrame.TextRange.Font.Color.RGB =
                        System.Drawing.ColorTranslator.ToOle(Color.FromArgb(127, 140, 141));
                    countLabel.TextFrame.TextRange.ParagraphFormat.Alignment =
                        Microsoft.Office.Interop.PowerPoint.PpParagraphAlignment.ppAlignCenter;
                    countLabel.Fill.Visible = Microsoft.Office.Core.MsoTriState.msoFalse;
                    countLabel.Line.Visible = Microsoft.Office.Core.MsoTriState.msoFalse;

                    // Add answer letter below bar
                    var answerLabel = slide.Shapes.AddTextbox(
                        Microsoft.Office.Core.MsoTextOrientation.msoTextOrientationHorizontal,
                        x, chartStartY + barAreaHeight + slideHeight * 0.01f, barWidth * 0.85f, slideHeight * 0.045f);
                    answerLabel.TextFrame.TextRange.Text = $"{(char)('A' + result.answer_order)}";
                    answerLabel.TextFrame.TextRange.Font.Size = (int)(slideHeight * 0.028f);
                    answerLabel.TextFrame.TextRange.Font.Bold = Microsoft.Office.Core.MsoTriState.msoTrue;
                    answerLabel.TextFrame.TextRange.Font.Color.RGB = System.Drawing.ColorTranslator.ToOle(
                        result.is_correct ?
                            Color.FromArgb(46, 204, 113) :
                            Color.FromArgb(52, 73, 94));
                    answerLabel.TextFrame.TextRange.ParagraphFormat.Alignment =
                        Microsoft.Office.Interop.PowerPoint.PpParagraphAlignment.ppAlignCenter;
                    answerLabel.Fill.Visible = Microsoft.Office.Core.MsoTriState.msoFalse;
                    answerLabel.Line.Visible = Microsoft.Office.Core.MsoTriState.msoFalse;

                    // Add checkmark for correct answer
                    if (result.is_correct)
                    {
                        var checkmark = slide.Shapes.AddTextbox(
                            Microsoft.Office.Core.MsoTextOrientation.msoTextOrientationHorizontal,
                            x, chartStartY + barAreaHeight + slideHeight * 0.055f, barWidth * 0.85f, slideHeight * 0.035f);
                        checkmark.TextFrame.TextRange.Text = "✓";
                        checkmark.TextFrame.TextRange.Font.Size = (int)(slideHeight * 0.025f);
                        checkmark.TextFrame.TextRange.Font.Bold = Microsoft.Office.Core.MsoTriState.msoTrue;
                        checkmark.TextFrame.TextRange.Font.Color.RGB =
                            System.Drawing.ColorTranslator.ToOle(Color.FromArgb(46, 204, 113));
                        checkmark.TextFrame.TextRange.ParagraphFormat.Alignment =
                            Microsoft.Office.Interop.PowerPoint.PpParagraphAlignment.ppAlignCenter;
                        checkmark.Fill.Visible = Microsoft.Office.Core.MsoTriState.msoFalse;
                        checkmark.Line.Visible = Microsoft.Office.Core.MsoTriState.msoFalse;
                    }

                    x += barWidth + spacing;
                }

                // Add legend at the bottom with dynamic sizing
                float legendStartY = chartStartY + barAreaHeight + slideHeight * 0.11f;

                // Calculate available space and item height to fit all results
                float availableHeight = slideHeight - legendStartY - marginY;
                float legendItemHeight = Math.Min(slideHeight * 0.045f, availableHeight / currentResults.Count);

                // If items would be too small, make them readable minimum size
                if (legendItemHeight < slideHeight * 0.035f)
                {
                    legendItemHeight = slideHeight * 0.035f;
                }

                foreach (var result in currentResults)
                {
                    // Color indicator
                    var colorBox = slide.Shapes.AddShape(
                        Microsoft.Office.Core.MsoAutoShapeType.msoShapeRectangle,
                        marginX, legendStartY + slideHeight * 0.006f, slideHeight * 0.025f, slideHeight * 0.025f);
                    colorBox.Fill.ForeColor.RGB = System.Drawing.ColorTranslator.ToOle(
                        result.is_correct ?
                            Color.FromArgb(46, 204, 113) :
                            Color.FromArgb(52, 152, 219));
                    colorBox.Fill.Solid();
                    colorBox.Line.Visible = Microsoft.Office.Core.MsoTriState.msoFalse;

                    // Answer text - truncate if too long
                    string answerDisplayText = result.answer_text;
                    int maxLength = (int)(slideWidth / 7);
                    if (answerDisplayText.Length > maxLength)
                    {
                        answerDisplayText = answerDisplayText.Substring(0, maxLength - 3) + "...";
                    }

                    var answerText = slide.Shapes.AddTextbox(
                        Microsoft.Office.Core.MsoTextOrientation.msoTextOrientationHorizontal,
                        marginX + slideHeight * 0.040f, legendStartY, contentWidth * 0.65f, legendItemHeight);
                    answerText.TextFrame.TextRange.Text =
                        $"{(char)('A' + result.answer_order)}. {answerDisplayText}";
                    answerText.TextFrame.TextRange.Font.Size = (int)(slideHeight * 0.018f);
                    answerText.TextFrame.TextRange.Font.Bold = result.is_correct ?
                        Microsoft.Office.Core.MsoTriState.msoTrue : Microsoft.Office.Core.MsoTriState.msoFalse;
                    answerText.TextFrame.TextRange.Font.Color.RGB = System.Drawing.ColorTranslator.ToOle(
                        result.is_correct ?
                            Color.FromArgb(46, 204, 113) :
                            Color.FromArgb(52, 73, 94));
                    answerText.TextFrame.WordWrap = Microsoft.Office.Core.MsoTriState.msoFalse;
                    answerText.Fill.Visible = Microsoft.Office.Core.MsoTriState.msoFalse;
                    answerText.Line.Visible = Microsoft.Office.Core.MsoTriState.msoFalse;

                    // Percentage and count
                    var statsText = slide.Shapes.AddTextbox(
                        Microsoft.Office.Core.MsoTextOrientation.msoTextOrientationHorizontal,
                        marginX + contentWidth * 0.70f, legendStartY, contentWidth * 0.25f, legendItemHeight);
                    statsText.TextFrame.TextRange.Text = $"{result.percentage:F1}% ({result.count})";
                    statsText.TextFrame.TextRange.Font.Size = (int)(slideHeight * 0.016f);
                    statsText.TextFrame.TextRange.Font.Bold = Microsoft.Office.Core.MsoTriState.msoTrue;
                    statsText.TextFrame.TextRange.Font.Color.RGB =
                        System.Drawing.ColorTranslator.ToOle(Color.FromArgb(127, 140, 141));
                    statsText.TextFrame.TextRange.ParagraphFormat.Alignment =
                        Microsoft.Office.Interop.PowerPoint.PpParagraphAlignment.ppAlignRight;
                    statsText.Fill.Visible = Microsoft.Office.Core.MsoTriState.msoFalse;
                    statsText.Line.Visible = Microsoft.Office.Core.MsoTriState.msoFalse;

                    legendStartY += legendItemHeight;
                }

                MessageBox.Show(
                    "Results have been added to a new slide successfully!",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error adding results to slides:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Error adding to slides: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private async void BtnClose_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                "Are you sure you want to close this quiz session?\n\nStudents will no longer be able to submit answers.",
                "Close Session",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                await CloseSession();
                // Removed this.Close() - form stays open
            }
        }

        private async Task CloseSession()
        {
            try
            {
                await ApiClient.CloseSessionAsync(sessionId);

                ThisAddIn.CurrentSessionId = 0;
                ThisAddIn.CurrentClassCode = null;
                ThisAddIn.CurrentSessionStartTime = DateTime.MinValue;

                sessionClosed = true;

                MessageBox.Show(
                    "Session closed successfully!\n\nStudents can no longer submit answers.",
                    "Session Closed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
        
                // The form remains open so teachers can view results and add them to slides
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

    // Custom Panel with Gradient
    public class GradientPanel : Panel
    {
        public Color GradientStartColor { get; set; } = Color.Blue;
        public Color GradientEndColor { get; set; } = Color.LightBlue;

        protected override void OnPaint(PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(
                this.ClientRectangle,
                GradientStartColor,
                GradientEndColor,
                LinearGradientMode.Horizontal))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
            base.OnPaint(e);
        }
    }

    // Custom Rounded Panel
    public class RoundedPanel : Panel
    {
        public int CornerRadius { get; set; } = 10;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath path = GetRoundedRect(this.ClientRectangle, CornerRadius))
            using (SolidBrush brush = new SolidBrush(this.BackColor))
            {
                e.Graphics.FillPath(brush, path);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            this.Invalidate();
        }
    }

    // Modern Button with Hover Effect
    public class ModernButton : Button
    {
        public Color HoverColor { get; set; } = Color.Gray;
        private Color _originalColor;

        public ModernButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            _originalColor = this.BackColor;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _originalColor = this.BackColor;
            this.BackColor = HoverColor;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            this.BackColor = _originalColor;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        }
    }

    // Student Details Dialog
    public class StudentDetailsDialog : Form
    {
        private int sessionId;
        private string classCode;
        private DataGridView gridStudents;

        public StudentDetailsDialog(int sessionId, string classCode)
        {
            this.sessionId = sessionId;
            this.classCode = classCode;

            InitializeUI();
            LoadStudentDetails();
        }

        private void InitializeUI()
        {
            this.Text = $"Student Responses - {classCode}";
            this.Size = new Size(950, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9F);

            // Header panel with gradient
            var headerPanel = new GradientPanel
            {
                Location = new Point(0, 0),
                Width = 950,
                Height = 80,
                GradientStartColor = Color.FromArgb(41, 128, 185),
                GradientEndColor = Color.FromArgb(52, 152, 219)
            };

            var lblHeader = new Label
            {
                Text = "Student Responses",
                Location = new Point(30, 20),
                Width = 890,
                Height = 40,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            headerPanel.Controls.Add(lblHeader);
            this.Controls.Add(headerPanel);

            // Grid container with rounded corners
            var gridContainer = new RoundedPanel
            {
                Location = new Point(30, 100),
                Width = 890,
                Height = 480,
                BackColor = Color.White,
                CornerRadius = 12
            };

            gridStudents = new DataGridView
            {
                Location = new Point(15, 15),
                Width = 860,
                Height = 450,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                ColumnHeadersHeight = 45,
                RowTemplate = { Height = 40 },
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(236, 240, 241),
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            };

            // Style the column headers
            gridStudents.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 152, 219);
            gridStudents.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            gridStudents.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            gridStudents.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            gridStudents.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);

            // Style the rows
            gridStudents.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            gridStudents.DefaultCellStyle.ForeColor = Color.FromArgb(52, 73, 94);
            gridStudents.DefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 152, 219);
            gridStudents.DefaultCellStyle.SelectionForeColor = Color.White;
            gridStudents.DefaultCellStyle.Padding = new Padding(10, 5, 5, 5);

            gridStudents.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 252);

            gridStudents.Columns.Add("StudentName", "Student Name");
            gridStudents.Columns.Add("Answer", "Answer");
            gridStudents.Columns.Add("IsCorrect", "Result");

            gridStudents.Columns["StudentName"].Width = 200;
            gridStudents.Columns["Answer"].Width = 450;
            gridStudents.Columns["IsCorrect"].Width = 150;

            gridContainer.Controls.Add(gridStudents);
            this.Controls.Add(gridContainer);

            var btnClose = new ModernButton
            {
                Text = "Close",
                Location = new Point(790, 590),
                Width = 130,
                Height = 45,
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand,
                HoverColor = Color.FromArgb(127, 140, 141)
            };
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }

        private async void LoadStudentDetails()
        {
            try
            {
                gridStudents.Rows.Clear();
                gridStudents.Rows.Add("", "Loading student data...", "", "");

                var studentData = await ApiClient.GetStudentResponsesAsync(sessionId);

                gridStudents.Rows.Clear();

                if (studentData == null || studentData.students == null || studentData.students.Count == 0)
                {
                    gridStudents.Rows.Add("", "No student responses yet.", "", "");
                    return;
                }

                foreach (var student in studentData.students)
                {
                    string resultStr = student.is_correct ? "Correct" :
                        (student.answer_text == "Not submitted" ? "-" : "Incorrect");

                    int rowIndex = gridStudents.Rows.Add(
                        student.student_name,
                        student.answer_text,
                        resultStr
                    );

                    var resultCell = gridStudents.Rows[rowIndex].Cells["IsCorrect"];
                    if (student.is_correct)
                    {
                        resultCell.Style.ForeColor = Color.FromArgb(46, 204, 113);
                        resultCell.Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                    }
                    else if (student.answer_text != "Not submitted")
                    {
                        resultCell.Style.ForeColor = Color.FromArgb(231, 76, 60);
                        resultCell.Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                    }
                    else
                    {
                        resultCell.Style.ForeColor = Color.FromArgb(149, 165, 166);
                    }
                }

                this.Text = $"Student Responses - {classCode} ({studentData.total_students} students, {studentData.total_responses} responses)";
            }
            catch (Exception ex)
            {
                gridStudents.Rows.Clear();
                gridStudents.Rows.Add("", $"Error: {ex.Message}", "", "");
                System.Diagnostics.Debug.WriteLine($"Error loading student details: {ex.Message}");
            }
        }
    }
}
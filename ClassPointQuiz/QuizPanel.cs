using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Configuration;

namespace ClassPointQuiz
{
    public class QuizPanel : UserControl
    {
        private PowerPointService pptService;
        private int teacherId = 0;
        private string teacherName = "";
        private string teacherEmail = "";
        private Timer loginCheckTimer;
        private Panel loginPanel;
        private Panel quizPanel;
        private Panel quizSettingsPanel;
        private Label lblLoginStatus;
        private Button btnLogin;
        private Label lblWelcome;
        private Label lblEmail;
        private Button btnLogout;

        // Quiz settings view controls
        private Label lblSelectedQuizTitle;
        private Label lblSelectedQuizInfo;
        private CheckBox chkSettingsStartWithSlide;
        private CheckBox chkSettingsMinimizeWindow;
        private CheckBox chkSettingsAutoClose;
        private ComboBox cmbSettingsAutoCloseTime;
        private Button btnRunSelectedQuiz;
        private Button btnCancelSettings;
        private ApiClient.QuizDetails selectedQuizDetails;
        private List<string> selectedQuizAnswers;
        private int selectedQuizCorrectIndex;

        // Quiz UI controls
        private Label lblTitle;
        private Label lblChoicesLabel;
        private FlowLayoutPanel choicesPanel;
        private CheckBox chkMultiple;
        private CheckBox chkHasCorrect;
        private ComboBox cmbCorrectAnswer;
        private CheckBox chkQuizMode;
        private Label lblQuizModeLabel;
        private Label lblPlayOptions;
        private CheckBox chkStartWithSlide;
        private CheckBox chkMinimizeWindow;
        private CheckBox chkAutoClose;
        private ComboBox cmbAutoCloseTime;
        private Button btnViewResponses;
        private int selectedChoices = 4;

        private static readonly string STREAMLIT_URL = ConfigurationManager.AppSettings["StreamlitUrl"] ?? "http://localhost:8501";

        // Get login file path from user's home directory
        private static string GetLoginFilePath()
        {
            string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(homeDir, "teacher_login.txt");
        }

        public QuizPanel()
        {
            this.Name = "QuizPanel";
            this.Size = new System.Drawing.Size(400, 700);
            this.BackColor = System.Drawing.Color.White;
            this.AutoScroll = true;

            InitializeUI();
            InitializeServices();
            CheckLoginStatus();
            StartLoginCheckTimer();
        }

        private void InitializeUI()
        {
            // Login Panel (shown when not logged in)
            loginPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20),
                Visible = true
            };

            // Logo/Header
            var lblHeader = new Label
            {
                Text = "📚 ClassPoint Quiz",
                Location = new Point(20, 30),
                Width = 360,
                Height = 40,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 152, 219),
                TextAlign = ContentAlignment.MiddleCenter
            };
            loginPanel.Controls.Add(lblHeader);

            // Status Icon
            var lblIcon = new Label
            {
                Text = "🔒",
                Location = new Point(20, 100),
                Width = 360,
                Height = 80,
                Font = new Font("Segoe UI", 48),
                TextAlign = ContentAlignment.MiddleCenter
            };
            loginPanel.Controls.Add(lblIcon);

            // Login status
            lblLoginStatus = new Label
            {
                Text = "Not Logged In",
                Location = new Point(20, 190),
                Width = 360,
                Height = 35,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(231, 76, 60),
                TextAlign = ContentAlignment.MiddleCenter
            };
            loginPanel.Controls.Add(lblLoginStatus);

            // Instructions
            var lblInstructions = new Label
            {
                Text = "Please login to create quizzes",
                Location = new Point(20, 235),
                Width = 360,
                Height = 25,
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter
            };
            loginPanel.Controls.Add(lblInstructions);

            // Login button
            btnLogin = new Button
            {
                Text = "🌐 Login to ClassPoint Quiz",
                Location = new Point(50, 290),
                Width = 300,
                Height = 50,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;
            loginPanel.Controls.Add(btnLogin);

            // Debug: Manual check button
            var btnCheckLogin = new Button
            {
                Text = "🔍 Check Login Status (Debug)",
                Location = new Point(50, 355),
                Width = 300,
                Height = 40,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCheckLogin.FlatAppearance.BorderSize = 0;
            btnCheckLogin.Click += (s, e) =>
            {
                string path = GetLoginFilePath();
                MessageBox.Show(
                    $"Looking for file at:\n{path}\n\n" +
                    $"File exists: {File.Exists(path)}\n\n" +
                    $"Click OK to check login status...",
                    "Debug",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                CheckLoginStatus();
            };
            loginPanel.Controls.Add(btnCheckLogin);

            // Help text
            var lblHelp = new Label
            {
                Text = "This will open the login page\nin your web browser",
                Location = new Point(20, 410),
                Width = 360,
                Height = 40,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.TopCenter
            };
            loginPanel.Controls.Add(lblHelp);

            this.Controls.Add(loginPanel);

            // Quiz Panel (shown when logged in)
            quizPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20),
                AutoScroll = true,
                Visible = false
            };

            int y = 10;

            // Welcome banner
            var welcomePanel = new Panel
            {
                Location = new Point(10, y),
                Width = 370,
                Height = 80,
                BackColor = Color.FromArgb(46, 204, 113),
                Padding = new Padding(10)
            };

            lblWelcome = new Label
            {
                Text = "👋 Welcome!",
                Location = new Point(15, 10),
                Width = 340,
                Height = 30,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White
            };
            welcomePanel.Controls.Add(lblWelcome);

            lblEmail = new Label
            {
                Text = "teacher@school.com",
                Location = new Point(15, 40),
                Width = 250,
                Height = 25,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White
            };
            welcomePanel.Controls.Add(lblEmail);

            btnLogout = new Button
            {
                Text = "Logout",
                Location = new Point(285, 35),
                Width = 70,
                Height = 30,
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += BtnLogout_Click;
            welcomePanel.Controls.Add(btnLogout);

            quizPanel.Controls.Add(welcomePanel);
            y += 95;

            // Separator
            var separator = new Panel
            {
                Location = new Point(20, y),
                Width = 360,
                Height = 2,
                BackColor = Color.FromArgb(189, 195, 199)
            };
            quizPanel.Controls.Add(separator);
            y += 15;

            // Title
            lblTitle = new Label
            {
                Text = "Multiple Choice",
                Location = new Point(20, y),
                Width = 360,
                Height = 35,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 152, 219)
            };
            quizPanel.Controls.Add(lblTitle);
            y += 45;

            // Number of choices label
            lblChoicesLabel = new Label
            {
                Text = "Number of choices",
                Location = new Point(20, y),
                Width = 360,
                Height = 25,
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            quizPanel.Controls.Add(lblChoicesLabel);
            y += 30;

            // Choice buttons (2-8)
            choicesPanel = new FlowLayoutPanel
            {
                Location = new Point(20, y),
                Width = 360,
                Height = 60,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true
            };

            for (int i = 2; i <= 8; i++)
            {
                var btn = new Button
                {
                    Text = i.ToString(),
                    Width = 45,
                    Height = 45,
                    Font = new Font("Segoe UI", 12),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    Tag = i,
                    Margin = new Padding(2)
                };

                if (i == 4)
                {
                    btn.BackColor = Color.FromArgb(52, 152, 219);
                    btn.ForeColor = Color.White;
                }
                else
                {
                    btn.BackColor = Color.FromArgb(236, 240, 241);
                    btn.ForeColor = Color.FromArgb(52, 73, 94);
                }

                btn.FlatAppearance.BorderSize = 0;
                btn.Click += ChoiceButton_Click;
                choicesPanel.Controls.Add(btn);
            }

            quizPanel.Controls.Add(choicesPanel);
            y += 70;

            // Allow selecting multiple choices
            chkMultiple = new CheckBox
            {
                Text = "Allow selecting multiple choices",
                Location = new Point(20, y),
                Width = 360,
                Height = 25,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            quizPanel.Controls.Add(chkMultiple);
            y += 35;

            // Has correct answer
            chkHasCorrect = new CheckBox
            {
                Text = "Has correct answer(s)",
                Location = new Point(20, y),
                Width = 200,
                Height = 25,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(52, 73, 94),
                Checked = true
            };
            chkHasCorrect.CheckedChanged += ChkHasCorrect_CheckedChanged;
            quizPanel.Controls.Add(chkHasCorrect);

            // Correct answer dropdown
            cmbCorrectAnswer = new ComboBox
            {
                Location = new Point(230, y - 5),
                Width = 150,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            UpdateCorrectAnswerDropdown();
            quizPanel.Controls.Add(cmbCorrectAnswer);
            y += 40;

            // Quiz mode
            chkQuizMode = new CheckBox
            {
                Text = "Quiz mode",
                Location = new Point(20, y),
                Width = 120,
                Height = 25,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(52, 73, 94),
                Checked = true
            };
            quizPanel.Controls.Add(chkQuizMode);

            // Stars (Easy)
            lblQuizModeLabel = new Label
            {
                Text = "⭐ (Easy)",
                Location = new Point(145, y),
                Width = 100,
                Height = 25,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(241, 196, 15)
            };
            quizPanel.Controls.Add(lblQuizModeLabel);
            y += 45;

            // Play Options header
            lblPlayOptions = new Label
            {
                Text = "Play Options",
                Location = new Point(20, y),
                Width = 200,
                Height = 30,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            quizPanel.Controls.Add(lblPlayOptions);

            // Save as default link
            var lblSaveDefault = new LinkLabel
            {
                Text = "Save as default",
                Location = new Point(260, y + 5),
                Width = 120,
                Height = 25,
                Font = new Font("Segoe UI", 10),
                LinkColor = Color.FromArgb(52, 152, 219)
            };
            quizPanel.Controls.Add(lblSaveDefault);
            y += 40;

            // Start activity with slide
            chkStartWithSlide = new CheckBox
            {
                Text = "Start activity with slide",
                Location = new Point(20, y),
                Width = 360,
                Height = 25,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            quizPanel.Controls.Add(chkStartWithSlide);
            y += 35;

            // Minimize activity window
            chkMinimizeWindow = new CheckBox
            {
                Text = "Minimize activity window after activity starts",
                Location = new Point(20, y),
                Width = 360,
                Height = 25,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            quizPanel.Controls.Add(chkMinimizeWindow);
            y += 35;

            // Auto-close submission
            chkAutoClose = new CheckBox
            {
                Text = "Auto-close submission after",
                Location = new Point(20, y),
                Width = 220,
                Height = 25,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            quizPanel.Controls.Add(chkAutoClose);

            // Auto-close time dropdown
            cmbAutoCloseTime = new ComboBox
            {
                Location = new Point(250, y - 5),
                Width = 130,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cmbAutoCloseTime.Items.AddRange(new object[] {
                "1 minute", "2 minutes", "3 minutes", "5 minutes", "10 minutes"
            });
            cmbAutoCloseTime.SelectedIndex = 0;
            quizPanel.Controls.Add(cmbAutoCloseTime);
            y += 50;

            // Create Quiz Button
            var btnCreateQuiz = new Button
            {
                Text = "➕ Create Quiz",
                Location = new Point(20, y),
                Width = 360,
                Height = 50,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCreateQuiz.FlatAppearance.BorderSize = 0;
            btnCreateQuiz.Click += BtnCreateQuiz_Click;
            quizPanel.Controls.Add(btnCreateQuiz);
            y += 60;

            // View My Quizzes Button
            btnViewResponses = new Button
            {
                Text = "📋 View My Quizzes",
                Location = new Point(20, y),
                Width = 360,
                Height = 50,
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnViewResponses.FlatAppearance.BorderSize = 0;
            btnViewResponses.Click += BtnViewQuizzes_Click;
            quizPanel.Controls.Add(btnViewResponses);
            y += 70;

            // Separator for Presentation Mode
            var separatorPresentation = new Panel
            {
                Location = new Point(20, y),
                Width = 360,
                Height = 2,
                BackColor = Color.FromArgb(189, 195, 199)
            };
            quizPanel.Controls.Add(separatorPresentation);
            y += 15;

            // Presentation Mode Header
            var lblPresentationMode = new Label
            {
                Text = "🎬 Presentation Mode",
                Location = new Point(20, y),
                Width = 360,
                Height = 30,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 152, 219)
            };
            quizPanel.Controls.Add(lblPresentationMode);
            y += 40;

            var lblPresentationHelp = new Label
            {
                Text = "Use these controls during your presentation:",
                Location = new Point(20, y),
                Width = 360,
                Height = 25,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray
            };
            quizPanel.Controls.Add(lblPresentationHelp);
            y += 30;

            // Start Quiz Button (for presentation)
            var btnStartQuizPresentation = new Button
            {
                Text = "▶️ Start Quiz",
                Location = new Point(20, y),
                Width = 360,
                Height = 50,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnStartQuizPresentation.FlatAppearance.BorderSize = 0;
            btnStartQuizPresentation.Click += BtnStartQuizPresentation_Click;
            quizPanel.Controls.Add(btnStartQuizPresentation);
            y += 60;

            // View Results Button (for presentation)
            var btnViewResultsPresentation = new Button
            {
                Text = "📊 View Results",
                Location = new Point(20, y),
                Width = 360,
                Height = 50,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnViewResultsPresentation.FlatAppearance.BorderSize = 0;
            btnViewResultsPresentation.Click += BtnViewResultsPresentation_Click;
            quizPanel.Controls.Add(btnViewResultsPresentation);

            this.Controls.Add(quizPanel);

            // Quiz Settings Panel (shown when selecting a quiz to run)
            InitializeQuizSettingsPanel();
        }

        private void InitializeQuizSettingsPanel()
        {
            quizSettingsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                AutoScroll = true,
                Visible = false
            };

            int y = 20;

            // Back button
            btnCancelSettings = new Button
            {
                Text = "← Back",
                Location = new Point(10, y),
                Width = 80,
                Height = 35,
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCancelSettings.FlatAppearance.BorderSize = 0;
            btnCancelSettings.Click += BtnCancelSettings_Click;
            quizSettingsPanel.Controls.Add(btnCancelSettings);
            y += 50;

            // Title
            var lblSettingsTitle = new Label
            {
                Text = "Quiz Settings",
                Location = new Point(20, y),
                Width = 340,
                Height = 35,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            quizSettingsPanel.Controls.Add(lblSettingsTitle);
            y += 50;

            // Quiz info panel
            var infoPanel = new Panel
            {
                Location = new Point(20, y),
                Width = 340,
                Height = 140,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(236, 240, 241)
            };

            var lblInfoHeader = new Label
            {
                Text = "📋 Quiz Information",
                Location = new Point(10, 10),
                Width = 320,
                Height = 25,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            infoPanel.Controls.Add(lblInfoHeader);

            lblSelectedQuizTitle = new Label
            {
                Text = "",
                Location = new Point(10, 40),
                Width = 320,
                Height = 25,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            infoPanel.Controls.Add(lblSelectedQuizTitle);

            lblSelectedQuizInfo = new Label
            {
                Text = "",
                Location = new Point(10, 70),
                Width = 320,
                Height = 60,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            infoPanel.Controls.Add(lblSelectedQuizInfo);

            quizSettingsPanel.Controls.Add(infoPanel);
            y += 160;

            // Settings Section
            var lblSettingsHeader = new Label
            {
                Text = "⚙️ Play Options",
                Location = new Point(20, y),
                Width = 340,
                Height = 30,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            quizSettingsPanel.Controls.Add(lblSettingsHeader);
            y += 40;

            // Start with slide checkbox
            chkSettingsStartWithSlide = new CheckBox
            {
                Text = "Start quiz automatically with this slide",
                Location = new Point(30, y),
                Width = 330,
                Height = 25,
                Font = new Font("Segoe UI", 10),
                Checked = false
            };
            quizSettingsPanel.Controls.Add(chkSettingsStartWithSlide);
            y += 35;

            // Minimize window checkbox
            chkSettingsMinimizeWindow = new CheckBox
            {
                Text = "Minimize PowerPoint when quiz opens",
                Location = new Point(30, y),
                Width = 330,
                Height = 25,
                Font = new Font("Segoe UI", 10),
                Checked = false
            };
            quizSettingsPanel.Controls.Add(chkSettingsMinimizeWindow);
            y += 35;

            // Auto-close checkbox
            chkSettingsAutoClose = new CheckBox
            {
                Text = "Auto-close submission after:",
                Location = new Point(30, y),
                Width = 200,
                Height = 25,
                Font = new Font("Segoe UI", 10),
                Checked = true
            };
            chkSettingsAutoClose.CheckedChanged += (s, e) => cmbSettingsAutoCloseTime.Enabled = chkSettingsAutoClose.Checked;
            quizSettingsPanel.Controls.Add(chkSettingsAutoClose);

            // Auto-close time dropdown
            cmbSettingsAutoCloseTime = new ComboBox
            {
                Location = new Point(30, y + 30),
                Width = 150,
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cmbSettingsAutoCloseTime.Items.AddRange(new object[] {
                "1 minute", "2 minutes", "3 minutes", "5 minutes", "10 minutes"
            });
            cmbSettingsAutoCloseTime.SelectedIndex = 3; // Default to 5 minutes
            quizSettingsPanel.Controls.Add(cmbSettingsAutoCloseTime);
            y += 80;

            // Run button
            btnRunSelectedQuiz = new Button
            {
                Text = "▶️ Run Quiz",
                Location = new Point(20, y),
                Width = 340,
                Height = 50,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRunSelectedQuiz.FlatAppearance.BorderSize = 0;
            btnRunSelectedQuiz.Click += BtnRunSelectedQuiz_Click;
            quizSettingsPanel.Controls.Add(btnRunSelectedQuiz);

            this.Controls.Add(quizSettingsPanel);
        }

        private void InitializeServices()
        {
            try
            {
                pptService = new PowerPointService();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing services: {ex.Message}",
                    "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CheckLoginStatus()
        {
            string loginFile = GetLoginFilePath();

            // DEBUG: Show where we're looking
            System.Diagnostics.Debug.WriteLine($"🔍 Looking for login file at: {loginFile}");

            if (File.Exists(loginFile))
            {
                System.Diagnostics.Debug.WriteLine("✅ File found!");

                try
                {
                    var lines = File.ReadAllLines(loginFile);
                    System.Diagnostics.Debug.WriteLine($"📄 File has {lines.Length} lines");

                    if (lines.Length >= 3)
                    {
                        teacherId = int.Parse(lines[0]);
                        teacherName = lines[1];
                        teacherEmail = lines[2];

                        System.Diagnostics.Debug.WriteLine($"👤 Teacher ID: {teacherId}");
                        System.Diagnostics.Debug.WriteLine($"👤 Name: {teacherName}");
                        System.Diagnostics.Debug.WriteLine($"📧 Email: {teacherEmail}");

                        // Show quiz panel
                        if (this.InvokeRequired)
                        {
                            this.Invoke(new Action(() =>
                            {
                                loginPanel.Visible = false;
                                quizPanel.Visible = true;
                                lblWelcome.Text = $"👋 Welcome, {teacherName}!";
                                lblEmail.Text = teacherEmail;
                            }));
                        }
                        else
                        {
                            loginPanel.Visible = false;
                            quizPanel.Visible = true;
                            lblWelcome.Text = $"👋 Welcome, {teacherName}!";
                            lblEmail.Text = teacherEmail;
                        }

                        System.Diagnostics.Debug.WriteLine("✅ UI Updated - Login successful!");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ File has only {lines.Length} lines (need 3)");
                    }
                }
                catch (Exception ex)
                {
                    // If file is corrupted, delete it
                    System.Diagnostics.Debug.WriteLine($"❌ Error reading login file: {ex.Message}");
                    File.Delete(loginFile);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("❌ File not found - User not logged in");
            }
        }

        private void StartLoginCheckTimer()
        {
            loginCheckTimer = new Timer();
            loginCheckTimer.Interval = 2000; // Check every 2 seconds
            loginCheckTimer.Tick += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("⏰ Timer tick - Checking login status...");
                CheckLoginStatus();
            };
            loginCheckTimer.Start();
            System.Diagnostics.Debug.WriteLine("✅ Login check timer started (every 2 seconds)");
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                // Open Streamlit website in browser
                Process.Start(new ProcessStartInfo
                {
                    FileName = STREAMLIT_URL,
                    UseShellExecute = true
                });

                MessageBox.Show(
                    "🌐 Opening login page...\n\n" +
                    "Please login in your web browser.\n" +
                    "After login, close the browser and return to PowerPoint.\n\n" +
                    "This sidebar will automatically update!",
                    "Login",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error opening browser: {ex.Message}\n\n" +
                    "Please manually open: {STREAMLIT_URL}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                "Are you sure you want to logout?",
                "Confirm Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // Delete login file
                string loginFile = GetLoginFilePath();
                if (File.Exists(loginFile))
                {
                    File.Delete(loginFile);
                }

                // Reset state
                teacherId = 0;
                teacherName = "";
                teacherEmail = "";

                // Show login panel
                quizPanel.Visible = false;
                loginPanel.Visible = true;

                MessageBox.Show("Logged out successfully!", "Logout",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ChoiceButton_Click(object sender, EventArgs e)
        {
            var clickedBtn = (Button)sender;
            selectedChoices = (int)clickedBtn.Tag;

            foreach (Control ctrl in choicesPanel.Controls)
            {
                if (ctrl is Button btn)
                {
                    if ((int)btn.Tag == selectedChoices)
                    {
                        btn.BackColor = Color.FromArgb(52, 152, 219);
                        btn.ForeColor = Color.White;
                    }
                    else
                    {
                        btn.BackColor = Color.FromArgb(236, 240, 241);
                        btn.ForeColor = Color.FromArgb(52, 73, 94);
                    }
                }
            }

            UpdateCorrectAnswerDropdown();
        }

        private void UpdateCorrectAnswerDropdown()
        {
            cmbCorrectAnswer.Items.Clear();
            for (int i = 0; i < selectedChoices; i++)
            {
                cmbCorrectAnswer.Items.Add(((char)('A' + i)).ToString());
            }
            if (cmbCorrectAnswer.Items.Count > 1)
            {
                cmbCorrectAnswer.SelectedIndex = 1;
            }
        }

        private void ChkHasCorrect_CheckedChanged(object sender, EventArgs e)
        {
            cmbCorrectAnswer.Enabled = chkHasCorrect.Checked;
        }

        private async void BtnCreateQuiz_Click(object sender, EventArgs e)
        {
            try
            {
                // ✅ FIX: Get ALL UI values FIRST (before any await)
                int numChoices = selectedChoices;
                bool allowMultiple = chkMultiple.Checked;
                bool hasCorrect = chkHasCorrect.Checked;
                int correctIndex = cmbCorrectAnswer.SelectedIndex;
                bool isQuizMode = chkQuizMode.Checked;
                bool startWithSlide = chkStartWithSlide.Checked;
                bool minimizeWindow = chkMinimizeWindow.Checked;

                // Parse auto-close minutes from selected text (e.g., "5 minutes" -> 5)
                int autoCloseMinutes = 1; // Default
                if (cmbAutoCloseTime.SelectedItem != null)
                {
                    string selectedText = cmbAutoCloseTime.SelectedItem.ToString();
                    string[] parts = selectedText.Split(' ');
                    if (parts.Length > 0 && int.TryParse(parts[0], out int parsedMinutes))
                    {
                        autoCloseMinutes = parsedMinutes;
                    }
                }

                int currentTeacherId = teacherId;

                // NOW do the async call
                bool isHealthy = await ApiClient.CheckHealthAsync();
                if (!isHealthy)
                {
                    MessageBox.Show(
                        "Python backend is not running or not accessible!\n\n" +
                        "Please ensure the backend server is started:\n\n" +
                        "1. Navigate to your backend directory\n" +
                        "2. Run: python main.py\n\n" +
                        "Make sure the backend is running on the configured URL.",
                        "Backend Connection Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                // Open quiz configuration form (use saved values)
                var configForm = new QuizConfigForm();
                configForm.NumChoices = numChoices;
                configForm.AllowMultiple = allowMultiple;
                configForm.HasCorrect = hasCorrect;
                configForm.CorrectAnswerIndex = correctIndex;
                configForm.QuizMode = isQuizMode ? "easy" : "normal";
                configForm.StartWithSlide = startWithSlide;
                configForm.MinimizeWindow = minimizeWindow;
                configForm.AutoCloseMinutes = autoCloseMinutes;

                if (configForm.ShowDialog() == DialogResult.OK)
                {
                    // Create quiz via API - use values from configForm as user may have modified them
                    var quizRequest = new ApiClient.QuizCreateRequest
                    {
                        teacher_id = currentTeacherId,
                        title = configForm.QuestionText.Length > 50
                            ? configForm.QuestionText.Substring(0, 50) + "..."
                            : configForm.QuestionText,
                        question_text = configForm.QuestionText,
                        num_choices = numChoices,
                        allow_multiple = allowMultiple,
                        has_correct = hasCorrect,
                        quiz_mode = isQuizMode ? "easy" : "normal",
                        start_with_slide = startWithSlide,
                        minimize_window = minimizeWindow,
                        auto_close_minutes = configForm.AutoCloseMinutes // Use the value from the form
                    };

                    var answers = new List<ApiClient.Answer>();
                    for (int i = 0; i < configForm.Answers.Count; i++)
                    {
                        answers.Add(new ApiClient.Answer
                        {
                            text = configForm.Answers[i],
                            order = i,
                            is_correct = i == configForm.CorrectAnswerIndex
                        });
                    }

                    var result = await ApiClient.CreateQuizAsync(quizRequest, answers);

                    MessageBox.Show(
                        $"✅ Quiz Created Successfully!\n\n" +
                        $"Quiz ID: {result.quiz_id}\n" +
                        $"Question: {configForm.QuestionText}\n\n" +
                        $"Click 'View My Quizzes' to see it and run it.",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}\n\n{ex.StackTrace}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnViewQuizzes_Click(object sender, EventArgs e)
        {
            try
            {
                // Check backend health
                bool isHealthy = await ApiClient.CheckHealthAsync();
                if (!isHealthy)
                {
                    MessageBox.Show(
                        "Python backend is not running!",
                        "Backend Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                // Get teacher's quizzes
                var quizzes = await ApiClient.GetTeacherQuizzesAsync(teacherId);

                if (quizzes == null || quizzes.Count == 0)
                {
                    MessageBox.Show(
                        "You haven't created any quizzes yet.\n\n" +
                        "Click 'Create Quiz' to get started!",
                        "No Quizzes",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                // Show quiz list dialog
                var quizListForm = new QuizListForm(quizzes, pptService);
                quizListForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnStartQuizPresentation_Click(object sender, EventArgs e)
        {
            try
            {
                // Get current slide's quiz ID
                int quizId = GetCurrentSlideQuizId();

                if (quizId == 0)
                {
                    MessageBox.Show(
                        "No quiz found on current slide!\n\n" +
                        "Please navigate to a slide with a quiz button,\n" +
                        "or create a new quiz first.",
                        "No Quiz",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Start the quiz session
                ThisAddIn.CurrentQuizId = quizId;
                var session = await ApiClient.StartSessionAsync(quizId);

                ThisAddIn.CurrentSessionId = session.session_id;
                ThisAddIn.CurrentClassCode = session.class_code;

                MessageBox.Show(
                    $"✅ Quiz Started!\n\n" +
                    $"📱 CLASS CODE: {session.class_code}\n\n" +
                    $"Students join at:\n" +
                    $"🌐 https://quizapp-joinclass.streamlit.app\n\n" +
                    $"Click '📊 View Results' to see live responses!",
                    "Session Active",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting quiz: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnViewResultsPresentation_Click(object sender, EventArgs e)
        {
            try
            {
                if (ThisAddIn.CurrentSessionId == 0)
                {
                    MessageBox.Show(
                        "No active session!\n\n" +
                        "Please start a quiz first by clicking '▶️ Start Quiz'.",
                        "No Session",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                var dialog = new LiveResultsDialog(
                    ThisAddIn.CurrentSessionId,
                    ThisAddIn.CurrentClassCode,
                    ThisAddIn.AutoCloseMinutes
                );
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int GetCurrentSlideQuizId()
        {
            try
            {
                var slide = (Microsoft.Office.Interop.PowerPoint.Slide)Globals.ThisAddIn.Application.ActiveWindow.View.Slide;

                // Check all shapes for quiz button
                foreach (Microsoft.Office.Interop.PowerPoint.Shape shape in slide.Shapes)
                {
                    if (shape.Tags.Count > 0)
                    {
                        try
                        {
                            string quizIdStr = shape.Tags["QuizId"];
                            if (!string.IsNullOrEmpty(quizIdStr))
                            {
                                return int.Parse(quizIdStr);
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }

            return 0;
        }

        // Public method to show quiz settings with quiz data
        public void ShowQuizSettings(ApiClient.QuizDetails quizDetails, List<string> answerTexts, int correctIndex)
        {
            selectedQuizDetails = quizDetails;
            selectedQuizAnswers = answerTexts;
            selectedQuizCorrectIndex = correctIndex;

            // Update UI with quiz information
            lblSelectedQuizTitle.Text = quizDetails.quiz.title;
            lblSelectedQuizInfo.Text = $"Choices: {quizDetails.quiz.num_choices}\n" +
                                      $"Allow Multiple: {(quizDetails.quiz.allow_multiple ? "Yes" : "No")}\n" +
                                      $"Mode: {(quizDetails.quiz.competition_mode ? "Competition" : "Normal")}";

            // Set auto-close time from quiz details
            SetAutoCloseComboBox(quizDetails.quiz.close_submission_after);

            // Show settings panel, hide others
            loginPanel.Visible = false;
            quizPanel.Visible = false;
            quizSettingsPanel.Visible = true;
        }

        private void SetAutoCloseComboBox(int minutes)
        {
            switch (minutes)
            {
                case 1:
                    cmbSettingsAutoCloseTime.SelectedIndex = 0;
                    break;
                case 2:
                    cmbSettingsAutoCloseTime.SelectedIndex = 1;
                    break;
                case 3:
                    cmbSettingsAutoCloseTime.SelectedIndex = 2;
                    break;
                case 5:
                    cmbSettingsAutoCloseTime.SelectedIndex = 3;
                    break;
                case 10:
                    cmbSettingsAutoCloseTime.SelectedIndex = 4;
                    break;
                default:
                    cmbSettingsAutoCloseTime.SelectedIndex = 3; // Default to 5 minutes
                    break;
            }
        }

        private void BtnCancelSettings_Click(object sender, EventArgs e)
        {
            // Go back to quiz panel
            quizSettingsPanel.Visible = false;
            quizPanel.Visible = true;
        }

        private void BtnRunSelectedQuiz_Click(object sender, EventArgs e)
        {
            if (selectedQuizDetails == null || selectedQuizAnswers == null)
            {
                MessageBox.Show("No quiz selected!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Get settings from UI
                bool startWithSlide = chkSettingsStartWithSlide.Checked;
                bool minimizeWindow = chkSettingsMinimizeWindow.Checked;

                // Parse auto-close minutes
                int autoCloseMinutes = 5; // Default
                if (chkSettingsAutoClose.Checked && cmbSettingsAutoCloseTime.SelectedItem != null)
                {
                    string selectedText = cmbSettingsAutoCloseTime.SelectedItem.ToString();
                    string[] parts = selectedText.Split(' ');
                    if (parts.Length > 0 && int.TryParse(parts[0], out int parsedMinutes))
                    {
                        autoCloseMinutes = parsedMinutes;
                    }
                }
                else if (!chkSettingsAutoClose.Checked)
                {
                    autoCloseMinutes = 0; // Disabled
                }

                // Store settings globally
                ThisAddIn.CurrentQuizId = selectedQuizDetails.quiz.quiz_id;
                ThisAddIn.AutoCloseMinutes = autoCloseMinutes;
                ThisAddIn.StartWithSlide = startWithSlide;
                ThisAddIn.MinimizeWindow = minimizeWindow;

                // Insert button to slide
                pptService.InsertQuizButtonToSlide(
                    selectedQuizDetails.question.question_text,
                    selectedQuizAnswers,
                    selectedQuizCorrectIndex,
                    selectedQuizDetails.quiz.quiz_id
                );

                string settingsInfo = $"⚙️ Settings:\n" +
                    $"   • Auto-close: {(autoCloseMinutes > 0 ? autoCloseMinutes + " minutes" : "Disabled")}\n" +
                    $"   • Start with slide: {(startWithSlide ? "Yes" : "No")}\n" +
                    $"   • Minimize window: {(minimizeWindow ? "Yes" : "No")}";

                MessageBox.Show(
                    $"✅ Quiz Added to Slide!\n\n" +
                    $"A 'Run Quiz' button has been added to your current slide.\n\n" +
                    $"📌 To start the quiz:\n" +
                    $"1. Start your presentation (F5)\n" +
                    $"2. Click the green 'Run Quiz' button\n" +
                    $"3. A class code will be generated\n" +
                    $"4. Students join at: https://quizapp-joinclass.streamlit.app\n" +
                    $"5. Click button again to see live results\n\n" +
                    settingsInfo,
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // Go back to quiz panel
                quizSettingsPanel.Visible = false;
                quizPanel.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (loginCheckTimer != null)
                {
                    loginCheckTimer.Stop();
                    loginCheckTimer.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}
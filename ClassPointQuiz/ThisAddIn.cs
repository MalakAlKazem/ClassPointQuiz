using System;
using Microsoft.Office.Tools;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Office = Microsoft.Office.Core;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ClassPointQuiz
{
    public partial class ThisAddIn
    {
        private CustomTaskPane taskPane;
        public static QuizPanel QuizPanelInstance { get; private set; }
        public static int CurrentSessionId { get; set; }
        public static string CurrentClassCode { get; set; }
        public static int CurrentQuizId { get; set; }
        public static int AutoCloseMinutes { get; set; } = 5;

        private static PowerPointService pptService;
        private static ThisAddIn instance;
        private static System.Windows.Forms.Control uiInvoker = new System.Windows.Forms.Control();

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            instance = this;

            // Create the quiz panel
            QuizPanelInstance = new QuizPanel();

            // Add it as a task pane
            taskPane = this.CustomTaskPanes.Add(QuizPanelInstance, "ClassPoint Quiz");

            // Configure task pane
            taskPane.Width = 450;
            taskPane.Visible = true;

            // Initialize PowerPoint service
            pptService = new PowerPointService();

            // Initialize the invoker control and ensure it has a handle
            if (uiInvoker == null || uiInvoker.IsDisposed)
                uiInvoker = new System.Windows.Forms.Control();
            var handle = uiInvoker.Handle; // force handle creation

            // ✅ NEW: Subscribe to BeforeDoubleClick event for quiz buttons
            this.Application.SlideShowBegin += Application_SlideShowBegin;
        }

        /// <summary>
        /// When slide show starts, set up shape click monitoring
        /// </summary>
        private void Application_SlideShowBegin(PowerPoint.SlideShowWindow Wn)
        {
            try
            {
                // Monitor each slide for quiz button clicks
                System.Diagnostics.Debug.WriteLine("Slide show started - monitoring for quiz buttons");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SlideShowBegin: {ex.Message}");
            }
        }

        /// <summary>
        /// Start quiz when button clicked in slide show
        /// </summary>
        private void StartQuizFromSlideShow()
        {
            try
            {
                int quizId = CurrentQuizId;

                if (quizId == 0)
                {
                    MessageBox.Show("No quiz found! Please create a quiz first.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (CurrentSessionId > 0)
                {
                    MessageBox.Show($"Quiz is already running!\n\nClass Code: {CurrentClassCode}",
                        "Already Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Start session asynchronously
                System.Threading.Tasks.Task.Run(async () =>
                {
                    try
                    {
                        var session = await ApiClient.StartSessionAsync(quizId);
                        CurrentSessionId = session.session_id;
                        CurrentClassCode = session.class_code;

                        // Update button on UI thread
                        uiInvoker.Invoke(new Action(() =>
                        {
                            pptService.UpdateButtonToShowCode(quizId, session.class_code);

                            MessageBox.Show(
                                $"✅ Quiz Started!\n\n" +
                                $"📱 CLASS CODE: {session.class_code}\n\n" +
                                $"Students join at:\n" +
                                $"🌐 https://quizapp-joinclass.streamlit.app\n\n" +
                                $"Click the button again to see live results!",
                                "Session Active",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }));
                    }
                    catch (Exception ex)
                    {
                        uiInvoker.Invoke(new Action(() =>
                        {
                            MessageBox.Show($"Error starting session:\n\n{ex.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
                    }
                }).Wait();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Show results when button clicked again
        /// </summary>
        private void ShowResultsFromSlideShow()
        {
            try
            {
                if (CurrentSessionId == 0)
                {
                    MessageBox.Show("No active session!\n\nPlease run the quiz first.",
                        "No Session", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                uiInvoker.Invoke(new Action(() =>
                {
                    var dialog = new LiveResultsDialog(
                        CurrentSessionId,
                        CurrentClassCode,
                        AutoCloseMinutes
                    );
                    dialog.ShowDialog();
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ✅ Keep these for backward compatibility (if VBA is used)
        [ComVisible(true)]
        public static void StartQuizFromVBA()
        {
            instance?.StartQuizFromSlideShow();
        }

        [ComVisible(true)]
        public static void ShowResultsFromVBA()
        {
            instance?.ShowResultsFromSlideShow();
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            // Cleanup
            if (CurrentSessionId > 0)
            {
                try
                {
                    ApiClient.CloseSessionAsync(CurrentSessionId).Wait();
                }
                catch { }
            }

            // Unsubscribe from events
            try
            {
                this.Application.SlideShowBegin -= Application_SlideShowBegin;
            }
            catch { }
        }

        #region VSTO generated code
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        #endregion
    }
}
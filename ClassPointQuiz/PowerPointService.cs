using System;
using System.Collections.Generic;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Office = Microsoft.Office.Core;
using System.Windows.Forms;

namespace ClassPointQuiz
{
    public class PowerPointService
    {
        private PowerPoint.Application pptApp;

        public PowerPointService()
        {
            pptApp = Globals.ThisAddIn.Application;
        }

        /// <summary>
        /// Insert "Run Quiz" button to slide WITHOUT VBA macro
        /// Uses shape tags instead to identify quiz buttons
        /// </summary>
        public PowerPoint.Shape InsertQuizButtonToSlide(string question, List<string> answers, int correctIndex, int quizId)
        {
            try
            {
                // Check if any presentations are open
                if (pptApp.Presentations.Count == 0)
                {
                    MessageBox.Show("Please open or create a PowerPoint presentation first.",
                        "No Presentation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }

                PowerPoint.Slide slide = null;

                // Get current slide or create new one
                try
                {
                    slide = (PowerPoint.Slide)pptApp.ActiveWindow.View.Slide;
                }
                catch
                {
                    var presentation = pptApp.ActivePresentation;
                    slide = presentation.Slides.Add(presentation.Slides.Count + 1, PowerPoint.PpSlideLayout.ppLayoutBlank);
                }

                // Store quiz ID in slide for later reference
                slide.Name = $"QuizSlide_{quizId}";

                // Add Question Title
                var questionBox = slide.Shapes.AddTextbox(
                    Office.MsoTextOrientation.msoTextOrientationHorizontal,
                    50, 50, 620, 100
                );
                questionBox.TextFrame.TextRange.Text = question;
                questionBox.TextFrame.TextRange.Font.Size = 32;
                questionBox.TextFrame.TextRange.Font.Bold = Office.MsoTriState.msoTrue;
                questionBox.TextFrame.TextRange.Font.Color.RGB =
                    System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(44, 62, 80));
                questionBox.TextFrame.WordWrap = Office.MsoTriState.msoTrue;

                // Add Answer Options
                int startY = 180;
                int spacing = 70;

                for (int i = 0; i < answers.Count; i++)
                {
                    var answerBox = slide.Shapes.AddTextbox(
                        Office.MsoTextOrientation.msoTextOrientationHorizontal,
                        80, startY + (i * spacing), 560, 60
                    );

                    answerBox.TextFrame.TextRange.Text = $"{(char)('A' + i)}. {answers[i]}";
                    answerBox.TextFrame.TextRange.Font.Size = 24;
                    answerBox.Fill.ForeColor.RGB = System.Drawing.ColorTranslator.ToOle(
                        System.Drawing.Color.FromArgb(52, 152, 219)
                    );
                    answerBox.Fill.Solid();
                    answerBox.TextFrame.TextRange.Font.Color.RGB =
                        System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.White);
                    answerBox.Line.Visible = Office.MsoTriState.msoFalse;
                }

                // ✅ BEST SOLUTION: Button is VISUAL ONLY (no action)
                // Teacher controls quiz from SIDEBAR during presentation
                var runButton = slide.Shapes.AddShape(
                    Office.MsoAutoShapeType.msoShapeRoundedRectangle,
                    250, 500, 220, 60
                );

                runButton.Name = $"QuizButton_{quizId}";

                // Store quiz info in tags (for sidebar to read)
                runButton.Tags.Add("QuizButton", "Run");
                runButton.Tags.Add("QuizId", quizId.ToString());

                runButton.TextFrame.TextRange.Text = "Run Quiz";
                runButton.TextFrame.TextRange.Font.Size = 24;
                runButton.TextFrame.TextRange.Font.Bold = Office.MsoTriState.msoTrue;
                runButton.TextFrame.TextRange.Font.Color.RGB =
                    System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.White);
                runButton.TextFrame.TextRange.ParagraphFormat.Alignment =
                    PowerPoint.PpParagraphAlignment.ppAlignCenter;
                runButton.Fill.ForeColor.RGB =
                    System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(46, 204, 113));
                runButton.Fill.Solid();
                runButton.Line.Visible = Office.MsoTriState.msoFalse;

                // ✅ NO ACTION - Button is visual reference only
                // Teacher will use sidebar controls during presentation

                // Add instruction text
                var instructionBox = slide.Shapes.AddTextbox(
                    Office.MsoTextOrientation.msoTextOrientationHorizontal,
                    50, 570, 620, 50
                );
                instructionBox.TextFrame.TextRange.Text =
                    "📌 TO START QUIZ:\n" +
                    "1. Press F5 to start presentation\n" +
                    "2. Navigate to this slide\n" +
                    "3. Use the SIDEBAR on the right → Click 'Start Quiz'\n" +
                    "4. Share the class code with students";
                instructionBox.TextFrame.TextRange.Font.Size = 11;
                instructionBox.TextFrame.TextRange.Font.Color.RGB =
                    System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(52, 73, 94));
                instructionBox.TextFrame.TextRange.ParagraphFormat.Alignment =
                    PowerPoint.PpParagraphAlignment.ppAlignCenter;
                instructionBox.Fill.ForeColor.RGB =
                    System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(255, 243, 205));
                instructionBox.Fill.Solid();
                instructionBox.Line.Visible = Office.MsoTriState.msoTrue;
                instructionBox.Line.ForeColor.RGB =
                    System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(241, 196, 15));

                // Note: Success message is shown by QuizListForm, not here
                return runButton;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}\n\n{ex.StackTrace}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// Update button to show class code after session starts
        /// </summary>
        public void UpdateButtonToShowCode(int quizId, string classCode)
        {
            try
            {
                PowerPoint.Slide slide = null;

                // Try to get current slide
                try
                {
                    slide = (PowerPoint.Slide)pptApp.ActiveWindow.View.Slide;
                }
                catch
                {
                    // Try from slide show
                    try
                    {
                        var ssWindow = pptApp.SlideShowWindows[1];
                        slide = ssWindow.View.Slide;
                    }
                    catch
                    {
                        MessageBox.Show("Cannot find slide to update button.", "Error");
                        return;
                    }
                }

                // Find the button by name
                PowerPoint.Shape button = null;
                foreach (PowerPoint.Shape shape in slide.Shapes)
                {
                    if (shape.Name == $"QuizButton_{quizId}")
                    {
                        button = shape;
                        break;
                    }
                }

                if (button != null)
                {
                    button.TextFrame.TextRange.Text = $"Code: {classCode}\n📊 View Results";
                    button.TextFrame.TextRange.Font.Size = 18;
                    button.Fill.ForeColor.RGB =
                        System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(52, 152, 219));

                    // Update tag
                    button.Tags.Delete("QuizButton");
                    button.Tags.Add("QuizButton", "ShowResults");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating button: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if shape is a quiz button and get its state
        /// </summary>
        public (bool isQuizButton, string state, int quizId) CheckQuizButton(PowerPoint.Shape shape)
        {
            try
            {
                if (shape.Tags.Count > 0)
                {
                    string state = "";
                    string quizIdStr = "";

                    // Try to get tags
                    try
                    {
                        state = shape.Tags["QuizButton"];
                    }
                    catch { }

                    try
                    {
                        quizIdStr = shape.Tags["QuizId"];
                    }
                    catch { }

                    if (!string.IsNullOrEmpty(state) && !string.IsNullOrEmpty(quizIdStr))
                    {
                        return (true, state, int.Parse(quizIdStr));
                    }
                }
            }
            catch { }

            return (false, null, 0);
        }

        /// <summary>
        /// Get shape at click position (for detecting button clicks)
        /// </summary>
        public PowerPoint.Shape GetShapeAtPosition(PowerPoint.Slide slide, float x, float y)
        {
            try
            {
                foreach (PowerPoint.Shape shape in slide.Shapes)
                {
                    // Check if click is within shape bounds
                    if (x >= shape.Left && x <= (shape.Left + shape.Width) &&
                        y >= shape.Top && y <= (shape.Top + shape.Height))
                    {
                        return shape;
                    }
                }
            }
            catch { }

            return null;
        }
    }
}
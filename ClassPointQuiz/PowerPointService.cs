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

        /// <summary>
        /// Insert quiz results into a new PowerPoint slide
        /// </summary>
        public bool InsertResultsToSlide(string classCode, List<ApiClient.ResultItem> results, int totalResponses, int sessionId)
        {
            try
            {
                // Check if any presentations are open
                if (pptApp.Presentations.Count == 0)
                {
                    MessageBox.Show("Please open or create a PowerPoint presentation first.",
                        "No Presentation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                var presentation = pptApp.ActivePresentation;

                // Get slide dimensions to ensure everything fits
                float slideWidth = presentation.PageSetup.SlideWidth;
                float slideHeight = presentation.PageSetup.SlideHeight;

                // Create new slide
                var slide = presentation.Slides.Add(
                    presentation.Slides.Count + 1,
                    PowerPoint.PpSlideLayout.ppLayoutBlank
                );

                slide.Name = $"Results_{classCode}_{sessionId}";

                // Calculate margins and dimensions based on slide size
                float marginX = slideWidth * 0.05f; // 5% margin
                float marginY = slideHeight * 0.04f; // 4% margin
                float contentWidth = slideWidth - (2 * marginX);

                // Add Title
                var titleBox = slide.Shapes.AddTextbox(
                    Office.MsoTextOrientation.msoTextOrientationHorizontal,
                    marginX, marginY, contentWidth, slideHeight * 0.08f
                );
                titleBox.TextFrame.TextRange.Text = $"📊 Quiz Results - {classCode}";
                titleBox.TextFrame.TextRange.Font.Size = (int)(slideHeight * 0.050f); // Slightly smaller
                titleBox.TextFrame.TextRange.Font.Bold = Office.MsoTriState.msoTrue;
                titleBox.TextFrame.TextRange.Font.Color.RGB =
                    System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(52, 152, 219));
                titleBox.TextFrame.TextRange.ParagraphFormat.Alignment =
                    PowerPoint.PpParagraphAlignment.ppAlignCenter;

                // Add subtitle with total responses
                float subtitleY = marginY + slideHeight * 0.09f;
                var subtitleBox = slide.Shapes.AddTextbox(
                    Office.MsoTextOrientation.msoTextOrientationHorizontal,
                    marginX, subtitleY, contentWidth, slideHeight * 0.04f
                );
                subtitleBox.TextFrame.TextRange.Text = $"Total Responses: {totalResponses}";
                subtitleBox.TextFrame.TextRange.Font.Size = (int)(slideHeight * 0.025f);
                subtitleBox.TextFrame.TextRange.Font.Color.RGB =
                    System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(127, 140, 141));
                subtitleBox.TextFrame.TextRange.ParagraphFormat.Alignment =
                    PowerPoint.PpParagraphAlignment.ppAlignCenter;

                // Calculate bar chart dimensions - reduced to make more room for legend
                float chartStartY = marginY + slideHeight * 0.16f;
                float chartWidth = contentWidth * 0.95f;
                float barAreaHeight = slideHeight * 0.35f; // Reduced from 0.40f to make room for legend

                // Calculate bar dimensions
                int barCount = results.Count;
                float maxBarWidth = slideWidth * 0.12f; // Max 12% of slide width per bar
                float barWidth = Math.Min(chartWidth / barCount * 0.7f, maxBarWidth);
                float spacing = (chartWidth - (barWidth * barCount)) / (barCount + 1);

                float x = marginX + spacing;

                // Draw bars for each answer
                foreach (var result in results)
                {
                    float barHeight = barAreaHeight * (float)(result.percentage / 100.0);
                    if (barHeight < 5 && result.count > 0) barHeight = 5;

                    float barY = chartStartY + barAreaHeight - barHeight;

                    // Create bar
                    var bar = slide.Shapes.AddShape(
                        Office.MsoAutoShapeType.msoShapeRoundedRectangle,
                        x, barY, barWidth * 0.85f, barHeight
                    );

                    bar.Fill.ForeColor.RGB = System.Drawing.ColorTranslator.ToOle(
                        result.is_correct ?
                            System.Drawing.Color.FromArgb(46, 204, 113) :
                            System.Drawing.Color.FromArgb(52, 152, 219)
                    );
                    bar.Fill.Solid();
                    bar.Line.Visible = Office.MsoTriState.msoFalse;

                    // Add percentage label above bar
                    var percentLabel = slide.Shapes.AddTextbox(
                        Office.MsoTextOrientation.msoTextOrientationHorizontal,
                        x, barY - slideHeight * 0.040f, barWidth * 0.85f, slideHeight * 0.035f
                    );
                    percentLabel.TextFrame.TextRange.Text = $"{result.percentage:F1}%";
                    percentLabel.TextFrame.TextRange.Font.Size = (int)(slideHeight * 0.020f);
                    percentLabel.TextFrame.TextRange.Font.Bold = Office.MsoTriState.msoTrue;
                    percentLabel.TextFrame.TextRange.Font.Color.RGB =
                        System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(52, 73, 94));
                    percentLabel.TextFrame.TextRange.ParagraphFormat.Alignment =
                        PowerPoint.PpParagraphAlignment.ppAlignCenter;

                    // Add count label
                    var countLabel = slide.Shapes.AddTextbox(
                        Office.MsoTextOrientation.msoTextOrientationHorizontal,
                        x, barY - slideHeight * 0.075f, barWidth * 0.85f, slideHeight * 0.030f
                    );
                    countLabel.TextFrame.TextRange.Text = $"({result.count})";
                    countLabel.TextFrame.TextRange.Font.Size = (int)(slideHeight * 0.016f);
                    countLabel.TextFrame.TextRange.Font.Color.RGB =
                        System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(127, 140, 141));
                    countLabel.TextFrame.TextRange.ParagraphFormat.Alignment =
                        PowerPoint.PpParagraphAlignment.ppAlignCenter;

                    // Add answer letter below bar
                    var answerLabel = slide.Shapes.AddTextbox(
                        Office.MsoTextOrientation.msoTextOrientationHorizontal,
                        x, chartStartY + barAreaHeight + slideHeight * 0.01f, barWidth * 0.85f, slideHeight * 0.045f
                    );
                    answerLabel.TextFrame.TextRange.Text = $"{(char)('A' + result.answer_order)}";
                    answerLabel.TextFrame.TextRange.Font.Size = (int)(slideHeight * 0.028f);
                    answerLabel.TextFrame.TextRange.Font.Bold = Office.MsoTriState.msoTrue;
                    answerLabel.TextFrame.TextRange.Font.Color.RGB =
                        System.Drawing.ColorTranslator.ToOle(
                            result.is_correct ?
                                System.Drawing.Color.FromArgb(46, 204, 113) :
                                System.Drawing.Color.FromArgb(52, 73, 94)
                        );
                    answerLabel.TextFrame.TextRange.ParagraphFormat.Alignment =
                        PowerPoint.PpParagraphAlignment.ppAlignCenter;

                    // Add checkmark for correct answer
                    if (result.is_correct)
                    {
                        var checkmark = slide.Shapes.AddTextbox(
                            Office.MsoTextOrientation.msoTextOrientationHorizontal,
                            x, chartStartY + barAreaHeight + slideHeight * 0.055f, barWidth * 0.85f, slideHeight * 0.035f
                        );
                        checkmark.TextFrame.TextRange.Text = "✓";
                        checkmark.TextFrame.TextRange.Font.Size = (int)(slideHeight * 0.025f);
                        checkmark.TextFrame.TextRange.Font.Bold = Office.MsoTriState.msoTrue;
                        checkmark.TextFrame.TextRange.Font.Color.RGB =
                            System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(46, 204, 113));
                        checkmark.TextFrame.TextRange.ParagraphFormat.Alignment =
                            PowerPoint.PpParagraphAlignment.ppAlignCenter;
                    }

                    x += barWidth + spacing;
                }

                // Add legend at the bottom with dynamic sizing
                float legendStartY = chartStartY + barAreaHeight + slideHeight * 0.11f;

                // Calculate available space and item height to fit all results
                float availableHeight = slideHeight - legendStartY - marginY;
                float legendItemHeight = Math.Min(slideHeight * 0.045f, availableHeight / results.Count);

                // If items would be too small, make them readable minimum size
                if (legendItemHeight < slideHeight * 0.035f)
                {
                    legendItemHeight = slideHeight * 0.035f;
                }

                foreach (var result in results)
                {
                    // Color indicator
                    var colorBox = slide.Shapes.AddShape(
                        Office.MsoAutoShapeType.msoShapeRectangle,
                        marginX, legendStartY + slideHeight * 0.006f, slideHeight * 0.025f, slideHeight * 0.025f
                    );
                    colorBox.Fill.ForeColor.RGB = System.Drawing.ColorTranslator.ToOle(
                        result.is_correct ?
                            System.Drawing.Color.FromArgb(46, 204, 113) :
                            System.Drawing.Color.FromArgb(52, 152, 219)
                    );
                    colorBox.Fill.Solid();
                    colorBox.Line.Visible = Office.MsoTriState.msoFalse;

                    // Answer text - truncate if too long
                    string answerDisplayText = result.answer_text;
                    int maxLength = (int)(slideWidth / 7); // Approximate character limit based on slide width
                    if (answerDisplayText.Length > maxLength)
                    {
                        answerDisplayText = answerDisplayText.Substring(0, maxLength - 3) + "...";
                    }

                    var answerText = slide.Shapes.AddTextbox(
                        Office.MsoTextOrientation.msoTextOrientationHorizontal,
                        marginX + slideHeight * 0.040f, legendStartY, contentWidth * 0.65f, legendItemHeight
                    );
                    answerText.TextFrame.TextRange.Text =
                        $"{(char)('A' + result.answer_order)}. {answerDisplayText}";
                    answerText.TextFrame.TextRange.Font.Size = (int)(slideHeight * 0.018f);
                    answerText.TextFrame.TextRange.Font.Bold = result.is_correct ?
                        Office.MsoTriState.msoTrue : Office.MsoTriState.msoFalse;
                    answerText.TextFrame.TextRange.Font.Color.RGB =
                        System.Drawing.ColorTranslator.ToOle(
                            result.is_correct ?
                                System.Drawing.Color.FromArgb(46, 204, 113) :
                                System.Drawing.Color.FromArgb(52, 73, 94)
                        );
                    answerText.TextFrame.WordWrap = Office.MsoTriState.msoFalse;

                    // Percentage and count
                    var statsText = slide.Shapes.AddTextbox(
                        Office.MsoTextOrientation.msoTextOrientationHorizontal,
                        marginX + contentWidth * 0.70f, legendStartY, contentWidth * 0.25f, legendItemHeight
                    );
                    statsText.TextFrame.TextRange.Text = $"{result.percentage:F1}% ({result.count})";
                    statsText.TextFrame.TextRange.Font.Size = (int)(slideHeight * 0.016f);
                    statsText.TextFrame.TextRange.Font.Bold = Office.MsoTriState.msoTrue;
                    statsText.TextFrame.TextRange.Font.Color.RGB =
                        System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(127, 140, 141));
                    statsText.TextFrame.TextRange.ParagraphFormat.Alignment =
                        PowerPoint.PpParagraphAlignment.ppAlignRight;

                    legendStartY += legendItemHeight;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inserting results: {ex.Message}\n\n{ex.StackTrace}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ClassPointQuiz
{
    public class ApiClient
    {
        private static readonly HttpClient client = new HttpClient();
        private const string BASE_URL = "http://localhost:8000/api";

        static ApiClient()
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        }

        // Request Models
        public class QuizCreateRequest
        {
            public int teacher_id { get; set; }
            public string title { get; set; }
            public string question_text { get; set; }
            public int num_choices { get; set; }
            public bool allow_multiple { get; set; }
            public bool has_correct { get; set; }
            public string quiz_mode { get; set; }
            public bool start_with_slide { get; set; }
            public bool minimize_window { get; set; }
            public int auto_close_minutes { get; set; }
        }

        public class Answer
        {
            public string text { get; set; }
            public int order { get; set; }
            public bool is_correct { get; set; }
        }

        // Response Models
        public class QuizResponse
        {
            public int quiz_id { get; set; }
            public int question_id { get; set; }
            public string message { get; set; }
        }

        public class SessionResponse
        {
            public int session_id { get; set; }
            public string class_code { get; set; }
            public string status { get; set; }
        }

        public class ResultItem
        {
            public string answer_text { get; set; }
            public int answer_order { get; set; }
            public bool is_correct { get; set; }
            public int count { get; set; }
            public double percentage { get; set; }
        }

        public class ResultsResponse
        {
            public List<ResultItem> results { get; set; }
            public int participant_count { get; set; }
            public int total_responses { get; set; }
        }

        public class QuizItem
        {
            public int quiz_id { get; set; }
            public string title { get; set; }
            public int num_choices { get; set; }
            public DateTime created_at { get; set; }
            public int session_count { get; set; }
        }

        public class QuizDetails
        {
            public QuizData quiz { get; set; }
            public QuestionData question { get; set; }
            public List<AnswerData> answers { get; set; }
        }

        public class QuizData
        {
            public int quiz_id { get; set; }
            public int teacher_id { get; set; }
            public string title { get; set; }
            public int num_choices { get; set; }
            public bool allow_multiple { get; set; }
            public bool has_correct { get; set; }
            public bool competition_mode { get; set; }
            public int close_submission_after { get; set; }
        }

        public class QuestionData
        {
            public int question_id { get; set; }
            public int quiz_id { get; set; }
            public string question_text { get; set; }
        }

        public class AnswerData
        {
            public int answer_id { get; set; }
            public int question_id { get; set; }
            public string answer_text { get; set; }
            public int answer_order { get; set; }
            public bool is_correct { get; set; }
        }

        public class SessionInfo
        {
            public int session_id { get; set; }
            public int quiz_id { get; set; }
            public string class_code { get; set; }
            public DateTime started_at { get; set; }
            public DateTime? ended_at { get; set; }
            public bool is_active { get; set; }
        }

        // API Methods
        public static async Task<QuizResponse> CreateQuizAsync(QuizCreateRequest quiz, List<Answer> answers)
        {
            try
            {
                // Build query string for GET parameters
                var queryParams = $"?teacher_id={quiz.teacher_id}" +
                    $"&title={Uri.EscapeDataString(quiz.title)}" +
                    $"&question_text={Uri.EscapeDataString(quiz.question_text)}" +
                    $"&num_choices={quiz.num_choices}" +
                    $"&allow_multiple={quiz.allow_multiple.ToString().ToLower()}" +
                    $"&has_correct={quiz.has_correct.ToString().ToLower()}" +
                    $"&quiz_mode={quiz.quiz_mode}" +
                    $"&start_with_slide={quiz.start_with_slide.ToString().ToLower()}" +
                    $"&minimize_window={quiz.minimize_window.ToString().ToLower()}" +
                    $"&auto_close_minutes={quiz.auto_close_minutes}";

                // Send answers in request body
                var requestBody = new { answers = answers };
                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"{BASE_URL}/quiz/create{queryParams}";
                System.Diagnostics.Debug.WriteLine($"API Call: {url}");
                System.Diagnostics.Debug.WriteLine($"Body: {json}");

                var response = await client.PostAsync(url, content);

                var responseContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Response: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API Error: {response.StatusCode} - {responseContent}");
                }

                return JsonConvert.DeserializeObject<QuizResponse>(responseContent);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Network Error: {ex.Message}. Make sure backend is running!");
            }
            catch (Exception ex)
            {
                throw new Exception($"API Error: {ex.Message}");
            }
        }

        public static async Task<SessionResponse> StartSessionAsync(int quizId)
        {
            try
            {
                var data = new { quiz_id = quizId };
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                System.Diagnostics.Debug.WriteLine($"Starting session for quiz: {quizId}");

                var response = await client.PostAsync($"{BASE_URL}/session/start", content);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API Error: {response.StatusCode} - {responseContent}");
                }

                return JsonConvert.DeserializeObject<SessionResponse>(responseContent);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Network Error: {ex.Message}. Make sure backend is running!");
            }
            catch (Exception ex)
            {
                throw new Exception($"API Error: {ex.Message}");
            }
        }

        public static async Task<ResultsResponse> GetResultsAsync(int sessionId)
        {
            try
            {
                var response = await client.GetAsync($"{BASE_URL}/session/{sessionId}/results");

                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API Error: {response.StatusCode} - {responseContent}");
                }

                return JsonConvert.DeserializeObject<ResultsResponse>(responseContent);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Network Error: {ex.Message}. Make sure backend is running!");
            }
            catch (Exception ex)
            {
                throw new Exception($"API Error: {ex.Message}");
            }
        }

        public static async Task<bool> CloseSessionAsync(int sessionId)
        {
            try
            {
                var response = await client.PostAsync($"{BASE_URL}/session/{sessionId}/close", null);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> CheckHealthAsync()
        {
            try
            {
                var response = await client.GetAsync("http://localhost:8000/health");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Health check: {content}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Health check failed: {ex.Message}");
                return false;
            }
        }

        public static async Task<List<QuizItem>> GetTeacherQuizzesAsync(int teacherId)
        {
            try
            {
                var response = await client.GetAsync($"{BASE_URL}/teacher/{teacherId}/quizzes");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<QuizItem>>(result);
            }
            catch (Exception ex)
            {
                throw new Exception($"API Error: {ex.Message}");
            }
        }

        public static async Task<QuizDetails> GetQuizDetailsAsync(int quizId)
        {
            try
            {
                var response = await client.GetAsync($"{BASE_URL}/quiz/{quizId}");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<QuizDetails>(result);
            }
            catch (Exception ex)
            {
                throw new Exception($"API Error: {ex.Message}");
            }
        }

        public static async Task<SessionInfo> GetSessionInfoAsync(int sessionId)
        {
            try
            {
                var response = await client.GetAsync($"{BASE_URL}/session/{sessionId}/info");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<SessionInfo>(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"API Error getting session info: {ex.Message}");
                return null;
            }
        }
    }
}
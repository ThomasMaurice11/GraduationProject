namespace GP.Services
{
    // DiseasePredictionService.cs
    using System.Net.Http.Json;
    using System.Text.Json;

    public class DiseasePredictionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl; // "http://localhost:5000" or your deployed URL

        public DiseasePredictionService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiBaseUrl = configuration["DiseasePredictionApi:BaseUrl"];
        }

        public async Task<List<DiseasePrediction>> PredictDiseaseAsync(Dictionary<string, int> symptoms)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_apiBaseUrl}/predict", symptoms);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
                return result?.Predictions ?? new List<DiseasePrediction>();
            }
            catch (Exception ex)
            {
                // Handle/log error
                throw;
            }
        }
    }

    public class ApiResponse
    {
        public List<DiseasePrediction> Predictions { get; set; } = new();
    }

    public class DiseasePrediction
    {
        public string Disease { get; set; }
        public float Probability { get; set; }
    }
}

namespace GP.Models
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string ErrorType { get; set; }
        public string Message { get; set; }
        public string? StackTrace { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // For development only
        public Dictionary<string, string>? Errors { get; set; }
    }
}



namespace GP.Exceptions
{
    public class AppException : Exception
    {
        public int StatusCode { get; set; }
        public string ErrorType { get; set; }

        public AppException(string message, int statusCode = 500, string errorType = "Internal Server Error")
            : base(message)
        {
            StatusCode = statusCode;
            ErrorType = errorType;
        }
    }   
}

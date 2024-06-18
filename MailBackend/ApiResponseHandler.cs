namespace MailBackend
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public ApiError Error { get; set; }
    }

    public class ApiError
    {
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}

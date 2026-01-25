namespace EMS.Domain.Entities
{
    public class ExceptionLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public string? StackTrace { get; set; }
        public string? FileName { get; set; }
        public int? LineNumber { get; set; }
        public int StatusCode { get; set; }
    }
}

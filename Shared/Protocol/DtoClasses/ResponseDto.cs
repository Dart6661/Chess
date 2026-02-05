namespace Chess.Shared
{
    public class ResponseDto
    {
        public string? Id { get; set; }
        public Status Status { get; set; }
        public ResponseType Type { get; set; }
        public string? SessionId { get; set; }
        public string? Message { get; set; }
        public string? Data { get; set; }
    }
}
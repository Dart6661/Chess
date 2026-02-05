namespace Chess.Shared
{
    public class RequestDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public RequestType Type { get; set; }
        public string? SessionId { get; set; }
        public string? Data { get; set; }
    }
}
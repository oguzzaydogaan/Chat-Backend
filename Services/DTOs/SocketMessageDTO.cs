namespace Services.DTOs
{
    public class SocketMessageDTO { }
    public class RequestSocketMessageDTO
    {
        public string? Type { get; set; }
        public RequestPayloadDTO Payload { get; set; } = new();
    }

    public class ResponseSocketMessageDTO {
        public string? Type { get; set; }
        public ResponsePayloadDTO Payload { get; set; } = new();
    }
}

namespace Services.DTOs
{
    public class RequestSocketDTO
    {
        public string? Type { get; set; }
        public RequestPayloadDTO Payload { get; set; } = new();
        public UserDTO Sender { get; set; } = new();
    }

    public class ResponseSocketDTO {
        public string? Type { get; set; }
        public ResponsePayloadDTO Payload { get; set; } = new();
        public UserDTO Sender { get; set; } = new();
    }
}

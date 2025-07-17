namespace Services.DTOs
{
    public class RequestPayloadDTO
    {
        public int UserId { get; set; }
        public int ChatId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int? MessageId { get; set; }
        public CreateChatRequestDTO? Chat { get; set; }
    }

    public class ResponsePayloadDTO
    {
        public MessageForChatDTO? Message { get; set; }
        public SocketChatDTO? Chat { get; set; }
        public string? Error { get; set; }
    }
}

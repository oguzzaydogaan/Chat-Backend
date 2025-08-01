namespace Services.DTOs
{
    public class RequestSocketDTO
    {
        public RequestEventType Type { get; set; }
        public RequestPayloadDTO Payload { get; set; } = new();
        public UserDTO Sender { get; set; } = new();
    }

    public class ResponseSocketDTO {
        public ResponseEventType Type { get; set; }
        public ResponsePayloadDTO Payload { get; set; } = new();
        public UserDTO Sender { get; set; } = new();
    }

    public enum RequestEventType
    {
        Message_Send = 0,
        Message_Delete = 1,
        Message_See = 2,
        Chat_Create = 3,
        Chat_AddUser = 4
    }
    public enum ResponseEventType
    {
        Message_Sent,
        Message_Deleted,
        Message_Seen,
        Chat_Created,
        Chat_UserAdded,
        Error
    }
}

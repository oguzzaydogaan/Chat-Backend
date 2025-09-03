namespace Services.DTOs
{
    public class RequestSocketDTO
    {
        public RequestEventType Type { get; set; }
        public RequestPayloadDTO Payload { get; set; } = new();
        public UserDTO Sender { get; set; } = new();
        public ICollection<int> Receivers { get; set; } = new List<int>();
    }

    public class ResponseSocket_ForMessageDTO
    {
        public ResponseEventType Type { get; set; } = ResponseEventType.Message_Received;
        public CreateMessageRequestDTO? Message { get; set; }
        public UserDTO? Sender { get; set; }
    }

    public class ResponseSocket_SFUToken(CallDTO call, string token)
    {
        public ResponseEventType Type { get; set; } = ResponseEventType.Call_SFUTokenReceived;
        public CallDTO Call { get; set; } = call;
        public string Token { get; set; } = token;
    }

    public class ResponseSocketDTO
    {
        public ResponseEventType Type { get; set; }
        public ResponsePayloadDTO Payload { get; set; } = new();
        public UserDTO Sender { get; set; } = new();
    }

    public enum RequestEventType
    {
        Message_Send,
        Message_Delete,
        Message_See,
        Chat_Create,
        Chat_AddUser,
        Call_Offer,
        Call_Cancel,
        Call_Accept,
        Call_Reject,
        Call_End,
        Call_Ice,
    }
    public enum ResponseEventType
    {
        Message_Received,
        Message_Saved,
        Message_Deleted,
        Message_Seen,
        Chat_Created,
        Chat_UserAdded,
        Call_Offered,
        Call_Cancelled,
        Call_Accepted,
        Call_Rejected,
        Call_Ended,
        Call_SFUTokenReceived,
        Call_Ice,
        Error
    }
}

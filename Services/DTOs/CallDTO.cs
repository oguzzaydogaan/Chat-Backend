using Repositories.Entities;

namespace Services.DTOs
{
    public class CallDTO
    {
        public int Id { get; set; }
        public UserDTO Caller { get; set; } = new();
        public UserDTO Callee { get; set; } = new();
        public DateTime CallTime { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int DurationInSeconds { get; set; }
        public CallAnswerType AnswerType { get; set; }
    }

    public class CreateCallDTO
    {
        public int CallerId { get; set; }
        public int CalleeId { get; set; }
    }

    public class CallOfferDTO
    {
        public string Type { get; set; } = string.Empty;
        public string? Sdp { get; set; }
        public string? Candidate { get; set; }
        public string? SdpMid { get; set; }
        public int? SdpMLineIndex { get; set; }
        public int? CallId { get; set; }
    }
}

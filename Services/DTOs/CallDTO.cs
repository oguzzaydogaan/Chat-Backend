using Repositories.Entities;

namespace Services.DTOs
{
    public class CallDTO
    {
        public int Id { get; set; }
        public UserDTO Caller { get; set; } = new();
        public List<UserDTO> Callees { get; set; } = new();
        public DateTime CallTime { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int DurationInSeconds { get; set; }
        public CallAnswerType AnswerType { get; set; }
        public string? SFUToken { get; set; }
        public string? SFURoom { get; set; }
    }

    public class CreateCallReqDTO
    {
        public int CallerId { get; set; }
        public List<int> CalleesIds { get; set; } = new();
        public DateTime CallTime { get; set; }
    }
}

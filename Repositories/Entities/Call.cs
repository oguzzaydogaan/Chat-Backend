namespace Repositories.Entities
{
    public class Call : BaseEntity
    {
        public int CallerId { get; set; }
        public User? Caller { get; set; }
        public int CalleeId { get; set; }
        public User? Callee { get; set; }
        public DateTime CallTime { get; set; } = DateTime.UtcNow;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int DurationInSeconds { get; set; } = 0;
        public CallAnswerType AnswerType { get; set; } = CallAnswerType.None;

    }

    public enum CallAnswerType
    {
        None,
        Cancelled,
        Accepted,
        Rejected
    }
}

namespace Services.DTOs
{
    public class CallDTO
    {
        public string Type { get; set; } = string.Empty;          // "offer", "answer", "candidate"
        public string TargetUserId { get; set; } = string.Empty;  // karşı taraf
        public string SourceUserId { get; set; } = string.Empty;  // gönderen taraf

        // SDP için
        public string? Sdp { get; set; }

        // ICE için
        public string? Candidate { get; set; }
        public string? SdpMid { get; set; }
        public int? SdpMLineIndex { get; set; }
    }

}

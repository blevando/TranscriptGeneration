namespace TranscriptGeneration.Models.Entities
{
    public class Sessions
    {
        public  int Id { get; set; }
        public  int SessionId { get; set; }
        public string? SessionName { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedDate { get; set; }  

    }
}

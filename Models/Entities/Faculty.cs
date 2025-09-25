namespace TranscriptGeneration.Models.Entities
{
    public class Faculty
    {
        public int Id { get; set; }
        public int FacultyId { get; set; }
        public string? FacultyName { get; set; }
        public string? FacultyCode { get; set; }
        public int? Status { get; set; }
        public string? DeansName { get; set; }
        public string? FMatricName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifyBy { get; set; }
        public DateTime? ModifyDate { get; set; }
      

    }
}

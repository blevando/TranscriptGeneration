namespace TranscriptGeneration.Models.Dto
{
    public class DepartmentResultDto
    {
        public int SessionId { get; set; }

        public string? SessionName { get; set; }
        public int SemesterId { get; set; }
        public int DepartmentId { get; set; }
        public string? ClassCode { get; set; }
        public string? FacultyName { get; set; }

        public string? ProgramName { get; set; } //e.g., Executive MBA
        public string? InstitutionAdress { get; set; }
        public string? InstitutionName { get; set; }
        public string? InstitutionLogo { get; set; }


    }
}

namespace TranscriptGeneration.Models.Entities
{
    public class StudentInfo
    {
        public int Id { get; set; }
        public string? MatricNumber { get; set; }  
        public string? Surname { get; set; }
        public string? OtherNames { get; set; }
        public string? Gender { get; set; } 
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int FacultyId { get; set; }
        public int DepartmentId { get; set; }
        public int StudentStatusId { get; set; }
        public string?  AdmittedSessionId { get; set; }


    }



}

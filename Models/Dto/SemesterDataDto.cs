using System.Collections.Specialized;

namespace TranscriptGeneration.Models.Dto
{
    public class SemesterDataDto
    {
        public Guid Id { get; set; }
        public string? SemesterTitle { get; set; }
        public List<SemesterResult> SemesterResults { get; set; } 
        public HybridDictionary? SemesterGPA { get; set; }

    }

    public class SemesterResult
    {
        public string? SN { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseTitle { get; set; }
        public string? CourseUnit { get; set; }
        public string? Grade { get; set; }
        public string? GradePoint { get; set; }
    }
}

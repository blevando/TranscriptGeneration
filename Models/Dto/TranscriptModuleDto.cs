namespace TranscriptGeneration.Models.Dto
{
    public class TranscriptModuleDto
    {
        public string Title { get; set; }
        public ResultRecord Header { get; set; }
        public List<ResultRecord> Body { get; set; }
        public string Footer { get; set; }
    }

    public class ResultRecord
    {
        public string SN { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseTitle { get; set; }
        public string? CourseUnit { get; set; }
        public string? Grade { get; set; }
        public string? GradePoint { get; set; }
    }
}

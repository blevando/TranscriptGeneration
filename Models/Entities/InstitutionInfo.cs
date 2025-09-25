using System.Collections.Specialized;
using TranscriptGeneration.Models.Dto;

namespace TranscriptGeneration.Models.Entities
{
    public class InstitutionInfo
    {
        public string? SerialNumber { get; set; }
        public string? UniversityLogo { get; set; }
        public string? UniversityName { get; set; }
        public string? UniversityOffice { get; set; }
        public string? UniversityAddress { get; set; }
        public string? FinalGPA { get; set; }
        public string? Signature { get; set; }
        public StudentInfo? StudentInfo { get; set; }
        public List<TranscriptModuleDto>? SemesterData { get; set; }

    }
}

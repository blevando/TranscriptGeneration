using System.Collections.Generic;
using System.Collections;
using System.Collections.Specialized;

namespace TranscriptGeneration.Models.Dto
{
    public class InstitutionInfoDto
    {
        public string? UniversityLogo { get; set; }
        public string? UniversityName { get; set; }
        public string? UniversityOffice { get; set; }
        public string? UniversityAddress { get; set; }
        public string? FinalGPA { get; set; }
        public string? Signature { get; set; }
        public HybridDictionary StudentInfo { get; set; } 
        public SemesterDataDto[] SemesterData { get; set; }

    }

}

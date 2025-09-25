using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace TranscriptGeneration.Models.Entities
{
    public class Programmes
    {

        [Key]
        public int ProgrammeId { get; set; }
        public string? ProgrammeName { get; set; }
        public string? ProgrammeCode { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ApplicantBPCode { get; set; }
        public string? StudentBPCode { get; set; }
        public string? ApplicantAcceptBPCode { get; set; }
    }   

    
}

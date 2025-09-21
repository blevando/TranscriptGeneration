using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResultManager.Processor.Models.Entities
{
    public class StudentResultSemesterSummary
    {
        [Key]
        public int Id { get; set; }
        public int SessionId { get; set; }
        public int DepartmentId { get; set; }
        public int SemesterId { get; set; }
        public string? ClassCode { get; set; }
        public string? MatricNumber { get; set; }
        public int StudentAttendant { get; set; }
        public double TGP { get; set; }
        public double TCR { get; set; }
        public double TCE { get; set; }
        public string? GPA { get; set; }
        public string? Remarks { get; set; }
        public string? ProcessedOn { get; set; }
        public int Status { get; set; }
    }
}

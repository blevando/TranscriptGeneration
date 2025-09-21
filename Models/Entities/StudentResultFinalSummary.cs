using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResultManager.Processor.Models.Entities
{
    public class StudentResultFinalSummary
    {
        [Key]
        public int Id { get; set; }
        public int DepartmentId { get; set; }
        public string? Department { get; set; }
        public string? ClassCode { get; set; }
        public string? MatricNumber { get; set; }
        public string? Name { get; set; }
        public int StudentAttendant { get; set; }
        public double CTGP { get; set; }
        public double CTCR { get; set; }
        public double CTCE { get; set; }
        public string? CGPA { get; set; }
        public string? Remarks { get; set; }
        public string? ClassOfDegree { get; set; }
        public int IsGraduated { get; set; }
        public string? GraduatedOn { get; set; }
        public string? ProcessedOn { get; set; }
        public int Status { get; set; }
    }
}

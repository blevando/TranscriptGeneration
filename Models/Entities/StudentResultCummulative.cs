using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResultManager.Processor.Models.Entities
{
    public class StudentResultCummulative
    {
        [Key]
        public int Id { get; set; }
        public int SessionId { get; set; }
        public int SemesterId { get; set; }
        public int DepartmentId { get; set; }
        public string? ClassCode { get; set; }
        public string? MatricNumber { get; set; }
        public Decimal TCR { get; set; }
        public Decimal TCE { get; set; }
        public Decimal TGP { get; set; }
        public Decimal GPA { get; set; }
        public Decimal PTCR { get; set; }
        public Decimal PTCE { get; set; }
        public Decimal PTGP { get; set; }
        public Decimal PGPA { get; set; }
        public Decimal CTCR { get; set; }
        public Decimal CTCE { get; set; }
        public Decimal CTGP { get; set; }
        public Decimal CGPA { get; set; }
        public string? ProcessedOn { get; set; }
        public string? Remarks { get; set; }
        public int Status { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResultManager.Processor.Models.DTO
{
   public class StudentResultDto
    {
        public long Id { get; set; }
        public string? Names { get; set; }
        public int SessionId { get; set; }
        public int DepartmentId { get; set; }
        public int SemesterId { get; set; }
        public string? ClassCode { get; set; }
        public string? MatricNumber { get; set; }
        public int Attendance { get; set; }
        public double TGP { get; set; }
        public string? GPA { get; set; }
        public double TCU { get; set; }
        public double TCR { get; set; } // Total Credit Registered
        public double TCE { get; set; } // Total Credit Earned
        public string? Remarks { get; set; }
        public string? ProcessedOn { get; set; }
        public int Status { get; set; }       
        public string? Remark { get; set; }
       
    }
}

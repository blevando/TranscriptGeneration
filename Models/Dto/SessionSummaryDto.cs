using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResultManager.Processor.Models.DTO
{
    public class SessionSummaryDto
    {
        
        public int Id { get; set; }
        public long SessionId { get; set; }
        public long DepartmentId { get; set; }
        public string? ClassCode { get; set; }
        public string? MatricNumber { get; set; }
        public double CTCU { get; set; }
        public double CTGP { get; set; }
        public string CGPA { get; set; }
        public string? Remark { get; set; }      
        public double CTCR { get; set; }
        public double CTCE { get; set; }
        public string? Remarks { get; set; }
        public string? ProcessedOn { get; set; }
        public int Status { get; set; }

    }
}

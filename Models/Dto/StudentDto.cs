using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResultManager.Processor.Models.DTO
{
    public class StudentDto
    {
        
        public long SessionId { get; set; }
        public long DepartmentId { get; set; }
        public string? ClassCode { get; set; }
        public string? MatricNumber { get; set; }
        public double CTCU { get; set; }
        public double CTGP { get; set; }
        public string CGPA { get; set; }
        public string? ClassOfDegree { get; set; }
        public string? Remark { get; set; }
        public int GraduationYear { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ResultManager.Processor.Models;

namespace ResultManager.Processor.Models.Entities
{
    public class StudentResultDetails  
    {
        public int Id { get; set; }
        public string MatricNumber { get; set; }
        public string CourseCode { get; set; }
        public string Score { get; set; }
        public string Grade { get; set; }
        public int Units { get; set; }
        public double GradePoint { get; set; } = 0.0;
        public double IGP { get; set; }
        public string SessionId { get; set; }
        public int Semester { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ResultManager.Processor.Models.DTO
{
    public class StudentDetailResultDto
    {
        public long Id { get; set; }
        public long SessionId { get; set; }
        public long DepartmentId { get; set; }
        public long SemesterId { get; set; }
        public string? ClassCode { get; set; }
        public string? MatricNumber { get; set; }
        public string? Names { get; set; }
        public string? CourseCode { get; set; }
        public string CourseType { get; internal set; }
        public string? CourseTitle { get; set; }
        public double Score { get; set; }
        public string? Grade { get; set; }
        public int Units { get; set; }
        public double Points { get; set; }
        public double GradePoints { get; set; } // Individual grade points
        public string ProcessedOn { get; set; }
        public int Status { get; set; }     
        public double GradePoint { get; set; }
        public double IGP { get; set; }
    }
    public class GroupUnit
    {
        public string MatricNumber { get; set; }
        public int Unit { get; set; }
    }
}

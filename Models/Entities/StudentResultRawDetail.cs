using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResultManager.Processor.Models.Entities
{
    public class StudentResultRawDetail
    {
        // Raw unprocessed results
        [Key]
        public int Id { get; set; }
        public int SessionId { get; set; }
        public int DepartmentId { get; set; }
        public int SemesterId { get; set; }
        public string? ClassCode { get; set; }
        public string? MatricNumber { get; set; }
        public string? Name { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseTitle { get; set; }
        public string? CourseType { get; set; }

        public int Units { get; set; }
        public decimal Score { get; set; }
        public string? Grade { get; set; }
        public int Points { get; set; }
        public int GradePoints { get; set; }     
        public string? CreatedBy { get; set; }
        public string? CreatedOn { get; set; }      
        public int Status { get; set; }
    }
}

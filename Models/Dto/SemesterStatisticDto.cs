using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResultManager.Processor.Models.DTO
{
    public class SemesterStatisticDto
    {
        public int Id { get; set; }    
        public string? CourseCode { get; set; }
        public string? CourseTitle { get; set; }
        public double PercentagePass { get; set; }
    
    }
}

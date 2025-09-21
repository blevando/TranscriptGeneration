using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResultManager.Processor.Models.DTO
{
    public class SemesterDemographicDto
    {
        public int Id { get; set; }
        public int SummaryCode { get; set; }
        public string? ResultSummary { get; set; }
        public int ResultCount { get; set; }
    }
}

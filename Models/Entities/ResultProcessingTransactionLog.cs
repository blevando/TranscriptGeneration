using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResultManager.Processor.Models.Entities
{
    public class ResultProcessingTransactionLog
    {
        public long Id { get; set; }
        public int SessionId { get; set; }
        public int DepartmentId { get; set; }
        public int SemesterId { get; set; }
        public string CreatedBy { get; set; } // the exam officer
        public DateTime CreatedOn { get; set; } 
        public DateTime ProcessedOn { get; set; }
        public bool Status { get; set; }

    }
}

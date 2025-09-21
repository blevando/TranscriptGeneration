using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResultManager.Processor.Models.DTO
{

    public class ResultDto
    {
        //[Column(TypeName="Numeric(18,0)")]
        public long Id { get; set; }
        public int sessionId { get; set; }
        public int departmentId { get; set; }
        public int semesterId { get; set; }
        public string? ClassCode { get; set; }

    }
    public class ResultDt1
    {
        //[Column(TypeName="Numeric(18,0)")]
        public long ID { get; set; }
        public int SessionId { get; set; }
        public int DepartmentId { get; set; }
        public int SemesterId { get; set; }
        public string? ClassCode { get; set; }
        //public long Id { get; set; }
        //public int sessionId { get; set; }
        //public int departmentId { get; set; }
        //public int semesterId { get; set; }
        //public string? ClassCode { get; set; }

    }



}

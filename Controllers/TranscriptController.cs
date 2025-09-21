//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using TranscriptGeneration.Mangers;
//using TranscriptGeneration.Models.Dto;

//namespace TranscriptGeneration.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class TranscriptController : ControllerBase
//    {
//        private readonly TranscriptManager _trans;

//        public TranscriptController(TranscriptManager trans)
//        {
//            _trans = trans;
//        }


//        [HttpGet]
//        [Route("GetTranscriptReportByStudentId/{studentId}")]
//        public async Task<IActionResult> GetTranscriptReportByStudentId(string studentId)
//        {
//            var res = await _trans.GetTranscriptReportAsync(studentId);
//           return StatusCode(res.StatusCode, res);
//        }

//        [HttpPost]
//        [Route("GetDepartmentResult/{ID}")]
//        public async Task<IActionResult> GetDepartmentResult(int Id, [FromBody] DepartmentResultDto model)
//        {
//            var res = await _trans.GetDepartmentResultAsync(model);
//            return StatusCode(res.StatusCode, res);
//        }

//        [HttpGet]
//        [Route("GetTranscriptReportBySessionDepartmentSemester")]
//        public async Task<IActionResult> GetTranscriptReportBySessionDepartmentSemester(int sessionId, int departmentId, int semesterId)
//        {
//            // Simulate fetching transcript report by session, department, and semester
//            // In a real application, you would call a service or repository method here
//            var response = new
//            {
//                StatusCode = 200,
//                Message = "Transcript report fetched successfully",
//                Data = new { SessionId = sessionId, DepartmentId = departmentId, SemesterId = semesterId, ReportDetails = "Sample Report Details" }
//            };
//            return Ok(response);
//        }

//        [HttpGet]
//        [Route("CreatePdfFromHTML/{studentId}/{departmentId}")]
//        public async Task<IActionResult> CreatePdfFromHTMLAsync(string html, int paperSize)
//        {
//            var res = await _trans.CreatePdfFromHTML(html, paperSize);

//            return StatusCode(res.StatusCode, res);
//        }

//        [HttpGet]
//        [Route("GetFinalGPACGP/{studentId}/{departmentId}")]
//        public  async Task<IActionResult> GetFinalGPACGPAsync(string studentId, int DepartmentId)
//        {
//            var res = await _trans.GetFinalGPACGPAsync(studentId, DepartmentId);
//            return StatusCode(res.StatusCode, res);
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetSemesterGPAsync(string studentId, int SessionId, int DepartmentId, int SemesterId)
//        {
//            var res = await  _trans.GetSemesterGPAsync(studentId, SessionId, DepartmentId, SemesterId);
//            return StatusCode(res.StatusCode, res);
//        }

//        [HttpGet]
//        [Route("GetSemesterResult/{studentId}/{SessionId}/{DepartmentId}/{SemesterId}")]
//        public async Task<IActionResult> GetSemesterResultAsync(string studentId, int SessionId, int DepartmentId, int SemesterId)
//        {
//            var res = await _trans.GetSemesterResultAsync(studentId, SessionId, DepartmentId, SemesterId);
//            return StatusCode(res.StatusCode, res);
//        }

//        [HttpGet]
//        [Route("GetSessionCGP/{studentId}/{SessionId}/{DepartmentId}")]
//        public async Task<IActionResult> GetSessionCGPAsync(string studentId, int SessionId, int DepartmentId)
//        {
//            var res = await _trans.GetSessionCGPAsync(studentId, SessionId, DepartmentId);
//            return StatusCode(res.StatusCode, res);
//        }

//        [HttpGet]
//        [Route("GetTranscriptReport/{studentId}")]
//        public async Task<IActionResult> GetTranscriptReportAsync(string studentId)
//        {
//            var res = await _trans.GetTranscriptReportAsync(studentId);
//            return StatusCode(res.StatusCode, res);

//        }
//        [HttpGet]
//        [Route("GetTranscriptReportAsync/{SessionId}/{DepartmentId}/{SemesterId}")]
//        public async Task<IActionResult> GetTranscriptReportAsync(int SessionId, int DepartmentId, int SemesterId)
//        {
//            var res = await _trans.GetTranscriptReportAsync(SessionId, DepartmentId, SemesterId);
//            return StatusCode(res.StatusCode, res);
//        }




//    }
//}

using Microsoft.AspNetCore.Mvc;
using TranscriptGeneration.Mangers;
using TranscriptGeneration.Models.Dto;

namespace TranscriptGeneration.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TranscriptController : ControllerBase
    {
        private readonly TranscriptManager _trans;

        public TranscriptController(TranscriptManager trans)
        {
            _trans = trans;
        }

        // GET: Get transcript report by student ID
        [HttpGet("GetTranscriptReportByStudentId/{studentId}")]
        public async Task<IActionResult> GetTranscriptReportByStudentId(string studentId)
        {
            var res = await _trans.GetTranscriptReportAsync(studentId);
            return StatusCode(res.StatusCode, res);
        }

        // GET: Full transcript report by session, department, and semester
        [HttpGet("GetTranscriptReportBySessionDepartmentSemester")]
        public async Task<IActionResult> GetTranscriptReportBySessionDepartmentSemester(
            [FromQuery] int sessionId,
            [FromQuery] int departmentId,
            [FromQuery] int semesterId)
        {
            var res = await _trans.GetTranscriptReportAsync(sessionId, departmentId, semesterId);
            return StatusCode(res.StatusCode, res);
        }

        // POST: Get result for department (filtered)
        [HttpPost("GetDepartmentResult")]
        public async Task<IActionResult> GetDepartmentResult( [FromBody] DepartmentResultDto model)
        {
            var res = await _trans.GetDepartmentResultAsync(model);
            return StatusCode(res.StatusCode, res);
        }

        // GET: Create PDF from HTML
        [HttpGet("CreatePdfFromHTML")]
        public async Task<IActionResult> CreatePdfFromHTMLAsync([FromQuery] string html, [FromQuery] int paperSize)
        {
            var res = await _trans.CreatePdfFromHTML(html, paperSize);
            return StatusCode(res.StatusCode, res);
        }

        // GET: Final GPA/CGPA
        [HttpGet("GetFinalGPACGP/{studentId}/{departmentId}")]
        private async Task<IActionResult> GetFinalGPACGPAsync(string studentId, int departmentId)
        {
            var res = await _trans.GetFinalGPACGPAsync(studentId, departmentId);
            return StatusCode(res.StatusCode, res);
        }

        // GET: Semester GP
        [HttpGet("GetSemesterGP")]
        private async Task<IActionResult> GetSemesterGPAsync(
            [FromQuery] string studentId,
            [FromQuery] int sessionId,
            [FromQuery] int departmentId,
            [FromQuery] int semesterId)
        {
            var res = await _trans.GetSemesterGPAsync(studentId, sessionId, departmentId, semesterId);
            return StatusCode(res.StatusCode, res);
        }

        // GET: Semester result
        [HttpGet("GetSemesterResult/{studentId}/{sessionId}/{departmentId}/{semesterId}")]
        private async Task<IActionResult> GetSemesterResultAsync(string studentId, int sessionId, int departmentId, int semesterId)
        {
            var res = await _trans.GetSemesterResultAsync(studentId, sessionId, departmentId, semesterId);
            return StatusCode(res.StatusCode, res);
        }

        // GET: Session CGPA
        [HttpGet("GetSessionCGP/{studentId}/{sessionId}/{departmentId}")]
        private async Task<IActionResult> GetSessionCGPAsync(string studentId, int sessionId, int departmentId)
        {
            var res = await _trans.GetSessionCGPAsync(studentId, sessionId, departmentId);
            return StatusCode(res.StatusCode, res);
        }
    }
}

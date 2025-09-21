using TranscriptGeneration.Models.Dto;
using TranscriptGeneration.Models.Entities;
using TranscriptGeneration.Services.Interface;

namespace TranscriptGeneration.Mangers
{
    public class TranscriptManager: ITranscriptReport
    {
        private readonly ITranscriptReport _transcript;
        public TranscriptManager(ITranscriptReport transcript)
        {
            _transcript = transcript;
        }

        public Task<GeneralResponse> CreatePdfFromHTML(string html, int paperSize)
        {
            var res = _transcript.CreatePdfFromHTML(html, paperSize);

            return res;
        }

        public Task<GeneralResponse> GetFinalGPACGPAsync(string studentId, int DepartmentId)
        {
            var res = _transcript.GetFinalGPACGPAsync( studentId,  DepartmentId);
            return res;
        }

        public Task<GeneralResponse> GetSemesterGPAsync(string studentId, int SessionId, int DepartmentId, int SemesterId)
        {
            var res = _transcript.GetSemesterGPAsync(studentId, SessionId, DepartmentId, SemesterId);
            return res;
        }

        public Task<GeneralResponse> GetSemesterResultAsync(string studentId, int SessionId, int DepartmentId, int SemesterId)
        {
            var res = _transcript.GetSemesterResultAsync(studentId, SessionId, DepartmentId, SemesterId);
            return res;
        }

        public Task<GeneralResponse> GetSessionCGPAsync(string studentId, int SessionId, int DepartmentId)
        {
            var res = _transcript.GetSessionCGPAsync(studentId, SessionId, DepartmentId);
            return res;
        }

        public async Task<GeneralResponse> GetTranscriptReportAsync(string studentId)
        {
            var res =await _transcript.GetTranscriptReportAsync(studentId);
            return res;

        }

        public Task<GeneralResponse> GetTranscriptReportAsync(int SessionId, int DepartmentId, int SemesterId)
        {
            var res = _transcript.GetTranscriptReportAsync(SessionId, DepartmentId, SemesterId);
            return res;
        }

        public Task<GeneralResponse> GetDepartmentResultAsync(DepartmentResultDto model)
        {
            var res = _transcript.GetDepartmentResultAsync(model);
            return res;
        }

        public Task<GeneralResponse> GetStudentCertificateAsync(StudentCertificateDto model)
        {
            throw new NotImplementedException();
        }

        public Task<GeneralResponse> GetManagementReportAsync(DepartmentResultDto model)
        {
            throw new NotImplementedException();
        }
    }
}

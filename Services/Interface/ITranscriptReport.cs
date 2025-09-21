using TranscriptGeneration.Models.Dto;

namespace TranscriptGeneration.Services.Interface
{
    public interface ITranscriptReport
    {
        Task<GeneralResponse> GetTranscriptReportAsync(string studentId);
        Task<GeneralResponse> GetTranscriptReportAsync(int SessionId, int DepartmentId, int SemesterId);
        Task<GeneralResponse> GetSemesterResultAsync(string studentId, int SessionId, int DepartmentId, int SemesterId);

        Task<GeneralResponse> GetSemesterGPAsync(string studentId, int SessionId, int DepartmentId, int SemesterId);
        Task<GeneralResponse> GetSessionCGPAsync(string studentId, int SessionId, int DepartmentId);
        Task<GeneralResponse> GetFinalGPACGPAsync(string studentId, int DepartmentId);
       
        Task<GeneralResponse> CreatePdfFromHTML(string html, int paperSize);// Paper Size is 3 or 4

        Task<GeneralResponse> GetDepartmentResultAsync(DepartmentResultDto model);// Paper Size is 3 or 4
        Task<GeneralResponse> GetStudentCertificateAsync(StudentCertificateDto model);

        Task<GeneralResponse> GetManagementReportAsync(DepartmentResultDto model);

        // Create PDF from  Task<GeneralResponse> GetTranscriptReportAsync(string studentId);



    }
}

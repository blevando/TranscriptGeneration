namespace TranscriptGeneration.Models.Dto
{
    public class GeneralResponse
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }
      
    }
}

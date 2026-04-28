namespace CodePulse.API.Models.DTO
{
    public class UploadImageRequest
    {
        public IFormFile File { get; set; }
        public string Title { get; set; }
        public string FileName { get; set; }
    }
}

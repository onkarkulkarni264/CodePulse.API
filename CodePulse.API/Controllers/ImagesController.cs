
using CodePulse.API.Models.Domain;
using CodePulse.API.Models.DTO;
using CodePulse.API.Repositories.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CodePulse.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IImageRepository _imageRepository;

        public ImagesController(IImageRepository imageRepository)
        {
            _imageRepository = imageRepository;
        }
        //url [GET]: {apibaseurl}/api/images
        [HttpGet]
        public async Task<IActionResult> GetAllImages()
        {
            var Response = await _imageRepository.GetAll();
            if(Response is not null)
            {
                return Ok(Response);
            }
            return BadRequest();
        }
        //url [POST]: {apibaseurl}/api/images
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImage([FromForm] UploadImageRequest request)
        {
            ValidateFile(request.File);
            if (ModelState.IsValid)
            {
                var blogImage = new BlogImage
                {
                    FileExtension = Path.GetExtension(request.File.FileName),
                    Title = request.Title,
                    FileName = request.FileName,
                    DateCreated = DateTime.UtcNow
                };
                var blogImageDTO = await _imageRepository.Upload(request.File, blogImage);
                return Ok(blogImageDTO);
            }
            return BadRequest(ModelState);
        }
        private void ValidateFile(IFormFile file)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            if(file.Length > 10485760)  {
                ModelState.AddModelError("file", "File size cannot be greater that 10MB");
            }
            if (!allowedExtensions.Contains(Path.GetExtension(file.FileName).ToLower()))
            {
                ModelState.AddModelError("file", "Unsupported file format.");
            }
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CodePulse.API.Models.Domain;
using CodePulse.API.Repositories.Interface;

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
        public async Task<IActionResult> UploadImage([FromForm]IFormFile file, [FromForm] string title, [FromForm] string fileName)
        {
            ValidateFile(file);
            if (ModelState.IsValid)
            {
                var blogImage =new BlogImage
                {
                    FileExtension = Path.GetExtension(file.FileName),
                    Title = title,
                    FileName = fileName,
                    DateCreated = DateTime.Now
                };
                var BlogImageDTO =  await _imageRepository.Upload(file, blogImage);
                return Ok(BlogImageDTO);
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

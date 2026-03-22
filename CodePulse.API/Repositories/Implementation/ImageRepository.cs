using CodePulse.API.Data;
using CodePulse.API.Models.Domain;
using CodePulse.API.Models.DTO;
using CodePulse.API.Repositories.Interface;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;

namespace CodePulse.API.Repositories.Implementation
{
    public class ImageRepository : IImageRepository
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _applicationDbContext;

        public ImageRepository(IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor, ApplicationDbContext applicationDbContext)
        {
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _applicationDbContext = applicationDbContext;
        }

        public async Task<IEnumerable<BlogImageDTO>> GetAll()
        {
            var BlogImages =await _applicationDbContext.BlogImages.ToListAsync();
            var response = new List<BlogImageDTO>();
            foreach (var Image in BlogImages)
            {
                response.Add(new BlogImageDTO
                {
                    Id = Image.Id,
                    Title = Image.Title,
                    FileName = Image.FileName,
                    FileExtension = Image.FileExtension,
                    Url = Image.Url,
                    DateCreated = Image.DateCreated
                });
            }
            return response;
        }

        public async Task<BlogImageDTO> Upload(IFormFile file, BlogImage blogImage)
        {
            var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "Images");
            if (!Directory.Exists(imagesFolder))
            {
                Directory.CreateDirectory(imagesFolder);
            }
            var localPath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "Images", $"{blogImage.FileName}{blogImage.FileExtension}"));
            using (var stream = new FileStream(localPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var httprequest = _httpContextAccessor.HttpContext?.Request;
            var urlPath = $"{httprequest?.Scheme}://{httprequest?.Host}/Images/{blogImage.FileName}{blogImage.FileExtension}";

            blogImage.Url = urlPath;

            await _applicationDbContext.BlogImages.AddAsync(blogImage);
            await _applicationDbContext.SaveChangesAsync();

            BlogImageDTO blogImageDTO = new BlogImageDTO
            {
                Id = blogImage.Id,
                FileName = blogImage.FileName,
                FileExtension = blogImage.FileExtension,
                Title = blogImage.Title,
                Url = blogImage.Url,
                DateCreated = blogImage.DateCreated
            };
            return blogImageDTO;
        }
    }
}

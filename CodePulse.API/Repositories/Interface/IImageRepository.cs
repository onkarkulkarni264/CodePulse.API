using CodePulse.API.Models.Domain;
using CodePulse.API.Models.DTO;

namespace CodePulse.API.Repositories.Interface
{
    public interface IImageRepository
    {
        Task<BlogImageDTO> Upload(IFormFile file,BlogImage blogImage);
        Task<IEnumerable<BlogImageDTO>> GetAll();
    }
}

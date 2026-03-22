using CodePulse.API.Models.Domain;
using CodePulse.API.Models.DTO;

namespace CodePulse.API.Repositories.Interface
{
    public interface IBlogPostRepository
    {
       Task<BlogPostsDTO> createAsync(CreateBlogPostsRequestDto blogPost);
       Task<IEnumerable<BlogPostsDTO>> getAllAsync(string? SearchText = null, string? SortBy = null, string? SortDirection = null, int? PageSize = 5, int? PageNumber = 1);
       Task<int?> GetCount(string? SearchText = null);
       Task<BlogPostsDTO?> GetByIdAsync(Guid id);
       Task<BlogPostsDTO?> UpdateBlogPosts(Guid id, UpdateBlogPostsRequestDto request);
       Task<List<BlogPostsDTO?>> DeleteBlogPost(Guid id);
       Task<BlogPostsDTO?> GetByURLHandleAsync(string urlHandle);
    }
}

using CodePulse.API.Models.Domain;
using CodePulse.API.Models.DTO;
using CodePulse.API.Repositories.Implementation;
using CodePulse.API.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CodePulse.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogPostsController : ControllerBase
    {
        private readonly IBlogPostRepository _BlogPostRepository;
        private readonly ICategoryRepository _CategoryRepository;
        public BlogPostsController(IBlogPostRepository BlogPostRepository,ICategoryRepository CategoryRepository)
        {

            _BlogPostRepository = BlogPostRepository;
            _CategoryRepository= CategoryRepository;
        }
        [HttpPost]
        [Authorize(Roles = "Writer")]
        //url : {apibaseurl}/api/blogposts
        public async Task<IActionResult> CreateBlogPost([FromBody] CreateBlogPostsRequestDto CategoryBlogPostsDTO)
        {
           BlogPostsDTO Response = await _BlogPostRepository.createAsync(CategoryBlogPostsDTO);
           return Ok(Response);
        }
        //url : {apibaseurl}/api/blogposts
        [HttpGet]
        public async Task<IActionResult> getAllBlogPosts([FromQuery] string? SearchText = null, [FromQuery] string? SortBy = null, [FromQuery] string? SortDirection = null, [FromQuery] int? PageSize = null, [FromQuery] int? PageNumber = null)
        {
            var response= await _BlogPostRepository.getAllAsync(SearchText, SortBy, SortDirection, PageSize, PageNumber);
            return Ok(response);
        }
        [HttpGet]
        [Route("count")]
        public async Task<IActionResult> GetCount([FromQuery] string? SearchText = null)
        {
            int? BlogPostsCount = await _BlogPostRepository.GetCount(SearchText);
            return Ok(BlogPostsCount);
        }
        //url : {apibaseurl}/api/blogposts/{id}
        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> GetBlogPostById([FromRoute] Guid id)
        {
            var response = await _BlogPostRepository.GetByIdAsync(id);
            if(response is null)
            {
                return NotFound();
            }
            return Ok(response);
        }
        //url : {apibaseurl}/api/blogposts/{urlHandle}
        [HttpGet]
        [Route("{urlHandle}")]
        public async Task<IActionResult> GetBlogPostByURLHandle([FromRoute] string urlHandle)
        {
            var response = await _BlogPostRepository.GetByURLHandleAsync(urlHandle);
            if (response is null)
            {
                return NotFound();
            }
            return Ok(response);
        }
        //PUT : {apibaseurl}/api/blogposts/{id}
        [HttpPut]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> EditBlogPost([FromRoute] Guid id, [FromBody] UpdateBlogPostsRequestDto UpdateBlogPostsRequestDto)
        {
            var response = await _BlogPostRepository.UpdateBlogPosts(id, UpdateBlogPostsRequestDto);
            if (response is null)
            {
                return NotFound();
            }
            return Ok(response);
        }
        [HttpDelete]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> DeleteBlogPost([FromRoute] Guid id)
        {
            var response = await _BlogPostRepository.DeleteBlogPost(id);
            if (response is null)
            {
                return NotFound();
            }
            
            return Ok(response);
        }
    }
}

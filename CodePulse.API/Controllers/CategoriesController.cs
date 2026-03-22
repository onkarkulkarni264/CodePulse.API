using CodePulse.API.Data;
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
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _CategoryRepository;
        public CategoriesController(ICategoryRepository CategoryRepository)
        {

            _CategoryRepository = CategoryRepository;
        }

        [HttpPost]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequestDTO Categpry)
        {
            Category category = new Category
            {
                Name = Categpry.Name,
                URLHandle = Categpry.URLHandle
            };
            await _CategoryRepository.CreateCateGory(category);
            var Response = new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                URLHandle = category.URLHandle
            };
            return Ok(Response);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllCreateCategories([FromQuery] string? SearchText = null, [FromQuery] string? SortBy = null, [FromQuery] string? SortDirection = null, [FromQuery] int? PageSize = null, [FromQuery] int? PageNumber = null)
        {
            var Categories = await _CategoryRepository.GetAllCategories(SearchText, SortBy, SortDirection, PageSize, PageNumber);
            var response = new List<CategoryDTO>();
            foreach (var category in Categories)
            {
                var categoryDto = new CategoryDTO
                {
                    Id = category.Id,
                    Name = category.Name,
                    URLHandle = category.URLHandle
                };
                response.Add(categoryDto);
            }
            return Ok(response);
        }
        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> GetCategoryByID([FromRoute] Guid id)
        {
            var existingCategory = await _CategoryRepository.GetCategoryByID(id);
            if (existingCategory is null)
            {
                return NotFound();
            }
            var responce = new CategoryDTO
            {
                Id = existingCategory.Id,
                Name = existingCategory.Name,
                URLHandle = existingCategory.URLHandle
            };
            return Ok(responce);

        }
        [HttpPut]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> EditCategory([FromRoute] Guid id, [FromBody] UpdateCategoryDTO request)
        {

            var Category = new Category
            {
                Id = id,
                Name = request.Name,
                URLHandle = request.URLHandle
            };
            Category= await _CategoryRepository.UpdateCategory(Category);
            if(Category is null)
            {
                return NotFound();
            }
            var response = new CategoryDTO
            {
                Id = Category.Id,
                Name = Category.Name,
                URLHandle = Category.URLHandle
            };
            return Ok(response);
        }
        [HttpDelete]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> DeleteCategory([FromRoute] Guid id)
        {
            var existingCategory = await _CategoryRepository.DeleteCategory(id);
            if (existingCategory is null)
            {
                return NotFound();
            }
            var response = new CategoryDTO
            {
                Id = existingCategory.Id,
                Name = existingCategory.Name,
                URLHandle = existingCategory.URLHandle
            };
            return Ok(response);
        }
        [HttpGet]
        [Route("count")]
        public async Task<IActionResult> GetCount()
        {
            int? CategoriesCount = await _CategoryRepository.GetCount();
            
            return Ok(CategoriesCount);
        }


    }
}

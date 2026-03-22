using Azure.Core;
using CodePulse.API.Data;
using CodePulse.API.Models.Domain;
using CodePulse.API.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CodePulse.API.Repositories.Implementation
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _Context;
        public CategoryRepository(ApplicationDbContext Context)
        {
            _Context = Context;
        }

        public async Task<Category> CreateCateGory(Category request)
        {
            await _Context.AddAsync(request);
            await _Context.SaveChangesAsync();
            return request;
        }

        public async Task<IEnumerable<Category>> GetAllCategories(string? SearchText = null, string? SortBy = null, string? SortDirection = null, int? PageSize = 5, int? PageNumber = 1)
        {
            var Categories =  _Context.Categories.AsQueryable();

            if (!string.IsNullOrEmpty(SearchText))
            {
                Categories = Categories.Where(x => (x.Name.Contains(SearchText) || x.URLHandle.Contains(SearchText)));
            }
            if(string.Equals(SortBy,"Name", StringComparison.OrdinalIgnoreCase))
            {
                Categories = string.Equals(SortDirection, "asc") ? Categories.OrderBy(x => x.Name) : Categories.OrderByDescending(x => x.Name);
            }
            if (string.Equals(SortBy, "URLHandle", StringComparison.OrdinalIgnoreCase))
            {
                Categories = string.Equals(SortDirection, "asc") ? Categories.OrderBy(x => x.URLHandle) : Categories.OrderByDescending(x => x.URLHandle);
            }

            Categories = Categories.Skip(((PageNumber - 1) * PageSize) ?? 0).Take(PageSize ?? 5);

            return Categories;
        }

        public async Task<Category?> GetCategoryByID(Guid id)
        {
            return await _Context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category?> UpdateCategory(Category request)
        {
            var existingcategory= await _Context.Categories.FirstOrDefaultAsync(c => c.Id == request.Id);
            if (existingcategory is not null)
            {
                _Context.Entry(existingcategory).CurrentValues.SetValues(request);
                _Context.SaveChanges();
                return request;
            }
            return null;
        }
        public async Task<Category?> DeleteCategory(Guid id)
        {
            var existingcategory = await _Context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (existingcategory is  null)
            {
                return null;
                
            }
            _Context.Categories.Remove(existingcategory);
            await _Context.SaveChangesAsync();
            return existingcategory;
        }
        public async Task<int?> GetCount()
        {
            var categoryCount = await _Context.Categories.CountAsync();
            return categoryCount;
        }
    }
}

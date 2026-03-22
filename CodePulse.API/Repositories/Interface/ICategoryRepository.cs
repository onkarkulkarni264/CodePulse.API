using CodePulse.API.Models.Domain;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;

namespace CodePulse.API.Repositories.Interface
{
    public interface ICategoryRepository
    {
         Task<Category> CreateCateGory(Category request);
         Task<IEnumerable<Category>> GetAllCategories(string? SearchText = null,string ? SortBy = null, string? SortDirection = null, int? PageSize = 5, int? PageNumber = 1);
         Task<Category?> GetCategoryByID(Guid id);
         Task<Category?> UpdateCategory(Category request);
         Task<Category?> DeleteCategory(Guid id);
         Task<int?> GetCount(string? SearchText = null);
    }
}

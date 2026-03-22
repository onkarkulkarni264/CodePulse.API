using Azure.Core;
using CodePulse.API.Data;
using CodePulse.API.Models.Domain;
using CodePulse.API.Models.DTO;
using CodePulse.API.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using CodePulse.API.Repositories.Implementation;

namespace CodePulse.API.Repositories.Implementation
{
    public class BlogPostRepository : IBlogPostRepository
    {
        private readonly ApplicationDbContext _Context;
        public BlogPostRepository(ApplicationDbContext Context)
        {
            _Context = Context;
        }
        public async Task<BlogPostsDTO> createAsync(CreateBlogPostsRequestDto CategoryBlogPostsDTO)
        {
            var blogPost = new BlogPost
            {
                Title = CategoryBlogPostsDTO.Title,
                Content = CategoryBlogPostsDTO.Content,
                FeaturedImageURL = CategoryBlogPostsDTO.FeaturedImageURL,
                Auther = CategoryBlogPostsDTO.Auther,
                IsVisible = CategoryBlogPostsDTO.IsVisible,
                URLHandle = CategoryBlogPostsDTO.URLHandle,
                ShortDescription = CategoryBlogPostsDTO.ShortDescription,
                PublishedDate = CategoryBlogPostsDTO.PublishedDate,
                Categories = new List<Category>()
            };
            if (CategoryBlogPostsDTO.Categories != null && CategoryBlogPostsDTO.Categories.Any())
            {
                foreach (var category in CategoryBlogPostsDTO.Categories)
                {
                    var existingCategory = await _Context.Categories.FindAsync(category);
                    if (existingCategory != null)
                    {
                        blogPost.Categories.Add(existingCategory);
                    }
                }
            }
            await _Context.BlogPosts.AddAsync(blogPost);
            await _Context.SaveChangesAsync();
            var response = new BlogPostsDTO
            {
                Id = blogPost.Id,
                Title = blogPost.Title,
                Content = blogPost.Content,
                FeaturedImageURL = blogPost.FeaturedImageURL,
                Auther = blogPost.Auther,
                IsVisible = blogPost.IsVisible,
                URLHandle = blogPost.URLHandle,
                ShortDescription = blogPost.ShortDescription,
                PublishedDate = blogPost.PublishedDate,
                Categories = blogPost.Categories.Select(X => new CategoryDTO
                {
                    Id = X.Id,
                    Name = X.Name,
                    URLHandle = X.URLHandle
                }).ToList()
            };
            return response;
        }

        public async Task<IEnumerable<BlogPostsDTO>> getAllAsync(string? SearchText = null, string? SortBy = null, string? SortDirection = null, int? PageSize = 5, int? PageNumber = 1)
        {
            var blogPosts = _Context.BlogPosts.Include(X => X.Categories).AsQueryable();

            if (!string.IsNullOrEmpty(SearchText))
            {
                blogPosts = blogPosts.Where(x => x.Title.Contains(SearchText) || x.ShortDescription.Contains(SearchText));
            }
            if (string.Equals(SortBy, "Title", StringComparison.OrdinalIgnoreCase))
            {
                blogPosts = string.Equals(SortDirection, "asc") ? blogPosts.OrderBy(x => x.Title) : blogPosts.OrderByDescending(x => x.Title);
            }
            if (string.Equals(SortBy, "ShortDescription", StringComparison.OrdinalIgnoreCase))
            {
                blogPosts = string.Equals(SortDirection, "asc") ? blogPosts.OrderBy(x => x.ShortDescription) : blogPosts.OrderByDescending(x => x.ShortDescription);
            }

            blogPosts = blogPosts.Skip(((PageNumber - 1) * PageSize) ?? 0).Take(PageSize ?? 5);

            var result = await blogPosts.ToListAsync();
            var response = new List<BlogPostsDTO>();
            foreach (var blogPost in result)
            {
                response.Add(new BlogPostsDTO
                {
                    Id = blogPost.Id,
                    Title = blogPost.Title,
                    Content = blogPost.Content,
                    FeaturedImageURL = blogPost.FeaturedImageURL,
                    Auther = blogPost.Auther,
                    IsVisible = blogPost.IsVisible,
                    URLHandle = blogPost.URLHandle,
                    ShortDescription = blogPost.ShortDescription,
                    PublishedDate = blogPost.PublishedDate,
                    Categories = blogPost.Categories.Select(X => new CategoryDTO
                    {
                        Id = X.Id,
                        Name = X.Name,
                        URLHandle = X.URLHandle
                    }).ToList()
                });
            }
            return response;
        }

        public async Task<int?> GetCount(string? SearchText = null)
        {
            var blogPosts = _Context.BlogPosts.AsQueryable();
            if (!string.IsNullOrEmpty(SearchText))
            {
                blogPosts = blogPosts.Where(x => x.Title.Contains(SearchText) || x.ShortDescription.Contains(SearchText));
            }
            var blogPostCount = await blogPosts.CountAsync();
            return blogPostCount;
        }

        public async Task<BlogPostsDTO?> GetByIdAsync(Guid id)
        {
            var blogPostById = await _Context.BlogPosts.Include(X => X.Categories).FirstOrDefaultAsync(c => c.Id == id);
            BlogPostsDTO? response = null;
            if (blogPostById is not null)
            {
                response = new BlogPostsDTO
                {
                    Id = blogPostById.Id,
                    Title = blogPostById.Title,
                    Content = blogPostById.Content,
                    FeaturedImageURL = blogPostById.FeaturedImageURL,
                    Auther = blogPostById.Auther,
                    IsVisible = blogPostById.IsVisible,
                    URLHandle = blogPostById.URLHandle,
                    ShortDescription = blogPostById.ShortDescription,
                    PublishedDate = blogPostById.PublishedDate,
                    Categories = blogPostById.Categories.Select(X => new CategoryDTO
                    {
                        Id = X.Id,
                        Name = X.Name,
                        URLHandle = X.URLHandle
                    }).ToList()
                };
            }
            return response;
        }
        public async Task<BlogPostsDTO?> GetByURLHandleAsync(string urlHandle)
        {
            var blogPostById = await _Context.BlogPosts.Include(X => X.Categories).FirstOrDefaultAsync(c => c.URLHandle == urlHandle);
            BlogPostsDTO? response = null;
            if (blogPostById is not null)
            {
                response = new BlogPostsDTO
                {
                    Id = blogPostById.Id,
                    Title = blogPostById.Title,
                    Content = blogPostById.Content,
                    FeaturedImageURL = blogPostById.FeaturedImageURL,
                    Auther = blogPostById.Auther,
                    IsVisible = blogPostById.IsVisible,
                    URLHandle = blogPostById.URLHandle,
                    ShortDescription = blogPostById.ShortDescription,
                    PublishedDate = blogPostById.PublishedDate,
                    Categories = blogPostById.Categories.Select(X => new CategoryDTO
                    {
                        Id = X.Id,
                        Name = X.Name,
                        URLHandle = X.URLHandle
                    }).ToList()
                };
            }
            return response;
        }
        public async Task<BlogPostsDTO?> UpdateBlogPosts(Guid id, UpdateBlogPostsRequestDto request)
        {
            var blogPost = new BlogPost
            {
                Id = id,
                Title = request.Title,
                Content = request.Content,
                FeaturedImageURL = request.FeaturedImageURL,
                Auther = request.Auther,
                IsVisible = request.IsVisible,
                URLHandle = request.URLHandle,
                ShortDescription = request.ShortDescription,
                PublishedDate = request.PublishedDate,
                Categories = new List<Category>()
            };
            foreach(Guid Id in request.Categories)
            {
                var ExistingCategory = await _Context.Categories.FindAsync(Id);
                if (ExistingCategory is not null)
                {
                    blogPost.Categories.Add(ExistingCategory);
                }
            }

            var  ExistingblogPosts = await _Context.BlogPosts.Include(X => X.Categories).FirstOrDefaultAsync(c => c.Id == blogPost.Id);
            if ( ExistingblogPosts is not null)
            {
                // Update blog Post
                _Context.Entry(ExistingblogPosts).CurrentValues.SetValues(blogPost);
                //Update Categories
                ExistingblogPosts.Categories = blogPost.Categories;

                await _Context.SaveChangesAsync();

                var response = new BlogPostsDTO
                {
                    Title = blogPost.Title,
                    Content = blogPost.Content,
                    FeaturedImageURL = blogPost.FeaturedImageURL,
                    Auther = blogPost.Auther,
                    IsVisible = blogPost.IsVisible,
                    URLHandle = blogPost.URLHandle,
                    ShortDescription = blogPost.ShortDescription,
                    PublishedDate = blogPost.PublishedDate,
                    Categories = blogPost.Categories.Select(X => new CategoryDTO
                    {
                        Id = X.Id,
                        Name = X.Name,
                        URLHandle = X.URLHandle
                    }).ToList()
                };
                return response;
            }
            return null;
        }
        public async Task<List<BlogPostsDTO?>> DeleteBlogPost(Guid id)
        {
            var existingBlogPost = await _Context.BlogPosts.FirstOrDefaultAsync(c => c.Id == id);
            if (existingBlogPost is null)
            {
                return null;

            }
            _Context.BlogPosts.Remove(existingBlogPost);
            await _Context.SaveChangesAsync();

            var blogPosts = await _Context.BlogPosts.Include(X => X.Categories).ToListAsync();
            var response = new List<BlogPostsDTO?>();
            foreach (var blogPost in blogPosts)
            {
                response.Add(new BlogPostsDTO
                {
                    Id = blogPost.Id,
                    Title = blogPost.Title,
                    Content = blogPost.Content,
                    FeaturedImageURL = blogPost.FeaturedImageURL,
                    Auther = blogPost.Auther,
                    IsVisible = blogPost.IsVisible,
                    URLHandle = blogPost.URLHandle,
                    ShortDescription = blogPost.ShortDescription,
                    PublishedDate = blogPost.PublishedDate,
                    Categories = blogPost.Categories.Select(X => new CategoryDTO
                    {
                        Id = X.Id,
                        Name = X.Name,
                        URLHandle = X.URLHandle
                    }).ToList()
                });
            }
            
            return response;
        }
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CodePulse.API.Data
{
    public class AuthDbContext : IdentityDbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
            
        }
        override protected void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            var ReaderRoleId = "91053c07-07ee-441f-88d4-17545041c25d";
            var WriterRoleId = "0b168ae8-3246-40c4-ba5f-eda8544886b6";

            // Seed Roles
            var Roles = new List<IdentityRole>
            {
                new IdentityRole
                {
                    Id = ReaderRoleId.ToString(),
                    Name = "Reader",
                    NormalizedName = "Reader".ToUpper(),
                    ConcurrencyStamp =ReaderRoleId.ToString()
                },
                new IdentityRole
                {
                    Id = WriterRoleId.ToString(),
                    Name = "Writer",
                    NormalizedName = "Writer".ToUpper(),
                    ConcurrencyStamp =WriterRoleId.ToString()
                }
            };
            // Seed the roles into the database
            builder.Entity<IdentityRole>().HasData(Roles);

            // Seed Admin User
            var adminUserId = "7f577b8b-dac2-45f7-b58a-a882ccbd14f2";
            var admin = new IdentityUser
            {
                Id = adminUserId,
                UserName = "admin@codepulse.com",
                NormalizedUserName = "admin".ToUpper(),
                Email = "admin@codepulse.com",
                NormalizedEmail = "admin@codepulse.com".ToUpper(),

            };
            // Hash the password for the admin user
            admin.PasswordHash = new PasswordHasher<IdentityUser<string>>().HashPassword(admin, "Admin@123");
            // Seed the admin user into the database
            builder.Entity<IdentityUser>().HasData(admin);
            // Assign both Reader and Writer roles to the admin user
            var adminRoles = new List<IdentityUserRole<string>>
            {
                new IdentityUserRole<string>
                {
                    UserId = adminUserId,
                    RoleId = ReaderRoleId.ToString()
                    
                },
                new IdentityUserRole<string>
                {
                    UserId = adminUserId,
                    RoleId = WriterRoleId.ToString()
                }
            };
            // Seed the admin roles into the database
            builder.Entity<IdentityUserRole<string>>().HasData(adminRoles);

        } 
    }
}

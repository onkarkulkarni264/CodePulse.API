using CodePulse.API.Data;
using CodePulse.API.Repositories.Implementation;
using CodePulse.API.Repositories.Interface;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ---- Disable config file-watching (fixes inotify limit crash on Render free tier) ----
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---- CORS Registration ----
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ---- Database Configuration ----
// Npgsql 7.x does not support 'channel_binding' parameter — strip it if present
static string SanitizeNpgsqlConnectionString(string url) =>
    url.Replace("&channel_binding=require", "").Replace("?channel_binding=require&", "?").Replace("?channel_binding=require", "");

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    var sanitizedUrl = SanitizeNpgsqlConnectionString(databaseUrl);
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(sanitizedUrl));
    builder.Services.AddDbContext<AuthDbContext>(options => options.UseNpgsql(sanitizedUrl));
}
else
{
    var neonConn = "Host=ep-still-bar-a14yboal-pooler.ap-southeast-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_cdPp2USnm6KX;SSL Mode=Require;Trust Server Certificate=true;";
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(neonConn));
    builder.Services.AddDbContext<AuthDbContext>(options => options.UseNpgsql(neonConn));
}

// ---- Cloudinary Configuration ----
var cloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME");
var cloudApiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY");
var cloudApiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET");
if (!string.IsNullOrEmpty(cloudName) && !string.IsNullOrEmpty(cloudApiKey) && !string.IsNullOrEmpty(cloudApiSecret))
{
    var account = new Account(cloudName, cloudApiKey, cloudApiSecret);
    var cloudinary = new Cloudinary(account);
    builder.Services.AddSingleton(cloudinary);
}

builder.Services.AddScoped<ITokenRepository, TokenRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IBlogPostRepository, BlogPostRepository>();
builder.Services.AddScoped<IImageRepository, ImageRepository>();

builder.Services.AddIdentityCore<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddTokenProvider<DataProtectorTokenProvider<IdentityUser>>("CodePulse")
                .AddEntityFrameworkStores<AuthDbContext>()
                .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(Options =>
{
    Options.Password.RequireDigit = false;
    Options.Password.RequireLowercase = false;
    Options.Password.RequireNonAlphanumeric = false;
    Options.Password.RequireUppercase = false;
    Options.Password.RequiredLength = 6;
    Options.Password.RequiredUniqueChars = 1;
});

// ---- JWT Configuration ----
// Read from env vars first, fall back to appsettings
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? builder.Configuration["Jwt:Key"];
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? builder.Configuration["Jwt:Issuer"];
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(Options =>
    {
        Options.TokenValidationParameters = new TokenValidationParameters
        {
            AuthenticationType ="Jwt",
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
        };
    });

// ---- Port Configuration (Render provides PORT env var) ----
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

app.MapGet("/api/health", () => Results.Ok("alive"));


// Configure the HTTP request pipeline.
// Enable Swagger in all environments for API documentation
app.UseSwagger();
app.UseSwaggerUI();

// ---- CORS ----
app.UseCors("AllowAll");

// Serve static files for local image storage (dev mode)
var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "Images");
if (Directory.Exists(imagesPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(imagesPath),
        RequestPath = "/Images"
    });
}

// Only redirect to HTTPS in development (Render handles SSL termination)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

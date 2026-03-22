using CodePulse.API.Models.DTO;
using CodePulse.API.Repositories.Implementation;
using CodePulse.API.Repositories.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CodePulse.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITokenRepository _tokenRepository;

        public AuthController(UserManager<IdentityUser> userManager, ITokenRepository  tokenRepository)
        {
            _userManager = userManager;
            _tokenRepository = tokenRepository;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
        {
            // Create a new IdentityUser with the provided email and password
            var user = new IdentityUser
            {
                UserName = request.Email,
                Email = request.Email
            };
            // Attempt to create the user in the database
            var IdentityResult = _userManager.CreateAsync(user, request.Password).Result;
            // If the user creation was successful, add the user to the "Reader" role
            if (IdentityResult.Succeeded)
            {
                IdentityResult = await _userManager.AddToRoleAsync(user, "Reader");
                if (IdentityResult.Succeeded)
                {
                    return Ok();
                }
                else
                {
                    if (IdentityResult.Errors.Any())
                    {
                        foreach (var errors in IdentityResult.Errors)
                        {
                            ModelState.AddModelError(errors.Code, errors.Description);
                        }
                    }
                }
            }
            else
            {
                if (IdentityResult.Errors.Any())
                {
                    foreach (var errors in IdentityResult.Errors)
                    {
                        ModelState.AddModelError(errors.Code, errors.Description);
                    }
                }
            }
            return ValidationProblem(ModelState);
        }
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            var identityUser = await _userManager.FindByEmailAsync(request.Email);
            if (identityUser is not null)
            {
                var CheckpasswordResult = await _userManager.CheckPasswordAsync(identityUser, request.Password);
                if (CheckpasswordResult)
                {
                    var Roles = await _userManager.GetRolesAsync(identityUser);
                    var token = _tokenRepository.CreateToken(identityUser, Roles.ToList());
                    var response = new LoginResponseDTO
                    {
                        Token = token,
                        Email = request.Email,
                        Roles = Roles.ToList()
                    };
                    return Ok(response);
                }
            }
            
            ModelState.AddModelError("InvalidCredentials", "The Email or Password is incorrect.");

            return ValidationProblem(ModelState);
        }
    }
}

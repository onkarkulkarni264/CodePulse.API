using CodePulse.API.Models;
using CodePulse.API.Models.DTO;
using CodePulse.API.Repositories.Interface;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CodePulse.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITokenRepository _tokenRepository;
        private readonly GoogleAuthSettings _googleAuthSettings;

        public AuthController(
            UserManager<IdentityUser> userManager,
            ITokenRepository tokenRepository,
            GoogleAuthSettings googleAuthSettings)
        {
            _userManager = userManager;
            _tokenRepository = tokenRepository;
            _googleAuthSettings = googleAuthSettings;
        }

        [HttpGet]
        [Route("google-client-id")]
        public IActionResult GetGoogleClientId()
        {
            if (string.IsNullOrWhiteSpace(_googleAuthSettings.ClientId))
            {
                return NotFound(new { message = "Google sign-in is not configured." });
            }

            return Ok(new { clientId = _googleAuthSettings.ClientId });
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
        {
            var user = new IdentityUser
            {
                UserName = request.Email,
                Email = request.Email
            };

            var identityResult = await _userManager.CreateAsync(user, request.Password);
            if (identityResult.Succeeded)
            {
                identityResult = await _userManager.AddToRoleAsync(user, "Reader");
                if (identityResult.Succeeded)
                {
                    return Ok();
                }

                foreach (var error in identityResult.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
            }
            else
            {
                foreach (var error in identityResult.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
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
                var checkPasswordResult = await _userManager.CheckPasswordAsync(identityUser, request.Password);
                if (checkPasswordResult)
                {
                    var roles = await _userManager.GetRolesAsync(identityUser);
                    var token = _tokenRepository.CreateToken(identityUser, roles.ToList());
                    var response = new LoginResponseDTO
                    {
                        Token = token,
                        Email = request.Email,
                        Roles = roles.ToList()
                    };
                    return Ok(response);
                }
            }

            ModelState.AddModelError("InvalidCredentials", "The Email or Password is incorrect.");
            return ValidationProblem(ModelState);
        }

        [HttpPost]
        [Route("google")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleAuthRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(_googleAuthSettings.ClientId))
            {
                ModelState.AddModelError("GoogleNotConfigured", "Google sign-in is not configured.");
                return ValidationProblem(ModelState);
            }

            if (string.IsNullOrWhiteSpace(request.IdToken))
            {
                ModelState.AddModelError("InvalidToken", "Google token is required.");
                return ValidationProblem(ModelState);
            }

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(
                    request.IdToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { _googleAuthSettings.ClientId }
                    });
            }
            catch (InvalidJwtException)
            {
                ModelState.AddModelError("InvalidToken", "Invalid Google token.");
                return ValidationProblem(ModelState);
            }

            if (string.IsNullOrWhiteSpace(payload.Email))
            {
                ModelState.AddModelError("InvalidToken", "Google account email is unavailable.");
                return ValidationProblem(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(payload.Email);
            if (user is null)
            {
                user = new IdentityUser
                {
                    UserName = payload.Email,
                    Email = payload.Email,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    foreach (var error in createResult.Errors)
                    {
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                    return ValidationProblem(ModelState);
                }

                await _userManager.AddLoginAsync(user, new UserLoginInfo("Google", payload.Subject, "Google"));
                await _userManager.AddToRoleAsync(user, "Reader");
            }
            else
            {
                var logins = await _userManager.GetLoginsAsync(user);
                if (!logins.Any(login => login.LoginProvider == "Google" && login.ProviderKey == payload.Subject))
                {
                    await _userManager.AddLoginAsync(user, new UserLoginInfo("Google", payload.Subject, "Google"));
                }

                if (!await _userManager.IsInRoleAsync(user, "Reader"))
                {
                    await _userManager.AddToRoleAsync(user, "Reader");
                }
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenRepository.CreateToken(user, roles.ToList());
            return Ok(new LoginResponseDTO
            {
                Token = token,
                Email = payload.Email,
                Roles = roles.ToList()
            });
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using SkyGuard.Core.DTOs;
using SkyGuard.Core.Enums;
using SkyGuard.Core.Models;
using SkyGuard.Core.Services;
using SkyGuard.Infrastructure.Respositories;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SkyGuard.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _config;
        private readonly IUserRepository _userRepository;
        private readonly IAzureAdTokenService _azureAdTokenService;
        private readonly ILogger<AuthController> _logger;


        public AuthController(IAuthService authService, IConfiguration config, IUserRepository userRepository, IAzureAdTokenService azureAdTokenService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _config = config;
            _userRepository = userRepository;
            _azureAdTokenService = azureAdTokenService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<LoginResponse>> Register(RegisterDto registerDto)
        {
            if (await _authService.UserExists(registerDto.Email))
                return BadRequest("User already exists");

            var roleString = registerDto.Role;

            if (!Enum.TryParse<UserRole>(roleString, ignoreCase: true, out var parsedRole))
            {
                return BadRequest("Invalid role specified");
            }

            var user = await _authService.Register(
                registerDto.Email,
                registerDto.Name,
                registerDto.Password,
                parsedRole);

            return new LoginResponse
            {
                user = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    Role = user.Role.ToString(),
                },
                token = _authService.CreateToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginDto loginDto)
        {
            var user = await _authService.Login(loginDto.Email, loginDto.Password);

            if (user == null) return Unauthorized("Invalid credentials");

            return new LoginResponse
            {
                user = new UserDto
                {
                    Id =  user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    Role = user.Role.ToString(),
                },            
                token = _authService.CreateToken(user)
            };
        }

        [HttpPost("azure-ad")]
        public async Task<ActionResult<LoginResponse>> LoginWithAzureAD([FromBody] AzureAdLoginDto azureAdLoginDto)
        {
            // Validate token and get claims principal
            var principal = await _azureAdTokenService.ValidateTokenAsync(azureAdLoginDto.Token);
            if (principal == null) return Unauthorized("Invalid Azure AD token");

            var email = principal.FindFirstValue(ClaimTypes.Email) ??
                       principal.FindFirstValue("preferred_username");

            if (string.IsNullOrEmpty(email))
                return BadRequest("No valid email claim found");

            // Check if user exists
            var user = await _userRepository.GetByEmailAsync(email);

            // create user if doesn't exist
            if (user == null)
            {
                using var hmac = new HMACSHA512();

                user = new User
                {
                    Email = email,
                    Name = principal.Claims.FirstOrDefault(c =>
                             c.Type == "name")?.Value!,
                    Role = DetermineDefaultRole(principal),
                    IsAzureAdUser = true,
                    CreatedAt = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow,
                    PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Pa$$w0rd")),
                    PasswordSalt = hmac.Key,
                    RefreshToken = string.Empty,
                    RefreshTokenExpires = DateTime.UtcNow.AddDays(7),
                };

                await _userRepository.AddAsync(user);
            }
            else
            {
                user.LastLogin = DateTime.UtcNow;
                user.RefreshToken = GenerateRefreshToken();
                user.RefreshTokenExpires = DateTime.UtcNow.AddDays(7); 
                await _userRepository.UpdateAsync(user);
            }

            // Generate application JWT
            var token = _authService.CreateToken(user);

            return Ok(new LoginResponse
            {
                user = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    Role = user.Role.ToString(),
                },
                token = token
            });
        }

        private UserRole DetermineDefaultRole(ClaimsPrincipal principal)
        {
            // Check Azure AD groups for role assignment
            var groups = principal.FindAll("groups").Select(c => c.Value);
            if (groups.Contains("Security-Team-Group-ObjectId"))
                return UserRole.SecurityTeam;

            // Set default role
            return UserRole.UASCoordinator;
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            var user = await _authService.GetUserByRefreshToken(refreshTokenDto.RefreshToken);

            if (user == null || user.RefreshToken != refreshTokenDto.RefreshToken ||
                user.RefreshTokenExpires <= DateTime.UtcNow)
                return Unauthorized("Invalid refresh token");

            return Ok(new LoginResponse
            {
                user = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    Role = user.Role.ToString(),
                },
                token = _authService.CreateToken(user)
            });
        
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            try
            {
                // Get the user ID from the claims
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Invalid user claims");
                }

                // Get user from repository
                var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Return user DTO
                return Ok(new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    Role = user.Role.ToString(),
                    // Add any other properties you want to return
                });
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "Error getting current user");
                return StatusCode(500, "An error occurred while retrieving user information");
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            await _authService.RevokeRefreshToken(Guid.Parse(userId));
            return NoContent();
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using XRPAtom.Core.Domain;
using XRPAtom.Core.DTOs;
using XRPAtom.Core.Interfaces;

namespace XRPAtom.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IPasswordService _passwordService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IUserService userService,
            IPasswordService passwordService,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _userService = userService;
            _passwordService = passwordService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto registrationDto)
        {
            try
            {
                // Check if email is already in use
                if (!await _userService.IsEmailUniqueAsync(registrationDto.Email))
                {
                    return BadRequest(new { error = "Email is already registered" });
                }

                // Confirm password match
                if (registrationDto.Password != registrationDto.ConfirmPassword)
                {
                    return BadRequest(new { error = "Passwords do not match" });
                }

                // Create the user
                var createUserDto = new CreateUserDto
                {
                    Name = registrationDto.Name,
                    Email = registrationDto.Email,
                    PhoneNumber = registrationDto.PhoneNumber,
                    Role = registrationDto.Role,
                    Organization = registrationDto.Organization,
                    Password = registrationDto.Password
                };

                var createdUser = await _userService.CreateUserAsync(createUserDto);

                // Generate token
                var token = GenerateJwtToken(createdUser);

                // Return user info and token
                return Ok(new AuthResponseDto
                {
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:DurationInMinutes"] ?? "60")),
                    User = createdUser
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, new { error = "An error occurred during registration" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
        {
            try
            {
                // Find user by email
                var user = await _userService.GetUserByEmailAsync(loginDto.Email);
                if (user == null)
                {
                    return Unauthorized(new { error = "Invalid email or password" });
                }

                // Validate password - this is a simplified example
                // In a real implementation, you would use Identity or a proper password hasher
                // This assumes your IPasswordService has a method to validate passwords
                var isPasswordValid = await _passwordService.ValidatePasswordAsync(new User { Id = user.Id }, loginDto.Password);
                if (!isPasswordValid)
                {
                    return Unauthorized(new { error = "Invalid email or password" });
                }

                // Update last login time
                await _userService.UpdateLastLoginAsync(user.Id);

                // Generate token
                var token = GenerateJwtToken(user);

                // Return user info and token
                return Ok(new AuthResponseDto
                {
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:DurationInMinutes"] ?? "60")),
                    User = user
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login");
                return StatusCode(500, new { error = "An error occurred during login" });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "Invalid authentication token" });
                }

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { error = "User not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user");
                return StatusCode(500, new { error = "An error occurred while retrieving user information" });
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "Invalid authentication token" });
                }

                // Confirm password match
                if (changePasswordDto.NewPassword != changePasswordDto.ConfirmNewPassword)
                {
                    return BadRequest(new { error = "New passwords do not match" });
                }

                // Change password
                var result = await _passwordService.ChangePasswordAsync(userId, changePasswordDto);
                if (!result)
                {
                    return BadRequest(new { error = "Failed to change password" });
                }

                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, new { error = "An error occurred while changing password" });
            }
        }

        private string GenerateJwtToken(UserDto user)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Key is not configured");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim("userId", user.Id),                        // Custom claim for user ID
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),  // Subject is email
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:DurationInMinutes"] ?? "60")),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
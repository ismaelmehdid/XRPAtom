using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XRPAtom.Core.Interfaces;
using XRPAtom.Core.DTOs;
using XRPAtom.Core.Domain;

namespace XRPAtom.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserService userService, 
            ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Get the current user's profile
        /// </summary>
        /// <returns>User profile details</returns>
        [HttpGet("profile")]
        public async Task<IActionResult> GetUserProfile()
        {
            try 
            {
                // Extract user ID from the JWT token
                var userId = User.FindFirst("userId")?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "Invalid user identifier" });
                }

                var userProfile = await _userService.GetUserProfileAsync(userId);
                
                if (userProfile == null)
                {
                    return NotFound(new { error = "User profile not found" });
                }

                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile");
                return StatusCode(500, new { error = "An unexpected error occurred while retrieving the user profile" });
            }
        }

        /// <summary>
        /// Update the current user's profile
        /// </summary>
        /// <param name="updateDto">User profile update information</param>
        /// <returns>Updated user profile</returns>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserDto updateDto)
        {
            try 
            {
                // Extract user ID from the JWT token
                var userId = User.FindFirst("userId")?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "Invalid user identifier" });
                }

                var updatedUser = await _userService.UpdateUserAsync(userId, updateDto);
                
                if (updatedUser == null)
                {
                    return NotFound(new { error = "User not found" });
                }

                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                return StatusCode(500, new { error = "An unexpected error occurred while updating the user profile" });
            }
        }

        /// <summary>
        /// Get user statistics and summary information
        /// </summary>
        /// <returns>User statistics</returns>
        [HttpGet("stats")]
        public async Task<IActionResult> GetUserStats()
        {
            try 
            {
                // Extract user ID from the JWT token
                var userId = User.FindFirst("userId")?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "Invalid user identifier" });
                }

                // In a real implementation, this would fetch various user statistics
                var userStats = new 
                {
                    TotalDevices = await _userService.GetUserDeviceCountAsync(userId),
                    ActiveEvents = await _userService.GetActiveEventCountAsync(userId),
                    TotalRewards = await _userService.GetTotalRewardsAsync(userId),
                    MemberSince = await _userService.GetRegistrationDateAsync(userId)
                };

                return Ok(userStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user statistics");
                return StatusCode(500, new { error = "An unexpected error occurred while retrieving user statistics" });
            }
        }

        /// <summary>
        /// Deactivate the current user's account
        /// </summary>
        /// <returns>Success or error message</returns>
        [HttpPost("deactivate")]
        public async Task<IActionResult> DeactivateAccount()
        {
            try 
            {
                // Extract user ID from the JWT token
                var userId = User.FindFirst("userId")?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "Invalid user identifier" });
                }

                var result = await _userService.DeactivateUserAsync(userId);
                
                if (!result)
                {
                    return BadRequest(new { error = "Failed to deactivate account" });
                }

                return Ok(new { message = "Account deactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user account");
                return StatusCode(500, new { error = "An unexpected error occurred while deactivating the account" });
            }
        }

        /// <summary>
        /// Admin-only endpoint to change a user's role
        /// </summary>
        /// <param name="userId">User ID to modify</param>
        /// <param name="newRole">New role to assign</param>
        /// <returns>Updated user information</returns>
        [HttpPut("{userId}/role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeUserRole(
            string userId, 
            [FromBody] UserRole newRole)
        {
            try 
            {
                var result = await _userService.ChangeUserRoleAsync(userId, newRole);
                
                if (!result)
                {
                    return BadRequest(new { error = "Failed to change user role" });
                }

                return Ok(new { message = "User role updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error changing role for user {userId}");
                return StatusCode(500, new { error = "An unexpected error occurred while changing user role" });
            }
        }
    }
}
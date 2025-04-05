using XRPAtom.Core.Domain;
using XRPAtom.Core.DTOs;

namespace XRPAtom.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for user-related operations in the XRPAtom system
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Retrieves a user by their unique identifier
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <returns>User details or null if not found</returns>
        Task<UserDto> GetUserByIdAsync(string userId);

        /// <summary>
        /// Retrieves a user by their email address
        /// </summary>
        /// <param name="email">The email address of the user</param>
        /// <returns>User details or null if not found</returns>
        Task<UserDto> GetUserByEmailAsync(string email);

        /// <summary>
        /// Creates a new user in the system
        /// </summary>
        /// <param name="createUserDto">Data transfer object containing user creation details</param>
        /// <returns>The created user's details</returns>
        /// <exception cref="InvalidOperationException">Thrown if user creation fails</exception>
        Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);

        /// <summary>
        /// Updates an existing user's profile information
        /// </summary>
        /// <param name="userId">The unique identifier of the user to update</param>
        /// <param name="updateUserDto">Data transfer object containing updated user information</param>
        /// <returns>The updated user's details</returns>
        /// <exception cref="InvalidOperationException">Thrown if user update fails</exception>
        Task<UserDto> UpdateUserAsync(string userId, UpdateUserDto updateUserDto);

        /// <summary>
        /// Deletes a user from the system
        /// </summary>
        /// <param name="userId">The unique identifier of the user to delete</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        Task<bool> DeleteUserAsync(string userId);

        /// <summary>
        /// Checks if an email address is unique in the system
        /// </summary>
        /// <param name="email">The email address to check</param>
        /// <returns>True if the email is unique, false if it already exists</returns>
        Task<bool> IsEmailUniqueAsync(string email);

        /// <summary>
        /// Retrieves a user's profile information
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <returns>Detailed user profile information</returns>
        Task<UserDto> GetUserProfileAsync(string userId);

        /// <summary>
        /// Changes a user's role in the system
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <param name="newRole">The new role to assign to the user</param>
        /// <returns>True if role change was successful, false otherwise</returns>
        Task<bool> ChangeUserRoleAsync(string userId, UserRole newRole);

        /// <summary>
        /// Deactivates a user's account
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <returns>True if deactivation was successful, false otherwise</returns>
        Task<bool> DeactivateUserAsync(string userId);

        /// <summary>
        /// Reactivates a previously deactivated user account
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <returns>True if reactivation was successful, false otherwise</returns>
        Task<bool> ReactivateUserAsync(string userId);

        /// <summary>
        /// Updates the last login timestamp for a user
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <returns>True if update was successful, false otherwise</returns>
        Task<bool> UpdateLastLoginAsync(string userId);

        /// <summary>
        /// Retrieves the total number of users in the system
        /// </summary>
        /// <returns>Total number of users</returns>
        Task<int> GetTotalUserCountAsync();

        /// <summary>
        /// Retrieves users by their role
        /// </summary>
        /// <param name="role">The role to filter users by</param>
        /// <param name="page">The page number for pagination</param>
        /// <param name="pageSize">The number of users per page</param>
        /// <returns>A collection of users with the specified role</returns>
        Task<IEnumerable<UserDto>> GetUsersByRoleAsync(UserRole role, int page = 1, int pageSize = 10);

        /// <summary>
        /// Get the number of devices associated with a user
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <returns>Number of devices</returns>
        Task<int> GetUserDeviceCountAsync(string userId);

        /// <summary>
        /// Get the count of active events for a user
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <returns>Number of active events</returns>
        Task<int> GetActiveEventCountAsync(string userId);

        /// <summary>
        /// Get the total rewards earned by a user
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <returns>Total rewards amount</returns>
        Task<decimal> GetTotalRewardsAsync(string userId);

        /// <summary>
        /// Get the user's registration date
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <returns>User's registration date</returns>
        Task<DateTime> GetRegistrationDateAsync(string userId);
    }
}
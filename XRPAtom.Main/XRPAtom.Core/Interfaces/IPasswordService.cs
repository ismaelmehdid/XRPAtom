using System.Threading.Tasks;
using XRPAtom.Core.Domain;
using XRPAtom.Core.DTOs;
using XRPAtom.Core.Security;

namespace XRPAtom.Core.Interfaces
{
    /// <summary>
    /// Provides services for password-related operations
    /// </summary>
    public interface IPasswordService
    {
        /// <summary>
        /// Changes a user's password after verifying the current password
        /// </summary>
        /// <param name="userId">The unique identifier of the user</param>
        /// <param name="changePasswordDto">Data transfer object containing password change details</param>
        /// <returns>True if the password was successfully changed, otherwise throws an exception</returns>
        /// <exception cref="ArgumentNullException">Thrown when password change details are null</exception>
        /// <exception cref="InvalidOperationException">Thrown when user is not found or password change fails</exception>
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);

        /// <summary>
        /// Validates a password against configured password policies
        /// </summary>
        /// <param name="user">The user for whom the password is being validated</param>
        /// <param name="password">The password to validate</param>
        /// <returns>Password validation result indicating validation status</returns>
        Task<bool> ValidatePasswordAsync(User user, string password);
    }
}
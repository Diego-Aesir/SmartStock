using Microsoft.AspNetCore.Identity;
using SmartStock.Models;

namespace SmartStock.Services.DatabaseServices
{
    public class UserDbService(UserManager<User> userManager)
    {
        private readonly UserManager<User> _userManager = userManager;

        public async Task<User?> GetUserById(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user ?? throw new Exception("User could not be found");
        }

        public async Task<User?> LoginUser(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email) ?? throw new ArgumentException("User could not be found.");

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!isPasswordValid)
            {
                throw new ArgumentException("Password does not match.");
            }

            return user;
        }

        public async Task<User> CreateUser(User user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"User could not be created. Errors: {errorMessage}");
            }
            await _userManager.AddToRoleAsync(user, "Client");

            return user;
        }

        public async Task<User> UpdateUser(User updatedUser, string? newPassword)
        {
            var oldUser = await GetUserById(updatedUser.Id);
            if (oldUser != null)
            {
                oldUser.UserName = updatedUser.UserName ?? oldUser.UserName;
                oldUser.Email = updatedUser.Email ?? oldUser.Email;
                oldUser.PhoneNumber = updatedUser.PhoneNumber ?? oldUser.PhoneNumber;
                oldUser.FullName = updatedUser.FullName ?? oldUser.FullName;

                if (!string.IsNullOrEmpty(newPassword))
                {
                    var result = await _userManager.ChangePasswordAsync(oldUser, oldUser.PasswordHash, newPassword);
                    if (!result.Succeeded)
                    {
                        throw new Exception("Password could not be updated");
                    }
                }

                var updateResult = await _userManager.UpdateAsync(oldUser);
                if (!updateResult.Succeeded)
                {
                    throw new Exception("User could not be updated");
                }

                return oldUser;
            }
            else
            {
                throw new Exception("User not found");
            }
        }

        public async Task<bool> DeleteUser(string userId)
        {
            var user = await GetUserById(userId) ?? throw new Exception("User not found");
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                throw new Exception("Could not Delete this user");
            }
            return result.Succeeded;
        }
    }
}

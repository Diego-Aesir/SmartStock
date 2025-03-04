using System.ComponentModel.DataAnnotations;

namespace SmartStock.DTO.User
{
    public class UserLoginDTO
    {
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public required string Password { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace SmartStock.DTO
{
    public class UserRegisterDTO
    {
        [Required(ErrorMessage = "Username is required")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
        public required string UserName { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        public required string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Your Email must be valid: wonderfulEmail@email.com")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        public required string Phone { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{6,}$", ErrorMessage = "Password must be at least 6 characters long, contain at least one uppercase letter, one lowercase letter, and one number.")]
        public required string Password { get; set; }
    }
}

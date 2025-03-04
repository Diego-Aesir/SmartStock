using System.ComponentModel.DataAnnotations;

namespace SmartStock.DTO.User
{
    public class UserUpdateDTO
    {
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
        public string? UserName { get; set; }

        public string? FullName { get; set; }

        [EmailAddress(ErrorMessage = "Your Email must be valid: wonderfulEmail@email.com")]
        public string? Email { get; set; }

        public string? Phone { get; set; }

        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{6,}$", ErrorMessage = "Password must be at least 6 characters long, contain at least one uppercase letter, one lowercase letter, and one number.")]
        public string? Password { get; set; }
    }
}

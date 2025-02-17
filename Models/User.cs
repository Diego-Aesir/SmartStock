using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace SmartStock.Models
{
    public class User : IdentityUser
    {
        [MinLength(3)]
        [PersonalData]
        public string? FullName { get; set; }

        public required int CartId { get; set; }
    }
}

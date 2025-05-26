using System.ComponentModel.DataAnnotations;

namespace LoginAPI.Models
{
    public class RegistrationDto
    {
        [Key]
        public string Id { get; set; }
        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; }
       
        [Required(ErrorMessage = "Password is required")]
        [StringLength(25, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 25 characters")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }
        public int IsActive { get; set; }
        public DateTime DateCreated { get; set; }
        public string? Role { get; set; } 

    }
}

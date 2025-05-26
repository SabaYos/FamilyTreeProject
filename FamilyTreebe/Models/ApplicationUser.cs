using Microsoft.AspNetCore.Identity;
namespace LoginAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<IdentityUserRole<string>> UserRoles { get; set; }
    }
}

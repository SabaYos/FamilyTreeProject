using Microsoft.AspNetCore.Identity;
namespace LoginAPI.Models
{
    public class ApplicationRole : IdentityRole
    {
        public virtual ICollection<IdentityUserRole<string>> UserRoles { get; set; }
    }
}

using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;
using LoginAPI.Models;

public class UserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    // Get current user info
    public async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return null;

        return await _userManager.FindByIdAsync(userId);
    }

    // Update username
    public async Task<bool> UpdateUsernameAsync(string newUsername)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return false;

        user.UserName = newUsername;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    // Change password
    public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return false;

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return result.Succeeded;
    }
}

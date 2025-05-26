using FamilyTreeAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace FamilyTreeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FamilyTreesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FamilyTreesController> _logger;

        public FamilyTreesController(ApplicationDbContext context, ILogger<FamilyTreesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/familytrees?page=1&pageSize=5
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FamilyTreeDto>>> GetFamilyTrees([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Unauthorized access attempt: User not authenticated.");
                    return Unauthorized("User not authenticated.");
                }

                var query = _context.FamilyTrees
                    .Where(t => t.OwnerId == userId)
                    .Select(t => new FamilyTreeDto
                    {
                        FamilyTreeId = t.FamilyTreeId,
                        FamilyTreeName = t.FamilyTreeName,
                        IsPublic = t.IsPublic,
                        OwnerId = t.OwnerId
                    });

                var totalItems = await query.CountAsync();
                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                Response.Headers.Add("X-Total-Count", totalItems.ToString());
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving family trees for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, "An error occurred while retrieving family trees. Please try again later.");
            }
        }

        // GET: api/familytrees/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<FamilyTreeDto>> GetFamilyTree(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Unauthorized access attempt: User not authenticated.");
                    return Unauthorized("User not authenticated.");
                }

                var tree = await _context.FamilyTrees.FirstOrDefaultAsync(t => t.FamilyTreeId == id);
                if (tree == null || tree.OwnerId != userId)
                {
                    _logger.LogWarning("Family tree with ID {Id} not found or not owned by user {UserId}", id, userId);
                    return NotFound("Family tree not found or you do not have access.");
                }

                return Ok(new FamilyTreeDto
                {
                    FamilyTreeId = tree.FamilyTreeId,
                    FamilyTreeName = tree.FamilyTreeName,
                    IsPublic = tree.IsPublic,
                    OwnerId = tree.OwnerId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving family tree with ID {Id} for user {UserId}", id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, "An error occurred while retrieving the family tree. Please try again later.");
            }
        }


        // GET: api/familytrees/user-roles
        [HttpGet("user-roles")]
        public async Task<ActionResult<IEnumerable<FamilyTreeUserRole>>> GetUserTreeRoles()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated.");
            }

            var userRoles = await _context.FamilyTreeUserRoles
                .Where(ut => ut.UserId == userId)
                .Join(_context.FamilyTrees,
                      ut => ut.FamilyTreeId,
                      ft => ft.FamilyTreeId,
                      (ut, ft) => new FamilyTreeUserRole
                      {
                          UserId = userId,
                          FamilyTreeId = ut.FamilyTreeId,
                          FamilyTreeName = ft.FamilyTreeName,
                          Role = ut.Role
                      })
                .ToListAsync();

            return Ok(userRoles);
        }

        // POST: api/familytrees/user-roles
        [HttpPost("user-roles")]
        public async Task<ActionResult<FamilyTreeUserRole>> CreateFamilyTreeUserRole(FamilyTreeUserRole userRole)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated.");
            }

            try
            {
                // Validate the FamilyTreeId exists
                var familyTree = await _context.FamilyTrees.FirstOrDefaultAsync(t => t.FamilyTreeId == userRole.FamilyTreeId);
                if (familyTree == null)
                {
                    _logger.LogWarning("Family tree with ID {FamilyTreeId} not found for user {UserId}", userRole.FamilyTreeId, userId);
                    return NotFound("Family tree not found.");
                }

                // Ensure the user has permission (e.g., must be the owner to assign roles)
                if (familyTree.OwnerId != userId)
                {
                    _logger.LogWarning("User {UserId} does not have permission to assign roles for family tree {FamilyTreeId}", userId, userRole.FamilyTreeId);
                    return Forbid("You do not have permission to assign roles for this family tree.");
                }

                // Validate the role
                if (string.IsNullOrEmpty(userRole.Role) || (userRole.Role != "Admin" && userRole.Role != "FamilyMember"))
                {
                    _logger.LogWarning("Invalid role {Role} provided by user {UserId}", userRole.Role, userId);
                    return BadRequest("Invalid role. Role must be 'Admin' or 'FamilyMember'.");
                }

                // Ensure the UserId in the payload matches an existing user (optional, depending on your setup)
                // For simplicity, we're assuming the UserId is valid

                // Create the FamilyTreeUserRole entry
                var newUserRole = new FamilyTreeUserRole
                {
                    FamilyTreeId = userRole.FamilyTreeId,
                    UserId = familyTree.OwnerId,
                    Role = userRole.Role,
                    FamilyTreeName = familyTree.FamilyTreeName // Populate for DTO consistency
                };

                _context.FamilyTreeUserRoles.Add(newUserRole);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {UserId} assigned role {Role} to user {TargetUserId} for family tree {FamilyTreeId}",
                    userId, userRole.Role, userRole.UserId, userRole.FamilyTreeId);

                return CreatedAtAction(nameof(GetUserTreeRoles), new { id = newUserRole.Id }, newUserRole);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating FamilyTreeUserRole for user");
                return StatusCode(500, "An error occurred while assigning the role. Please try again later.");
            }
        }

        // POST: api/familytrees
        [HttpPost]
        public async Task<ActionResult<FamilyTreeDto>> CreateFamilyTree(FamilyTreeDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Unauthorized access attempt: User not authenticated.");
                    return Unauthorized("User not authenticated.");
                }

                if (string.IsNullOrWhiteSpace(dto.FamilyTreeName))
                {
                    _logger.LogWarning("Invalid family tree name provided by user {UserId}", userId);
                    return BadRequest("Family tree name is required.");
                }

                var tree = new FamilyTree
                {
                    FamilyTreeName = dto.FamilyTreeName,
                    IsPublic = dto.IsPublic,
                    OwnerId = userId
                };

                _context.FamilyTrees.Add(tree);
                await _context.SaveChangesAsync();

                dto.FamilyTreeId = tree.FamilyTreeId;
                dto.OwnerId = userId;
                _logger.LogInformation("Family tree with ID {Id} created by user {UserId}", tree.FamilyTreeId, userId);
                return CreatedAtAction(nameof(GetFamilyTree), new { id = tree.FamilyTreeId }, dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating family tree for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, "An error occurred while creating the family tree. Please try again later.");
            }
        }

        // PUT: api/familytrees/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<FamilyTreeDto>> UpdateFamilyTree(int id, FamilyTreeDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Unauthorized access attempt: User not authenticated.");
                    return Unauthorized("User not authenticated.");
                }

                if (id != dto.FamilyTreeId)
                {
                    _logger.LogWarning("ID mismatch in update request: URL ID {UrlId} does not match DTO ID {DtoId}", id, dto.FamilyTreeId);
                    return BadRequest("ID in URL does not match ID in body.");
                }

                if (string.IsNullOrWhiteSpace(dto.FamilyTreeName))
                {
                    _logger.LogWarning("Invalid family tree name provided by user {UserId}", userId);
                    return BadRequest("Family tree name is required.");
                }

                var tree = await _context.FamilyTrees.FirstOrDefaultAsync(t => t.FamilyTreeId == id);
                if (tree == null || tree.OwnerId != userId)
                {
                    _logger.LogWarning("Family tree with ID {Id} not found or not owned by user {UserId}", id, userId);
                    return NotFound("Family tree not found or you do not have access.");
                }

                tree.FamilyTreeName = dto.FamilyTreeName;
                tree.IsPublic = dto.IsPublic;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Family tree with ID {Id} updated by user {UserId}", id, userId);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating family tree with ID {Id} for user {UserId}", id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, "An error occurred while updating the family tree. Please try again later.");
            }
        }

        // DELETE: api/familytrees/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFamilyTree(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Unauthorized access attempt: User not authenticated.");
                    return Unauthorized("User not authenticated.");
                }

                var tree = await _context.FamilyTrees.FirstOrDefaultAsync(t => t.FamilyTreeId == id);
                if (tree == null || tree.OwnerId != userId)
                {
                    _logger.LogWarning("Family tree with ID {Id} not found or not owned by user {UserId}", id, userId);
                    return NotFound("Family tree not found or you do not have access.");
                }

                // Delete related entities (cascading delete for members and relationships)
                var members = await _context.FamilyMembers.Where(m => m.FamilyTreeId == id).ToListAsync();
                if (members.Any())
                {
                    var memberIds = members.Select(m => m.FamilyMemberId).ToList();
                    var relationships = await _context.Relationships
                        .Where(r => memberIds.Contains(r.FromPersonId) || memberIds.Contains(r.ToPersonId))
                        .ToListAsync();
                    _context.Relationships.RemoveRange(relationships);
                    _context.FamilyMembers.RemoveRange(members);
                }

                _context.FamilyTrees.Remove(tree);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Family tree with ID {Id} deleted by user {UserId}", id, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting family tree with ID {Id} for user {UserId}", id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, "An error occurred while deleting the family tree. Please try again later.");
            }
        }
    }
}
using FamilyTreeAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels;
using System.Security.Claims;

namespace FamilyTreeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FamilyMemberController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FamilyMemberController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/FamilyMember?familyTreeId={familyTreeId}
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FamilyMemberDto>>> GetFamilyMembers([FromQuery] int familyTreeId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated.");

                // Verify the user has access to the tree
                if (!await CanAccessTree(familyTreeId, userId))
                    return NotFound("Family tree not found or you do not have access.");

                var members = await _context.FamilyMembers
                    .Where(m => m.FamilyTreeId == familyTreeId)
                    .Select(m => new FamilyMemberDto
                    {
                        FamilyMemberId = m.FamilyMemberId,
                        FirstName = m.FirstName,
                        LastName = m.LastName,
                        Gender = m.Gender,
                        DateOfBirth = m.DateOfBirth,
                        DateOfDeath = m.DateOfDeath,
                        FamilyTreeId = m.FamilyTreeId,
                        MotherId = m.MotherId,
                        FatherId = m.FatherId,
                        CreatedBy = m.CreatedBy

                    })
                    .ToListAsync();

                return Ok(members);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving family members: {ex.Message}");
            }
        }

        // GET: api/FamilyMember/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<FamilyMemberDto>> GetFamilyMember(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var person = await _context.FamilyMembers.FirstOrDefaultAsync(p => p.FamilyMemberId == id);
                if (person == null || !await CanAccessTree(person.FamilyTreeId, userId))
                    return NotFound();

                return Ok(new FamilyMemberDto
                {
                    FamilyMemberId = person.FamilyMemberId,
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    Gender = person.Gender,
                    DateOfBirth = person.DateOfBirth,
                    DateOfDeath = person.DateOfDeath,
                    FamilyTreeId = person.FamilyTreeId,
                    MotherId = person.MotherId,
                    FatherId = person.FatherId,
                    CreatedBy = person.CreatedBy
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving family member: {ex.Message}");
            }
        }

        // POST: api/FamilyMember
        [HttpPost]
        public async Task<ActionResult<FamilyMemberDto>> CreateFamilyMember(FamilyMemberDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
               // var tree = await _context.FamilyTrees.FirstOrDefaultAsync(t => t.FamilyTreeId == dto.FamilyTreeId && t.OwnerId == userId);
                var tree = await _context.FamilyTreeUserRoles.FirstOrDefaultAsync(t => t.FamilyTreeId == dto.FamilyTreeId && (t.Role == "Admin" || t.Role == "Family Member"));
                if (tree == null)
                    return Unauthorized("Invalid tree or access denied.");

                // Validate MotherId and FatherId
                if (dto.MotherId.HasValue && !await _context.FamilyMembers.AnyAsync(p => p.FamilyMemberId == dto.MotherId && p.FamilyTreeId == dto.FamilyTreeId))
                    return BadRequest("Invalid MotherId.");
                if (dto.FatherId.HasValue && !await _context.FamilyMembers.AnyAsync(p => p.FamilyMemberId == dto.FatherId && p.FamilyTreeId == dto.FamilyTreeId))
                    return BadRequest("Invalid FatherId.");

                var person = new FamilyMember
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Gender = dto.Gender,
                    DateOfBirth = dto.DateOfBirth,
                    DateOfDeath = dto.DateOfDeath,
                    FamilyTreeId = dto.FamilyTreeId,
                    MotherId = dto.MotherId,
                    FatherId = dto.FatherId,
                    CreatedBy = userId
                };

                _context.FamilyMembers.Add(person);
                await _context.SaveChangesAsync();

                dto.FamilyMemberId = person.FamilyMemberId;
                return CreatedAtAction(nameof(GetFamilyMember), new { id = person.FamilyMemberId }, dto);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating person: {ex.Message}");
            }
        }

        // PUT: api/FamilyMember/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<FamilyMemberDto>> UpdateFamilyMember(int id, FamilyMemberDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated.");

                var person = await _context.FamilyMembers.FirstOrDefaultAsync(p => p.FamilyMemberId == id);
                if (person == null)
                    return NotFound("Person not found.");

                if (!await CanAccessTree(person.FamilyTreeId, userId) || person.CreatedBy != userId)
                    return Unauthorized("Access denied to this tree.");

                if (dto.FamilyMemberId != id)
                    return BadRequest("FamilyMemberId in the request body must match the URL ID.");

                if (dto.MotherId.HasValue && !await _context.FamilyMembers.AnyAsync(p => p.FamilyMemberId == dto.MotherId && p.FamilyTreeId == dto.FamilyTreeId))
                    return BadRequest("Invalid MotherId.");
                if (dto.FatherId.HasValue && !await _context.FamilyMembers.AnyAsync(p => p.FamilyMemberId == dto.FatherId && p.FamilyTreeId == dto.FamilyTreeId))
                    return BadRequest("Invalid FatherId.");

                person.FirstName = dto.FirstName;
                person.LastName = dto.LastName;
                person.Gender = dto.Gender;
                person.DateOfBirth = dto.DateOfBirth;
                person.DateOfDeath = dto.DateOfDeath;
                person.FamilyTreeId = dto.FamilyTreeId;
                person.MotherId = dto.MotherId;
                person.FatherId = dto.FatherId;
                person.CreatedBy = dto.CreatedBy;

                await _context.SaveChangesAsync();

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating person: {ex.Message}");
            }
        }

        // DELETE: api/FamilyMember/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFamilyMember(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated.");

                var person = await _context.FamilyMembers
                    .FirstOrDefaultAsync(p => p.FamilyMemberId == id);
                if (person == null)
                    return NotFound("Person not found.");

                if (!await CanAccessTree(person.FamilyTreeId, userId) || person.CreatedBy != userId)
                    return Unauthorized("Access denied to this tree.");

                // Fetch all relationships where this member is involved (as FromPersonId or ToPersonId)
                var relationships = await _context.Relationships
                    .Where(r => r.FromPersonId == id || r.ToPersonId == id)
                    .ToListAsync();

                if (relationships.Any())
                {
                    _context.Relationships.RemoveRange(relationships);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Deleted {relationships.Count} relationships for member ID: {id}");
                }

                // Clear child references (MotherId and FatherId) for any children
                var hasChildren = await _context.FamilyMembers.AnyAsync(p => p.MotherId == id || p.FatherId == id);
                if (hasChildren)
                {
                    var childrenAsMother = await _context.FamilyMembers
                        .Where(p => p.MotherId == id)
                        .ToListAsync();
                    foreach (var child in childrenAsMother)
                        child.MotherId = null;

                    var childrenAsFather = await _context.FamilyMembers
                        .Where(p => p.FatherId == id)
                        .ToListAsync();
                    foreach (var child in childrenAsFather)
                        child.FatherId = null;

                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Cleared child references for member ID: {id}");
                }

                // Delete the member
                _context.FamilyMembers.Remove(person);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting person: {ex.Message}");
            }
        }

        private async Task<bool> CanAccessTree(int treeId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return false;

            // Check if the user is the owner
            bool isOwner = await _context.FamilyTrees
                .AnyAsync(t => t.FamilyTreeId == treeId && t.OwnerId == userId);

            if (isOwner)
                return true;

            // Check if the user has a role in FamilyTreeUserRoles
            bool hasRole = await _context.FamilyTreeUserRoles
                .AnyAsync(ut => ut.FamilyTreeId == treeId && ut.UserId == userId);

            return hasRole;
        }
    }
}
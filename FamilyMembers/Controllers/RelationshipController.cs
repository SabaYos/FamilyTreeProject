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
    public class RelationshipController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RelationshipController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Relationship/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<RelationshipDto>> GetRelationship(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated.");

                var relationship = await _context.Relationships
                    .Where(r => r.RelationshipId == id)
                    .Select(r => new
                    {
                        Relationship = r,
                        FromPerson = r.FromPerson,
                        FamilyTreeId = r.FromPerson.FamilyTreeId,
                        CreatedBy = r.CreatedBy
                    })
                    .FirstOrDefaultAsync();

                if (relationship == null || relationship.FromPerson == null)
                    return NotFound("Relationship or associated person not found.");

                var (canAccess, _) = await CanAccessTree(relationship.FamilyTreeId, userId);
                if (!canAccess)
                    return Unauthorized("Access denied to this tree.");

                return Ok(new RelationshipDto
                {
                    RelationshipId = relationship.Relationship.RelationshipId,
                    FromPersonId = relationship.Relationship.FromPersonId,
                    ToPersonId = relationship.Relationship.ToPersonId,
                    RelationshipType = relationship.Relationship.RelationshipType.ToString(),
                    StartDate = relationship.Relationship.StartDate,
                    EndDate = relationship.Relationship.EndDate,
                    CreatedBy =relationship.CreatedBy
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving relationship: {ex.Message}");
            }
        }

        // GET: api/Relationship?familyTreeId={familyTreeId}
        [HttpGet]
        public async Task<ActionResult<List<RelationshipDto>>> GetRelationships([FromQuery] int familyTreeId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated.");

                var (canAccess, _) = await CanAccessTree(familyTreeId, userId);
                if (!canAccess)
                    return Unauthorized("Access denied to this tree.");

                var relationships = await _context.Relationships
                    .Include(r => r.FromPerson)
                    .Include(r => r.ToPerson)
                    .Where(r => r.FromPerson.FamilyTreeId == familyTreeId)
                    .Select(r => new RelationshipDto
                    {
                        RelationshipId = r.RelationshipId,
                        FromPersonId = r.FromPersonId,
                        ToPersonId = r.ToPersonId,
                        RelationshipType = r.RelationshipType.ToString(),
                        StartDate = r.StartDate,
                        EndDate = r.EndDate,
                        CreatedBy= r.CreatedBy
                    })
                    .ToListAsync();

                return Ok(relationships);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving relationships: {ex.Message}");
            }
        }

        // POST: api/Relationship
        [HttpPost]
        public async Task<ActionResult<RelationshipDto>> CreateRelationship(RelationshipDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated.");

                var fromPerson = await _context.FamilyMembers
                    .FirstOrDefaultAsync(p => p.FamilyMemberId == dto.FromPersonId);
                var toPerson = await _context.FamilyMembers
                    .FirstOrDefaultAsync(p => p.FamilyMemberId == dto.ToPersonId);

                if (fromPerson == null || toPerson == null)
                    return BadRequest("Invalid FromPersonId or ToPersonId.");

                var (canAccess, role) = await CanAccessTree(fromPerson.FamilyTreeId, userId);
                if (!canAccess || fromPerson.FamilyTreeId != toPerson.FamilyTreeId )
                    return Unauthorized("Invalid tree or access denied.");

                if (role == "Viewer")
                    return Forbid("Viewer role cannot create relationships.");

                if (!TryParseRelationshipType(dto.RelationshipType, out RelationshipType relationshipType))
                    return BadRequest("Invalid RelationshipType.");

                if (await CreatesCycle(dto.FromPersonId, dto.ToPersonId, relationshipType))
                    return BadRequest("This relationship would create a cycle in the family tree.");

                var relationship = new Relationship
                {
                    FromPersonId = dto.FromPersonId,
                    ToPersonId = dto.ToPersonId,
                    RelationshipType = relationshipType,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    CreatedBy = userId
                };

                _context.Relationships.Add(relationship);
                await _context.SaveChangesAsync();

                await SyncFamilyMemberRelationships(relationship, fromPerson, toPerson);

                dto.RelationshipId = relationship.RelationshipId;
                return CreatedAtAction(nameof(GetRelationship), new { id = relationship.RelationshipId }, dto);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating relationship: {ex.Message}");
            }
        }

        // PUT: api/Relationship/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<RelationshipDto>> UpdateRelationship(int id, RelationshipDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated.");

                if (id != dto.RelationshipId)
                    return BadRequest("ID in URL does not match ID in body.");

                if (!TryParseRelationshipType(dto.RelationshipType, out RelationshipType relationshipType))
                    return BadRequest("Invalid RelationshipType.");

                var relationship = await _context.Relationships
                    .Include(r => r.FromPerson)
                    .Include(r => r.ToPerson)
                    .FirstOrDefaultAsync(r => r.RelationshipId == id);

                if (relationship == null || relationship.FromPerson == null)
                    return NotFound("Relationship or associated person not found.");

                var (canAccess, role) = await CanAccessTree(relationship.FromPerson.FamilyTreeId, userId);
                if (!canAccess)
                    return Unauthorized("Access denied to this tree.");

                if (role == "Viewer")
                    return Forbid("Viewer role cannot edit relationships.");

                // For FamilyMember role, check if at least one of the current members was created by the user
                if (role == "Family Member")
                {
                    var fromPersonCreatedBy = await _context.FamilyMembers
                        .Where(p => p.FamilyMemberId == relationship.FromPersonId)
                        .Select(p => p.CreatedBy)
                        .FirstOrDefaultAsync();

                    var toPersonCreatedBy = await _context.FamilyMembers
                        .Where(p => p.FamilyMemberId == relationship.ToPersonId)
                        .Select(p => p.CreatedBy)
                        .FirstOrDefaultAsync();

                    if (fromPersonCreatedBy != userId && toPersonCreatedBy != userId)
                        return Forbid("FamilyMember role can only edit relationships involving members they created.");
                }

                var newFromPerson = await _context.FamilyMembers.FirstOrDefaultAsync(p => p.FamilyMemberId == dto.FromPersonId);
                var newToPerson = await _context.FamilyMembers.FirstOrDefaultAsync(p => p.FamilyMemberId == dto.ToPersonId);

                if (newFromPerson == null || newToPerson == null)
                    return BadRequest("Invalid FromPersonId or ToPersonId.");

                if (newFromPerson.FamilyTreeId != newToPerson.FamilyTreeId)
                    return BadRequest("Persons must belong to the same tree.");

                if (await CreatesCycle(dto.FromPersonId, dto.ToPersonId, relationshipType, relationship.RelationshipId))
                    return BadRequest("This relationship would create a cycle in the family tree.");

                await ClearFamilyMemberRelationships(relationship);

                relationship.FromPersonId = dto.FromPersonId;
                relationship.ToPersonId = dto.ToPersonId;
                relationship.RelationshipType = relationshipType;
                relationship.StartDate = dto.StartDate;
                relationship.EndDate = dto.EndDate;
                relationship.CreatedBy = userId;

                await _context.SaveChangesAsync();

                await SyncFamilyMemberRelationships(relationship, newFromPerson, newToPerson);

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating relationship: {ex.Message}");
            }
        }

        // DELETE: api/Relationship/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRelationship(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated.");

                var relationship = await _context.Relationships
                    .Include(r => r.FromPerson)
                    .Include(r => r.ToPerson)
                    .FirstOrDefaultAsync(r => r.RelationshipId == id);

                if (relationship == null)
                    return NotFound("Relationship not found.");

                var (canAccess, role) = await CanAccessTree(relationship.FromPerson.FamilyTreeId, userId);
                if (!canAccess)
                    return Unauthorized("Access denied to this tree.");

                if (role == "Viewer")
                    return Forbid($"{role} role cannot delete relationships.");

                await ClearFamilyMemberRelationships(relationship);

                _context.Relationships.Remove(relationship);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting relationship: {ex.Message}");
            }
        }

        private async Task<(bool canAccess, string role)> CanAccessTree(int treeId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return (false, null);

            bool isOwner = await _context.FamilyTrees
                .AnyAsync(t => t.FamilyTreeId == treeId && t.OwnerId == userId);

            if (isOwner)
                return (true, "Admin");

            var userRole = await _context.FamilyTreeUserRoles
                .Where(ut => ut.FamilyTreeId == treeId && ut.UserId == userId)
                .Select(ut => ut.Role)
                .FirstOrDefaultAsync();

            return (userRole != null, userRole);
        }

        private bool TryParseRelationshipType(string relationshipTypeStr, out RelationshipType relationshipType)
        {
            switch (relationshipTypeStr?.ToLower())
            {
                case "spouse":
                    relationshipType = RelationshipType.Spouse;
                    return true;
                case "child":
                    relationshipType = RelationshipType.Child;
                    return true;
                case "parent":
                    relationshipType = RelationshipType.Parent;
                    return true;
                default:
                    relationshipType = default;
                    return false;
            }
        }

        private async Task<bool> CreatesCycle(int fromPersonId, int toPersonId, RelationshipType type, int? excludeRelationshipId = null)
        {
            if (type != RelationshipType.Parent && type != RelationshipType.Child)
                return false;

            int parentId = type == RelationshipType.Parent ? fromPersonId : toPersonId;
            int childId = type == RelationshipType.Parent ? toPersonId : fromPersonId;

            HashSet<int> visited = new HashSet<int>();
            return await HasCycle(childId, parentId, visited, excludeRelationshipId);
        }

        private async Task<bool> HasCycle(int currentPersonId, int originalParentId, HashSet<int> visited, int? excludeRelationshipId)
        {
            if (visited.Contains(currentPersonId))
                return currentPersonId == originalParentId;

            visited.Add(currentPersonId);

            var parentRelationships = await _context.Relationships
                .Where(r => r.ToPersonId == currentPersonId && r.RelationshipType == RelationshipType.Parent)
                .Where(r => !excludeRelationshipId.HasValue || r.RelationshipId != excludeRelationshipId)
                .Select(r => r.FromPersonId)
                .ToListAsync();

            foreach (var parentId in parentRelationships)
            {
                if (await HasCycle(parentId, originalParentId, visited, excludeRelationshipId))
                    return true;
            }

            return false;
        }

        private async Task SyncFamilyMemberRelationships(Relationship relationship, FamilyMember fromPerson, FamilyMember toPerson)
        {
            if (TryParseRelationshipType(relationship.RelationshipType.ToString(), out RelationshipType type))
            {
                if (type == RelationshipType.Parent)
                {
                    if (fromPerson.Gender)
                        toPerson.FatherId = fromPerson.FamilyMemberId;
                    else
                        toPerson.MotherId = fromPerson.FamilyMemberId;
                }
                else if (type == RelationshipType.Child)
                {
                    if (toPerson.Gender)
                        fromPerson.FatherId = toPerson.FamilyMemberId;
                    else
                        fromPerson.MotherId = toPerson.FamilyMemberId;
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task ClearFamilyMemberRelationships(Relationship relationship)
        {
            var fromPerson = await _context.FamilyMembers
                .FirstOrDefaultAsync(p => p.FamilyMemberId == relationship.FromPersonId);
            var toPerson = await _context.FamilyMembers
                .FirstOrDefaultAsync(p => p.FamilyMemberId == relationship.ToPersonId);

            if (fromPerson == null || toPerson == null)
                return;

            if (TryParseRelationshipType(relationship.RelationshipType.ToString(), out RelationshipType type))
            {
                if (type == RelationshipType.Parent)
                {
                    if (toPerson.MotherId == fromPerson.FamilyMemberId)
                        toPerson.MotherId = null;
                    if (toPerson.FatherId == fromPerson.FamilyMemberId)
                        toPerson.FatherId = null;
                }
                else if (type == RelationshipType.Child)
                {
                    if (fromPerson.MotherId == toPerson.FamilyMemberId)
                        fromPerson.MotherId = null;
                    if (fromPerson.FatherId == toPerson.FamilyMemberId)
                        fromPerson.FatherId = null;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
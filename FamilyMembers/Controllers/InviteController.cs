using FamilyTreeAPI.Models;
using LoginAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FamilyTreeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InviteController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InviteController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("familytree/{id}/generate-invite")]
        [Authorize]
        public async Task<IActionResult> GenerateInvite(int id)
        {
            // Verify the user is authenticated
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("User not authenticated.");
            }

            // Check if the family tree exists and the user is the owner
            var familyTree = await _context.FamilyTrees
                .FirstOrDefaultAsync(ft => ft.FamilyTreeId == id && ft.OwnerId == userId);
            if (familyTree == null)
            {
                return NotFound($"Family tree with ID '{id}' not found or you are not the owner.");
            }

            // Generate a unique token
            var token = Guid.NewGuid().ToString();

            // Create the invite
            var invite = new FamilyTreeInvite
            {
                Token = token,
                FamilyTreeId = id,
                Role = "Family Member",
                ExpirationDate = DateTime.UtcNow.AddHours(24),
                IsUsed = false,
                RecipientEmail = null
            };

            // Add the invite to the context and save immediately
            try
            {
                _context.Entry(invite).State = EntityState.Added; // Explicitly set state
                await _context.SaveChangesAsync();

                // Debug: Verify the invite was saved
                var savedInvite = await _context.FamilyTreeInvites
                    .FirstOrDefaultAsync(i => i.Token == token);
                if (savedInvite == null)
                {
                    return StatusCode(500, "Failed to save the invite to the database after save.");
                }
                Console.WriteLine($"Invite saved with Id: {savedInvite.Id}, Token: {savedInvite.Token}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }

            // Construct the invite URL
            var inviteUrl = $"https://FamilyTree/invite?token={token}";
            return Ok(new { InviteUrl = inviteUrl });
        }

        [HttpGet("validate")]
        public async Task<IActionResult> ValidateInvite([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("Token is required.");
            }

            var invite = await _context.FamilyTreeInvites
                .FirstOrDefaultAsync(i => i.Token == token);

            if (invite == null)
            {
                return NotFound("Invite not found.");
            }

            if (invite.IsUsed)
            {
                return BadRequest("Invite has already been used.");
            }

            if (invite.ExpirationDate < DateTime.UtcNow)
            {
                return BadRequest("Invite has expired.");
            }

            // Fetch FamilyTree separately since navigation property is removed
            var familyTree = await _context.FamilyTrees
                .FirstOrDefaultAsync(ft => ft.FamilyTreeId == invite.FamilyTreeId);

            return Ok(new
            {
                FamilyTreeId = invite.FamilyTreeId,
                FamilyTreeName = familyTree?.FamilyTreeName ?? "Unknown",
                Role = invite.Role,
               
                
            });
        }

        [HttpPost("accept")]
        [Authorize]
        public async Task<IActionResult> AcceptInvite([FromQuery] string token )
        {
            // Verify the user is authenticated
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("User not authenticated.");
            }

            // Validate the token
            var invite = await _context.FamilyTreeInvites
                .FirstOrDefaultAsync(i => i.Token == token);

            

            if (invite == null)
            {
                return NotFound("Invite not found.");
            }

            if (invite.IsUsed)
            {
                return BadRequest("Invite has already been used.");
            }

            if (invite.ExpirationDate < DateTime.UtcNow)
            {
                return BadRequest("Invite has expired.");
            }

          
            // Check if the user already has a role for this family tree
            var existingRole = await _context.FamilyTreeUserRoles
                .FirstOrDefaultAsync(ut => ut.FamilyTreeId == invite.FamilyTreeId && ut.UserId == userId);
            if (existingRole != null)
            {
                return BadRequest("You already have a role for this family tree.");
            }

            var familyTree = await _context.FamilyTrees
                                      .FirstOrDefaultAsync(ft => ft.FamilyTreeId == invite.FamilyTreeId);

            // Assign the "Family Member" role to the user
            var userRole = new FamilyTreeUserRole
            {
                FamilyTreeId = invite.FamilyTreeId,
                UserId = userId,
                Role = invite.Role,
                FamilyTreeName = familyTree.FamilyTreeName
            };

            _context.FamilyTreeUserRoles.Add(userRole);

            // Mark the invite as used
            invite.IsUsed = true;
            _context.FamilyTreeInvites.Update(invite);

            await _context.SaveChangesAsync();

            return Ok("Invite accepted successfully. You are now a Family Member of the family tree.");
        }
    }
}



/*
 * 
 * eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImExYjJjM2Q0LWU1ZjYtN2c4aC05aTBqLWsxbDJtM240bzVwNiIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJzYWJhYWFhIiwiZXhwIjoxNzQ4MDM3NTM0LCJpc3MiOiJGYW1pbHlUcmVlIiwiYXVkIjoiRmFtaWx5VHJlZVVzZXJzIn0.wMu7Pu8Y6IGcwdRkxBmf9CutVMX5OfYXEx2nAlZkaF8
 
 */
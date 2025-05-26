using System.ComponentModel.DataAnnotations;

namespace FamilyTreeAPI.Models
{
    public class FamilyTreeInvite
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Token { get; set; }
        [Required]
        public int FamilyTreeId { get; set; }
        [Required]
        public string Role { get; set; }
        [Required]
        public DateTime ExpirationDate { get; set; }
        [Required]
        public bool IsUsed { get; set; }
        public string? RecipientEmail { get; set; }
      //  public FamilyTree FamilyTree { get; set; }
    }
}

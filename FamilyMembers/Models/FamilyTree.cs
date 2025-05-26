using System.ComponentModel.DataAnnotations;

namespace FamilyTreeAPI.Models
{
    public class FamilyTree
    {
        [Key]
        public int FamilyTreeId { get; set; }
        [Required]
        public string FamilyTreeName { get; set; } = string.Empty;
        [Required]
        public bool IsPublic { get; set; }
        [Required]
        public string OwnerId { get; set; } = string.Empty;

        public ICollection<FamilyMember> FamilyMembers { get; set; } = new List<FamilyMember>();



    }
}

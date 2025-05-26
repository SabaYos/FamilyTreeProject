using System.ComponentModel.DataAnnotations;
namespace SharedModels
{
    public class FamilyTreeDto
    {
        [Key]
        public int FamilyTreeId { get; set; }
        [Required]
        public string FamilyTreeName { get; set; } = string.Empty;
        [Required]
        public bool IsPublic { get; set; }
        public string OwnerId { get; set; } = string.Empty;
        public string Role { get; set; } // Add this
    }
}

using LoginAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace FamilyTreeAPI.Models
{
    public class FamilyMember
    {
        [Key]
        public int FamilyMemberId { get; set; }

        [Required]
        [StringLength(255)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string LastName { get; set; } = string.Empty ;

        public DateTime? DateOfBirth { get; set; }
        public DateTime? DateOfDeath { get; set; }

        public int? MotherId { get; set; }

        public int? FatherId { get; set; }

        public bool Gender { get; set; }
        public int FamilyTreeId { get; set; }

        public string CreatedBy { get; set; } //to track creator

        // Navigation properties
        public FamilyTree Tree { get; set; } = null!;
        public FamilyMember? Mother { get; set; }
        public FamilyMember? Father { get; set; }
        public ICollection<FamilyMember> ChildrenAsMother { get; set; } = new List<FamilyMember>();
        public ICollection<FamilyMember> ChildrenAsFather { get; set; } = new List<FamilyMember>();
        public ICollection<Relationship> RelationshipsAsFromPerson { get; set; } = new List<Relationship>();
        public ICollection<Relationship> RelationshipsAsToPerson { get; set; } = new List<Relationship>();

    }


}

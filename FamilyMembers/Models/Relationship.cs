using System.ComponentModel.DataAnnotations;
using SharedModels;

namespace FamilyTreeAPI.Models
{ 
    public class Relationship
    {

        [Key]
        public int RelationshipId { get; set; }
        [Required]
        public int FromPersonId { get; set; }

        [Required]
        public int ToPersonId { get; set; }

        [Required]
        public RelationshipType RelationshipType { get; set; }
        [Required]

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Navigation properties
        public FamilyMember FromPerson { get; set; } = null!;
        public FamilyMember ToPerson { get; set; } = null!;
        public string CreatedBy { get; set; } // to track creator

    }

}

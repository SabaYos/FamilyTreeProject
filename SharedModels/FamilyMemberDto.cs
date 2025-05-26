using System.ComponentModel.DataAnnotations;

namespace SharedModels
{
    public class FamilyMemberDto
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
        public string CreatedBy { get; set; } // to track creator

    }


}

using System.ComponentModel.DataAnnotations;

namespace SharedModels
{
    public class RelationshipDto
    {

        [Key]
        public int RelationshipId { get; set; }
        [Required]
        public int FromPersonId { get; set; }

        [Required]
        public int ToPersonId { get; set; }

        [Required]
        public string RelationshipType { get; set; } 
        
        [Required]
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CreatedBy { get; set; } // to track creator

    }


}

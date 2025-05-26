using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoginAPI.Models;

namespace FamilyTreeAPI.Models
{
    public class FamilyTreeUserRole
    {
        [Key]
        public int Id { get; set; }
        public int FamilyTreeId { get; set; }
        public string UserId { get; set; }
        public string Role { get; set; }
        public string FamilyTreeName { get; set; } // For DTO purposes
    }
}

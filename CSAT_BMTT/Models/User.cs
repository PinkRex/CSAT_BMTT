using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CSAT_BMTT.Models
{
    public class User : IdentityUser
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Citizen ID")]
        public int CitizenIdentificationNumber { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Birthday { get; set; }

        [Required]
        public string Adress { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        public int PhoneNumber { get; set; }

        [Required]
        public int ATM { get; set; }
    }
}

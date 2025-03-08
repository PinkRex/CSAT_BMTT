using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CSAT_BMTT.Models
{
    public class User : IdentityUser
    {
        public int Id { get; set; }
        public string? StaticKey { get; set; }
        public string? IvKey { get; set; }
        public string? PublicKey { get; set; }
        public string? PrivateKey { get; set; }

        [Required]
        [Display(Name = "Citizen ID")]
        public string CitizenIdentificationNumber { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Birthday { get; set; }

        [Required]
        public string Adress { get; set; }

        [Required]
        [Display(Name = "Phone")]
        public string PhoneNumber { get; set; }

        [Required]
        public string ATM { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace CSAT_BMTT.Models
{
    public class RegisterModel
    {
        [Required]
        public string CitizenIdentificationNumber { get; set; }

        [Required]
        public string PinCode { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Birthday { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Adress { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string ATM { get; set; }
    }

    public class LoginModel
    {
        [Required]
        public string CitizenIdentificationNumber { get; set; }

        [Required]
        public string Password { get; set; }
    }

}

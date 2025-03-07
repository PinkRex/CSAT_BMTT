using System.ComponentModel.DataAnnotations;

namespace CSAT_BMTT.Models
{
    public class RegisterModel
    {
        [Required]
        public int CitizenIdentificationNumber { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Birthday { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Adress { get; set; }

        [Required]
        public int PhoneNumber { get; set; }

        [Required]
        public int ATM { get; set; }
    }

    public class LoginModel
    {
        [Required]
        public string CitizenIdentificationNumber { get; set; }

        [Required]
        public string Password { get; set; }
    }

}

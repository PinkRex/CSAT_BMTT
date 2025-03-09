using CSAT_BMTT.Models;

namespace CSAT_BMTT.DTOs
{
    public class UserDto : User
    {
        public string PinCode { get; set; }
        
        public UserDto() { }

        public UserDto(User user, string pinCode)
        {
            this.Id = user.Id;
            this.StaticKey = user.StaticKey;
            this.IvKey = user.IvKey;
            this.PublicKey = user.PublicKey;
            this.PrivateKey = user.PrivateKey;
            this.CitizenIdentificationNumber = user.CitizenIdentificationNumber;
            this.Name = user.Name;
            this.Email = user.Email;
            this.Birthday = user.Birthday;
            this.Adress = user.Adress;
            this.PhoneNumber = user.PhoneNumber;
            this.ATM = user.ATM;
            this.PinCode = pinCode;
        }
    }

    public class PinCodeDto
    {
        public int Id { get; set; }
        public string? PinCode { get; set; }
    }
}

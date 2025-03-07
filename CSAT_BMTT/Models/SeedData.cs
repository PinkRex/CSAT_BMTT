using Microsoft.EntityFrameworkCore;
using CSAT_BMTT.Data;

namespace CSAT_BMTT.Models;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using (var context = new CSAT_BMTTContext(
            serviceProvider.GetRequiredService<
                DbContextOptions<CSAT_BMTTContext>>()))
        {
            if (context.User.Any())
            {
                return;
            }
            context.User.AddRange(
                new User
                {
                    CitizenIdentificationNumber = "0012030001234",
                    Name = "Nguyễn Minh Đức",
                    Adress = "Hà Nội",
                    ATM = "CT060406",
                    Birthday = DateTime.Now.ToString(),
                    Email = "nmd@gmail.com",
                    PhoneNumber = "0932412357",
                },
                new User
                {
                    CitizenIdentificationNumber = "0012030002345",
                    Name = "Nguyễn Lưu Quốc Hoàng",
                    Adress = "Hải Dương",
                    ATM = "CT060215",
                    Birthday = DateTime.Now.ToString(),
                    Email = "nlqh@gmail.com",
                    PhoneNumber = "0987364581",
                },
                new User
                {
                    CitizenIdentificationNumber = "0012030003456",
                    Name = "Trần Lưu Dũng",
                    Adress = "Hà Nội",
                    ATM = "CT060408",
                    Birthday = DateTime.Now.ToString(),
                    Email = "tld@gmail.com",
                    PhoneNumber = "0984657384",
                },
                new User
                {
                    CitizenIdentificationNumber = "001203000456",
                    Name = "Nguyễn Gia Huy",
                    Adress = "Quảng Ninh",
                    ATM = "CT060417",
                    Birthday = DateTime.Now.ToString(),
                    Email = "ngh@gmail.com",
                    PhoneNumber = "0945237866",
                }
            );
            context.SaveChanges();
        }
    }
}

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
                    CitizenIdentificationNumber = 1,
                    Name = "Test1",
                    Adress = "dsda",
                    ATM = 3123123,
                    Birthday = DateTime.Now,
                    Email = "dasdasd@gmail.com",
                    PhoneNumber = 241241241,
                },
                new User
                {
                    CitizenIdentificationNumber = 2,
                    Name = "Test2",
                    Adress = "dsda",
                    ATM = 3123123,
                    Birthday = DateTime.Now,
                    Email = "dasdasd@gmail.com",
                    PhoneNumber = 241241241,
                },
                new User
                {
                    CitizenIdentificationNumber = 3,
                    Name = "Test3",
                    Adress = "dsda",
                    ATM = 3123123,
                    Birthday = DateTime.Now,
                    Email = "dasdasd@gmail.com",
                    PhoneNumber = 241241241,
                },
                new User
                {
                    CitizenIdentificationNumber = 4,
                    Name = "Test4",
                    Adress = "dsda",
                    ATM = 3123123,
                    Birthday = DateTime.Now,
                    Email = "dasdasd@gmail.com",
                    PhoneNumber = 241241241,
                }
            );
            context.SaveChanges();
        }
    }
}

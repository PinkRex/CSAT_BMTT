using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CSAT_BMTT.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace CSAT_BMTT.Data
{
    public class CSAT_BMTTContext : IdentityDbContext<User>
    {
        public CSAT_BMTTContext (DbContextOptions<CSAT_BMTTContext> options)
            : base(options) {}

        public DbSet<User> User { get; set; } = default!;

        //protected override void OnModelCreating(ModelBuilder builder)
        //{
        //    base.OnModelCreating(builder);
        //    builder.Entity<User>().ToTable("AspNetUsers");
        //}
    }
}

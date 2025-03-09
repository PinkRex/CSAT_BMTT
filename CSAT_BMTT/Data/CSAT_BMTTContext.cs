using Microsoft.EntityFrameworkCore;

namespace CSAT_BMTT.Data
{
    public class CSAT_BMTTContext : DbContext
    {
        public CSAT_BMTTContext (DbContextOptions<CSAT_BMTTContext> options)
            : base(options) {}

        public DbSet<Models.User> User { get; set; } = default!;
        public DbSet<Models.AccessPermissionModel> AccessPermission { get; set; } = default!;
    }
}

using Microsoft.EntityFrameworkCore;

namespace JOBFLEX.Models
{
    public class JobFlexContext : DbContext
    {
        public JobFlexContext(DbContextOptions<JobFlexContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}
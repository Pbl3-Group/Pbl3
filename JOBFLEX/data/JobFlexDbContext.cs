using JOBFLEX.Models;
using Microsoft.EntityFrameworkCore;

namespace JOBFLEX.Data
{
    public class JobFlexDbContext : DbContext
    {
        public JobFlexDbContext(DbContextOptions<JobFlexDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}
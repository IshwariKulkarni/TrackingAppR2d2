using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace TrackingApp.Entities
{
    public class TrackingDbContext:DbContext
    {
        public TrackingDbContext(DbContextOptions<TrackingDbContext> options) : base(options) { }

        public DbSet<TrackingDB> TrackingDB { get; set; }
    }
}

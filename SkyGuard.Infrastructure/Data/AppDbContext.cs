using Microsoft.EntityFrameworkCore;
using SkyGuard.Core.Models;

namespace SkyGuard.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() { } // Required for migrations

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Incident> Incidents { get; set; }
        public DbSet<SecurityResponse> SecurityResponses { get; set; }
        public DbSet<AuditLogEntry> AuditLogs { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(u => u.ReportedIncidents)
                .WithOne(i => i.ReportedBy)
                .HasForeignKey(i => i.ReportedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.AssignedIncidents)
                .WithOne(i => i.AssignedTo)
                .HasForeignKey(i => i.AssignedToId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Incident>()
                .HasOne(i => i.Response)
                .WithOne(r => r.Incident)
                .HasForeignKey<SecurityResponse>(r => r.IncidentId);
        }
    }
}

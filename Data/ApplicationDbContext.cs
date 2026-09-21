// Defines the EF Core database used by Identity and portal business records.
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SecureEmployeePortal.Data.Entities;

namespace SecureEmployeePortal.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<SecurityEvent> SecurityEvents => Set<SecurityEvent>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AccessRequest> AccessRequests => Set<AccessRequest>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(x => x.FirstName).HasMaxLength(100);
            entity.Property(x => x.LastName).HasMaxLength(100);
            entity.Property(x => x.EmployeeNumber).HasMaxLength(32);
            entity.Property(x => x.JobTitle).HasMaxLength(120);
            entity.Property(x => x.Department).HasMaxLength(120);
        });

        builder.Entity<AttendanceRecord>(entity =>
        {
            entity.HasIndex(x => new { x.UserId, x.ClockInAt });
        });

        builder.Entity<SecurityEvent>(entity =>
        {
            entity.Property(x => x.EventType).HasMaxLength(80);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.HasIndex(x => x.OccurredAt);
            entity.HasIndex(x => x.UserId);
        });

        builder.Entity<AuditLog>(entity =>
        {
            entity.Property(x => x.ActionType).HasMaxLength(120);
            entity.Property(x => x.TargetUserId).HasMaxLength(450);
            entity.Property(x => x.PreviousValue).HasMaxLength(500);
            entity.Property(x => x.NewValue).HasMaxLength(500);
            entity.HasIndex(x => x.OccurredAt);
            entity.HasIndex(x => x.ActorUserId);
        });

        builder.Entity<AccessRequest>(entity =>
        {
            entity.Property(x => x.RequestedResource).HasMaxLength(160);
            entity.Property(x => x.Reason).HasMaxLength(1000);
            entity.Property(x => x.Status).HasMaxLength(30);
            entity.Property(x => x.DecisionNote).HasMaxLength(1000);
            entity.HasIndex(x => x.RequesterUserId);
            entity.HasIndex(x => x.Status);
        });
    }
}

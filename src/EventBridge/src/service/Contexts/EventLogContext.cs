using Microsoft.EntityFrameworkCore;
using Pocco.Svc.EventBridge.Contexts.Models;

namespace Pocco.Svc.EventBridge.Contexts;

public class EventLogContext(DbContextOptions<EventLogContext> options) : DbContext(options) {

  public DbSet<EventLogModel> EventLogs { get; set; } = null!;

  // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
  //   var connectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING") ??
  //                          throw new InvalidOperationException("MYSQL_CONNECTION_STRING environment variable is not set.");

  //   optionsBuilder.UseMySQL(connectionString);

  //   base.OnConfiguring(optionsBuilder);
  // }

  protected override void OnModelCreating(ModelBuilder modelBuilder) {
    modelBuilder.Entity<EventLogModel>()
      .HasKey(e => e.EventId);

    modelBuilder.Entity<EventLogModel>()
      .Property(e => e.EventData)
      .IsRequired()
      .HasDefaultValue("{}");

    base.OnModelCreating(modelBuilder);
  }

  public async Task<List<EventLogModel>> GetLogFromToAsync(DateTime from, DateTime to, int limit = 100) {
    var logs = await EventLogs
      .Where(e => e.FiredAt >= from && e.FiredAt <= to)
      .OrderByDescending(e => e.FiredAt)
      .Take(limit)
      .ToListAsync();

    return logs;
  }
}

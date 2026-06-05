using Microsoft.EntityFrameworkCore;
using PollaMundialista.Domain.Entities;
using PollaMundialista.Infrastructure.Persistence.Configurations;

namespace PollaMundialista.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Prediction> Predictions => Set<Prediction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new MatchConfiguration());
        modelBuilder.ApplyConfiguration(new PredictionConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}

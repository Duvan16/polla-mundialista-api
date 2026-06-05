using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PollaMundialista.Application.Common.Interfaces;
using PollaMundialista.Domain.Entities;
using PollaMundialista.Domain.Enums;

namespace PollaMundialista.Infrastructure.Persistence.Seeder;

public class DatabaseSeeder
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _hasher;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        AppDbContext context,
        IPasswordHasher hasher,
        IConfiguration configuration,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _hasher = hasher;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
        => await _context.Database.MigrateAsync(cancellationToken);

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _context.Users.AnyAsync(cancellationToken))
            return;

        var adminPassword = _configuration["Seeding:AdminPassword"];
        var userPassword = _configuration["Seeding:UserPassword"];

        if (string.IsNullOrWhiteSpace(adminPassword) || string.IsNullOrWhiteSpace(userPassword))
            throw new InvalidOperationException(
                "Seeding is enabled but Seeding:AdminPassword and/or Seeding:UserPassword are not set. " +
                "Provide them via environment variables (Seeding__AdminPassword, Seeding__UserPassword), " +
                "user-secrets, or appsettings.Development.json.");

        _logger.LogInformation("Seeding database...");

        var admin = User.Create("admin@polla.com", _hasher.Hash(adminPassword), "Admin", UserRole.Admin);
        var user = User.Create("user@polla.com", _hasher.Hash(userPassword), "Player One");

        await _context.Users.AddRangeAsync([admin, user], cancellationToken);

        var matches = new[]
        {
            // Group A — Argentina, Brazil, France, Germany (round-robin = 6 matches)
            Match.Create("Group A", "Argentina", "Brazil",   new DateTime(2026, 6, 15, 18, 0, 0, DateTimeKind.Utc)),
            Match.Create("Group A", "France",    "Germany",  new DateTime(2026, 6, 15, 21, 0, 0, DateTimeKind.Utc)),
            Match.Create("Group A", "Argentina", "France",   new DateTime(2026, 6, 19, 15, 0, 0, DateTimeKind.Utc)),
            Match.Create("Group A", "Germany",   "Brazil",   new DateTime(2026, 6, 19, 18, 0, 0, DateTimeKind.Utc)),
            Match.Create("Group A", "Argentina", "Germany",  new DateTime(2026, 6, 23, 21, 0, 0, DateTimeKind.Utc)),
            Match.Create("Group A", "Brazil",    "France",   new DateTime(2026, 6, 23, 21, 0, 0, DateTimeKind.Utc)),
            // Group B — Spain, Portugal, England, Netherlands (round-robin = 6 matches)
            Match.Create("Group B", "Spain",     "Portugal",    new DateTime(2026, 6, 16, 18, 0, 0, DateTimeKind.Utc)),
            Match.Create("Group B", "England",   "Netherlands", new DateTime(2026, 6, 16, 21, 0, 0, DateTimeKind.Utc)),
            Match.Create("Group B", "Spain",     "England",     new DateTime(2026, 6, 20, 15, 0, 0, DateTimeKind.Utc)),
            Match.Create("Group B", "Portugal",  "Netherlands", new DateTime(2026, 6, 20, 18, 0, 0, DateTimeKind.Utc)),
            Match.Create("Group B", "Spain",     "Netherlands", new DateTime(2026, 6, 24, 21, 0, 0, DateTimeKind.Utc)),
            Match.Create("Group B", "Portugal",  "England",     new DateTime(2026, 6, 24, 21, 0, 0, DateTimeKind.Utc)),
        };

        await _context.Matches.AddRangeAsync(matches, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Database seeded successfully.");
    }
}

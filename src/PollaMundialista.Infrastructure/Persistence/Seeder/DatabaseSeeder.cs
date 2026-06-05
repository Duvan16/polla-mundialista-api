using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PollaMundialista.Application.Common.Interfaces;
using PollaMundialista.Domain.Entities;
using PollaMundialista.Domain.Enums;

namespace PollaMundialista.Infrastructure.Persistence.Seeder;

public class DatabaseSeeder
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _hasher;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(AppDbContext context, IPasswordHasher hasher, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _hasher = hasher;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.MigrateAsync(cancellationToken);

        if (await _context.Users.AnyAsync(cancellationToken))
            return;

        _logger.LogInformation("Seeding database...");

        var admin = User.Create("admin@polla.com", _hasher.Hash("Admin123!"), "Admin", UserRole.Admin);
        var user = User.Create("user@polla.com", _hasher.Hash("User123!"), "Player One");

        await _context.Users.AddRangeAsync([admin, user], cancellationToken);

        var matches = new[]
        {
            Match.Create("Group A", "Argentina", "Brazil", new DateTime(2026, 6, 15, 18, 0, 0, DateTimeKind.Utc)),
            Match.Create("Group A", "France", "Germany", new DateTime(2026, 6, 15, 21, 0, 0, DateTimeKind.Utc)),
            Match.Create("Group B", "Spain", "Portugal", new DateTime(2026, 6, 16, 18, 0, 0, DateTimeKind.Utc)),
        };

        await _context.Matches.AddRangeAsync(matches, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Database seeded successfully.");
    }
}

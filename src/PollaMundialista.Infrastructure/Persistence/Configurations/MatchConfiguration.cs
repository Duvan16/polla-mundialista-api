using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PollaMundialista.Domain.Entities;

namespace PollaMundialista.Infrastructure.Persistence.Configurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.GroupName).HasMaxLength(50).IsRequired();
        builder.Property(m => m.HomeTeam).HasMaxLength(100).IsRequired();
        builder.Property(m => m.AwayTeam).HasMaxLength(100).IsRequired();
        builder.Property(m => m.MatchDate).IsRequired();
        builder.Property(m => m.IsFinished).IsRequired();
    }
}

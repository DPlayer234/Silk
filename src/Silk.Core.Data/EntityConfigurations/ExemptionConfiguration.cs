using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.EntityConfigurations;

public class ExemptionConfiguration : IEntityTypeConfiguration<ExemptionEntity>
{
    public void Configure(EntityTypeBuilder<ExemptionEntity> builder)
    {
        builder.Property(ex => ex.GuildID)
               .HasConversion(new SnowflakeConverter());

        builder.Property(ex => ex.TargetID)
               .HasConversion(new SnowflakeConverter());
    }
}
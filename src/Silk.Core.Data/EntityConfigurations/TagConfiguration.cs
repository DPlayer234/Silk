﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.EntityConfigurations;

public class TagEntityConfiguration : IEntityTypeConfiguration<TagEntity>
{
    public void Configure(EntityTypeBuilder<TagEntity> builder)
    {
        builder.Property(t => t.OwnerID)
               .HasConversion(new SnowflakeConverter());
        
        builder.Property(t => t.GuildID)
               .HasConversion(new SnowflakeConverter());
    }
}
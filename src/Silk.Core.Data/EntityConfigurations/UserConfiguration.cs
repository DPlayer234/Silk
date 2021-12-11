﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.EntityConfigurations;

public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.HasKey(u => new { Id= u.ID, GuildId = u.GuildID });

        builder.Property(u => u.ID)
               .ValueGeneratedNever()
               .HasConversion(new SnowflakeConverter());
        
        builder.Property(u => u.GuildID)
               .ValueGeneratedNever()
               .HasConversion(new SnowflakeConverter());
        
        builder.HasOne(u => u.History)
               .WithOne(h => h.User)
               .HasForeignKey<UserHistoryEntity>(u => new {
                    UserId                                   = u.UserID,
                                                     GuildId = u.GuildID });
    }
}
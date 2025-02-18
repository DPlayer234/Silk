﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Remora.Rest.Core;
using Silk.Data.Entities.Guild.Config;

namespace Silk.Data.Entities;

[Table("guild_configs")]
public class GuildConfigEntity
{
    // Database requisites. //
    /// <summary>
    ///     The Primary Key (PK) of the model.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     Requisite property to form a Foreign Key (FK)
    /// </summary>
    [Column("guild_id")]
    public Snowflake GuildID { get; set; }

    /// <summary>
    ///     Requisite property to form a Foreign Key (FK)
    /// </summary>
    public GuildEntity Guild { get; set; }

    /// <summary>
    /// Greetings configured for this guild.
    /// </summary>
    [Column("greetings")]
    public List<GuildGreetingEntity> Greetings { get; set; } = new();
    
    /// <summary>
    ///     Id of the role to apply when muting members.
    /// </summary>
    [Column("mute_role")]
    public Snowflake MuteRoleID { get; set; }
    
    /// <summary>
    /// Whether to use "native mutes" (Timeouts) when muting members.
    ///
    /// This is only applicable if the mute lasts for four weeks or less.
    /// </summary>
    [Column("use_native_mute")]
    public bool UseNativeMute { get; set; }

    public InviteConfigEntity Invites { get; set; } = new();
    
    /// <summary>
    ///     The maximum amount of users that can be mentioned in a single message.
    /// </summary>
    [Column("max_user_mentions")]
    public int MaxUserMentions { get; set; }

    /// <summary>
    ///     The maximum amount of roles that can be mentioned in a single role.
    /// </summary>
    [Column("max_role_mentions")]
    public int MaxRoleMentions { get; set; }
    
    /// <summary>
    ///     Whether to use increasingly severe infractions when a user is automatically warned.
    /// </summary>
    [Column("progressive_infractions")]
    public bool ProgressiveStriking { get; set; }
    
    /// <summary>
    ///     All active auto-mod exemptions on the guild.
    /// </summary>
    [Column("exemptions")]
    public List<ExemptionEntity> Exemptions { get; set; } = new();

    /// <summary>
    ///     Whether or not to even scan for phishing links on a server.
    /// </summary>
    [Column("detect_phishing")]
    public bool DetectPhishingLinks { get; set; }
    
    /// <summary>
    /// Whether or not to automatically ban users who exhibit suspicious usernames similar to known phishing names.
    /// </summary>
    [Column("ban_suspicious_usernames")]
    public bool BanSuspiciousUsernames { get; set; }

    /// <summary>
    ///     Whether or not phishing links should be deleted.
    /// </summary>
    [Column("delete_detected_phishing")]
    public bool DeletePhishingLinks { get; set; }

    /// <summary>
    /// Whether Silk! should automatically track and detect potential raids.
    /// </summary>
    [Column("detect_raids")]
    public bool EnableRaidDetection { get; set; }
    
    /// <summary>
    /// How many sequential joins beyond this threshold over a 1 minute period should be considered a raid.
    /// </summary>
    [Column("raid_threshold")]
    public int RaidDetectionThreshold { get; set; }

    /// <summary>
    /// How long (in seconds) after the last join that raid mode should be automatically disabled.
    /// </summary>
    [Column("raid_decay_seconds")]
    public int RaidCooldownSeconds { get; set; } = 120;

    /// <summary>
    ///     Gets various logging-related settings.
    /// </summary>
    public GuildLoggingConfigEntity Logging { get; set; } = new();

    /// <summary>
    ///     A list of steps depending on the number of infractions a <see cref="UserEntity" /> has.
    /// </summary>
    public List<InfractionStepEntity> InfractionSteps { get; set; } = new();

    /// <summary>
    ///     A dictionary containing "named" infraction steps, where the key to the dictionary represents a discriminated action,
    ///     which may have different consequences than another. In the event a key is not found in this dictionary, it should be assumed the action would
    ///     result to <see cref="InfractionType.Strike" />.
    /// </summary>
    public Dictionary<string, InfractionStepEntity> NamedInfractionSteps { get; set; } = new();
}
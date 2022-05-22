using System;
using Remora.Rest.Core;

namespace Silk.Data.DTOs.Guilds.Users;

/// <summary>
/// Represents a record of a user joining and leaving a guild.
/// </summary>
/// <param name="GuildID">The ID of the guild that was joined.</param>
/// <param name="Joined">The timestamp the user joined.</param>
/// <param name="Left">The timestamp the user left.</param>
public record UserHistoryDTO(Snowflake GuildID, DateTimeOffset Joined, DateTimeOffset? Left);
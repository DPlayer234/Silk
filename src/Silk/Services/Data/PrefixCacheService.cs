﻿using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Remora.Rest.Core;
using Silk.Data.Entities;
using Silk.Data.MediatR.Guilds;
using Silk.Services.Interfaces;
using Silk.Shared.Constants;
using Silk.Shared.Types;

namespace Silk.Services.Data;

/// <inheritdoc cref="IPrefixCacheService" />
public sealed class PrefixCacheService : IPrefixCacheService
{
    private readonly ILogger<PrefixCacheService> _logger;
    private readonly IMediator                   _mediator;
    private readonly IMemoryCache                _memoryCache;
    public PrefixCacheService(ILogger<PrefixCacheService> logger, IMemoryCache memoryCache, IMediator mediator)
    {
        _logger      = logger;
        _memoryCache = memoryCache;
        _mediator    = mediator;
    }

    [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
    public string RetrievePrefix(Snowflake? guildId)
    {
        if (guildId is null) return string.Empty;
        if (_memoryCache.TryGetValue(ConfigKeyHelper.GenerateGuildPrefixKey(guildId.Value), out object? prefix)) return (string)prefix;
        return GetDatabasePrefixAsync(guildId.Value).GetAwaiter().GetResult();
    }

    // I don't know if updating a reference will update 
    public void UpdatePrefix(Snowflake id, string prefix)
    {
        object key = ConfigKeyHelper.GenerateGuildPrefixKey(id);

        _memoryCache.TryGetValue(key, out string oldPrefix);
        _memoryCache.Set(key, prefix);
        _logger.LogDebug($"Updated prefix for {id} - {oldPrefix} -> {prefix}");
    }


    private async Task<string> GetDatabasePrefixAsync(Snowflake guildId)
    {
        GuildEntity guild = await _mediator.Send(new GetOrCreateGuild.Request(guildId, StringConstants.DefaultCommandPrefix));
        _memoryCache.Set(ConfigKeyHelper.GenerateGuildPrefixKey(guildId), guild.Prefix);
        return guild.Prefix;
    }
}
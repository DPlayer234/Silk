﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR.Infractions;

public record GetUserInfractionsRequest(ulong GuildId, ulong UserId) : IRequest<IEnumerable<InfractionEntity>>;

public class GetUserInfractionsHandler : IRequestHandler<GetUserInfractionsRequest, IEnumerable<InfractionEntity>>
{
    private readonly GuildContext _db;
    public GetUserInfractionsHandler(GuildContext db) => _db = db;

    public async Task<IEnumerable<InfractionEntity>> Handle(GetUserInfractionsRequest request, CancellationToken cancellationToken)
    {
        UserEntity? user = await _db
                                .Users
                                .Include(u => u.Infractions)
                                .FirstOrDefaultAsync(u => u.Id == request.UserId && u.GuildId == request.GuildId, cancellationToken);

        return user?.Infractions ?? Array.Empty<InfractionEntity>().AsEnumerable();
    }
}
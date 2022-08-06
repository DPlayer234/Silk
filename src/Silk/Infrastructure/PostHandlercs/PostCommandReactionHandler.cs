using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Services;
using Remora.Results;
using Silk.Infrastructure;

namespace Silk;

public class PostCommandReactionHandler : IPostExecutionEvent
{
    private readonly IDiscordRestChannelAPI _channels;
    public PostCommandReactionHandler(IDiscordRestChannelAPI channels) => _channels = channels;

    public async Task<Result> AfterExecutionAsync(ICommandContext context, IResult commandResult, CancellationToken ct = default)
    {
        if (!commandResult.IsSuccess)
        {
            return Result.FromSuccess();
        }

        if (context is not MessageContext mc)
        {
            return Result.FromSuccess();
        }

        if (commandResult.Inner!.Inner!.Inner!.Inner is not Result<ReactionResult> re)
        {
            return Result.FromSuccess();   
        }
        
        await _channels.CreateReactionAsync
        (
         context.ChannelID,
         mc.MessageID,
         re.Entity.Reaction.TryPickT0(out var snowflake, out var unicode) 
             ? $"_:{snowflake}" 
             : unicode,
         ct
        );

        if (re.Entity.Message.IsDefined(out var message))
        {
            await _channels.CreateMessageAsync(context.ChannelID, message, ct: ct);
        }
        
        return Result.FromSuccess();
    }
}
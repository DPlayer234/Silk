using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer.Localisation;
using Recognizers.Text.DateTime.Wrapper;
using Recognizers.Text.DateTime.Wrapper.Models.BclDateTime;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Interactivity;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Commands.Furry.Types;
using Silk.Commands.General;
using Silk.Extensions;
using Silk.Services.Bot;
using Silk.Services.Guild;

namespace Silk.Interactivity;

[Ephemeral]
public class ReminderModalHandler : IModalInteractiveEntity
{
    private const string ReminderTimeNotPresent = "It seems you didn't specify a time in your reminder.\n" +
                                                  "I can recognize times like 10m, 5h, 2h30m, and even natural language like 'three hours from now' and 'in 2 days'";

    private readonly TimeHelper                 _timeHelper;
    private readonly ReminderService            _reminders;
    private readonly InteractionContext         _context;
    private readonly IDiscordRestChannelAPI     _channels;
    private readonly IDiscordRestInteractionAPI _interactions;
    
    
    public ReminderModalHandler
    (
        TimeHelper timeHelper,
        ReminderService reminders,
        InteractionContext context,
        IDiscordRestChannelAPI channels,
        IDiscordRestInteractionAPI interactions
    )
    {
        _timeHelper   = timeHelper;
        _reminders    = reminders;
        _context      = context;
        _channels     = channels;
        _interactions = interactions;
    }

    public Task<Result<bool>> IsInterestedAsync(ComponentType? componentType, string customID, CancellationToken ct = default) 
        => Task.FromResult<Result<bool>>(componentType is null && customID is "reminder-modal");

    public async Task<Result> HandleInteractionAsync(IUser user, string customID, IReadOnlyList<IPartialMessageComponent> components, CancellationToken ct = default)
    {
        components = components.SelectMany(c => (c as PartialActionRowComponent)!.Components.Value).ToArray();
        
        var raw = (components[0] as PartialTextInputComponent)!.Value.Value;

        var reply = new Snowflake(ulong.Parse((components[1] as PartialTextInputComponent)!.CustomID.Value));
        
        var offset     = await _timeHelper.GetOffsetForUserAsync(_context.User.ID);
        var timeResult = _timeHelper.ExtractTime(raw, offset, out _);

        if (!timeResult.IsDefined(out var parsedTime))
        {
            var informResult = await _interactions.CreateFollowupMessageAsync
            (
             _context.ApplicationID,
             _context.Token,
             timeResult.Error!.Message,
             ct: ct
            );
            
            return informResult.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(informResult.Error);
        }

        if (parsedTime <= TimeSpan.Zero)
        {
            var informResult = await _interactions.CreateFollowupMessageAsync
            (
             _context.ApplicationID,
             _context.Token,
             "It seems you specified a time in the past.\n" +
             "Please specify a time in the future.",
             ct: ct
            );
            
            return informResult.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(informResult.Error);
        }

        if (parsedTime < TimeSpan.FromMinutes(3))
        {
            var minTimeResult = await _interactions.CreateFollowupMessageAsync
            (
             _context.ApplicationID,
             _context.Token,
             "You can't set a reminder less than three minutes!",
             flags: MessageFlags.Ephemeral,
             ct: ct
            );
            
            return minTimeResult.IsSuccess
                ? Result.FromSuccess()
                : Result.FromError(minTimeResult.Error);
        }
        
        var reminderTime = DateTimeOffset.UtcNow + parsedTime;

        var messageResult = await _channels.GetChannelMessageAsync(_context.ChannelID, reply, ct);
        
        if (!messageResult.IsDefined(out var message))
            return Result.FromError(messageResult.Error!);
        
        await _reminders.CreateReminderAsync
        (
         reminderTime,
         _context.User.ID,
         _context.ChannelID,
         null,
         null,
         (components[1] as PartialTextInputComponent)!.Value.IsDefined(out var reminderText) ? reminderText : null,
         message.Content,
         reply,
         message.Author.ID
        );

        var res =  await _interactions.CreateFollowupMessageAsync
        (
         _context.ApplicationID,
         _context.Token,
         $"Done! I'll remind you {reminderTime.ToTimestamp()}!",
         flags: MessageFlags.Ephemeral,
         ct: ct
        );

        return res.IsSuccess ? Result.FromSuccess() : Result.FromError(res.Error);
    }
}
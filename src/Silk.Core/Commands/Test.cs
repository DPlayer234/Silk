﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Humanizer;
using MediatR;
using NpgsqlTypes;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Core.Data.MediatR.Guilds.Config;
using Silk.Core.Data.Models;
using Silk.Core.Utilities;
using Silk.Extensions;
using Silk.Extensions.DSharpPlus;
using Silk.Shared.Constants;

namespace Silk.Core.Commands
{
	[Group("config")]
	[RequireFlag(UserFlag.Staff)]
	[Description("View and edit configuration for the current guild.")]
	public class TestConfigModule : BaseCommandModule
	{
		private readonly IMediator _mediator;
		public TestConfigModule(IMediator mediator) => _mediator = mediator;
		
		
		// Wrapper that points to config view //
		[GroupCommand]
		public Task Default(CommandContext ctx) =>
			ctx.CommandsNext.ExecuteCommandAsync(ctx.CommandsNext.CreateContext(ctx.Message, ctx.Prefix, ctx.CommandsNext.RegisteredCommands["config view"]));
		
		[Group("view")]
		public sealed class ViewConfigModule : BaseCommandModule
		{
			private readonly IMediator _mediator;
			public ViewConfigModule(IMediator mediator) => _mediator = mediator;

			private string GetCountString(int count) => count is 0 ? "Not set/enabled" : count.ToString();


			[GroupCommand]
			[Description("View the current config.")]
			public async Task View(CommandContext ctx)
			{
				var config = await _mediator.Send(new GetGuildConfigRequest(ctx.Guild.Id));
				var modConfig = await _mediator.Send(new GetGuildModConfigRequest(ctx.Guild.Id));

				var embed = new DiscordEmbedBuilder();
				var contentBuilder = new StringBuilder();

				contentBuilder
					.Clear()
					.AppendLine("**General Config:**")
					.AppendLine("__Greeting:__ ")
					.AppendLine($"> Option: {config.GreetingOption.Humanize()}")
					.AppendLine($"> Greetting channel {(config.GreetingOption is GreetingOption.DoNotGreet ? "N/A" : $"<#{config.GreetingChannel}>")}")
					.AppendLine($"> Greeting text: {(config.GreetingOption is GreetingOption.DoNotGreet ? "N/A" : $"[See {ctx.Prefix}config view greeting]")}")
					.AppendLine()
					.AppendLine()
					.AppendLine("**Moderation Config:**")
					.AppendLine($"Max role mentions: {GetCountString(modConfig.MaxRoleMentions)}")
					.AppendLine($"Max user mentions: {GetCountString(modConfig.MaxUserMentions)}")
					.AppendLine()
					.AppendLine($"Mute role: {(modConfig.MuteRoleId is 0 ? "Not set" : $"<@&{modConfig.MuteRoleId}>")}")
					.AppendLine($"Logging channel: {(modConfig.LoggingChannel is var count and not 0 ? $"<#{count}>" : "Not set")}")
					.AppendLine()
					.AppendLine("__Invites:__")
					.AppendLine($"> Scan invite: <:_:{(modConfig.ScanInvites ? Emojis.ConfirmId : Emojis.DeclineId)}>")
					.AppendLine($"> Infract on invite: <:_:{(modConfig.WarnOnMatchedInvite ? Emojis.ConfirmId : Emojis.DeclineId)}>")
					.AppendLine($"> Delete matched invite: <:_:{(modConfig.DeleteMessageOnMatchedInvite ? Emojis.ConfirmId : Emojis.DeclineId)}>")
					.AppendLine($@"> Use agressive invite matching: <:_:{(modConfig.UseAggressiveRegex ? Emojis.ConfirmId : Emojis.DeclineId)}>>>")
					.AppendLine($"> Allowed invites: {(modConfig.AllowedInvites.Count is 0 ? "None" : $"{modConfig.AllowedInvites.Count} allowed invites [See {ctx.Prefix}config view invites]")}")
					.AppendLine("Aggressive pattern matching regex:")
					.AppendLine(@"`disc((ord)?(((app)?\.com\/invite)|(\.gg)))\/([A-z0-9-_]{2,})`")
					.AppendLine()
					.AppendLine("__Infractions:__")
					.AppendLine($"> Infraction steps: {(modConfig.InfractionSteps.Count is var dictCount and not 0 ? $"{dictCount} steps [See {ctx.Prefix}config view infractions]" : "Not configured")}")
					.AppendLine($"> Infraction steps (named): {((modConfig.NamedInfractionSteps?.Count ?? 0) is var infNameCount and not 0 ? $"{infNameCount} steps [See {ctx.Prefix}config view infractions]" : "Not configured")}")
					.AppendLine($"> Auto-escalate automod infractions: <:_:{(modConfig.AutoEscalateInfractions ? Emojis.ConfirmId : Emojis.DeclineId)}>");

				embed
					.WithTitle($"Configuration for {ctx.Guild.Name}:")
					.WithColor(DiscordColor.Azure)
					.WithDescription(contentBuilder.ToString());

				await ctx.RespondAsync(embed);
			}

			[Command]
			[Description("View in-depth greeting-related config.")]
			public async Task Greeting(CommandContext ctx)
			{
				var contentBuilder = new StringBuilder();
				var config = await _mediator.Send(new GetGuildConfigRequest(ctx.Guild.Id));

				contentBuilder
					.Clear()
					.AppendLine("__**Greeting option:**__")
					.AppendLine($"> Option: {config.GreetingOption.Humanize()}")
					.AppendLine($"> Greetting channel {(config.GreetingOption is GreetingOption.DoNotGreet ? "N/A" : $"<#{config.GreetingChannel}>")}")
					.AppendLine($"> Greeting text: {(config.GreetingOption is GreetingOption.DoNotGreet ? "N/A" : $"\n\n{config.GreetingText}")}")
					.AppendLine($"> Greeting role: {(config.GreetingOption is GreetingOption.GreetOnRole && config.VerificationRole is var role and not 0 ? $"<@&{role}>" : "N/A")}");

				var explanation = config.GreetingOption switch
				{
					GreetingOption.DoNotGreet => "I will not greet members at all.",
					GreetingOption.GreetOnJoin => "I will greet members as soon as they join",
					GreetingOption.GreetOnRole => "I will greet members when they're given a specific role",
					GreetingOption.GreetOnScreening => "I will greet members when they pass membership screening. Only applicable to community servers.",
					_ => throw new ArgumentOutOfRangeException()
				};

				contentBuilder
					.AppendLine()
					.AppendLine("**Greeting option explanation:**")
					.AppendLine(explanation);

				var embed = new DiscordEmbedBuilder()
					.WithColor(DiscordColor.Azure)
					.WithTitle($"Config for {ctx.Guild.Name}")
					.WithDescription(contentBuilder.ToString());

				await ctx.RespondAsync(embed);
			}

			[Command]
			[Description("View in-depth invite related config.")]
			public async Task Invites(CommandContext ctx)
			{
				//TODO: config view invites-list
				var config = await _mediator.Send(new GetGuildModConfigRequest(ctx.Guild.Id));
				var contentBuilder = new StringBuilder();

				contentBuilder
					.Clear()
					.AppendLine("__Invites:__")
					.AppendLine($"> Scan invite: <:_:{(config.ScanInvites ? Emojis.ConfirmId : Emojis.DeclineId)}>")
					.AppendLine($"> Infract on invite: <:_:{(config.WarnOnMatchedInvite ? Emojis.ConfirmId : Emojis.DeclineId)}>")
					.AppendLine($"> Delete matched invite: <:_:{(config.DeleteMessageOnMatchedInvite ? Emojis.ConfirmId : Emojis.DeclineId)}>")
					.AppendLine($@"> Use agressive invite matching : <:_:{(config.UseAggressiveRegex ? Emojis.ConfirmId : Emojis.DeclineId)}>")
					.AppendLine()
					.AppendLine($"> Allowed invites: {(config.AllowedInvites.Count is 0 ? "There are no whitelisted invites!" : $"{config.AllowedInvites.Count} allowed invites:")}")
					.AppendLine($"> {config.AllowedInvites.Take(15).Select(inv => $"`{inv.VanityURL}`\n").Join("> ")}");

				if (config.AllowedInvites.Count > 15)
					contentBuilder.AppendLine($"..Plus {config.AllowedInvites.Count - 15} more");
				
				contentBuilder
					.AppendLine("Aggressive pattern matching are any invites that match this rule:")
					.AppendLine(@"`disc((ord)?(((app)?\.com\/invite)|(\.gg)))\/([A-z0-9-_]{2,})`");

				var embed = new DiscordEmbedBuilder()
					.WithTitle($"Configuration for {ctx.Guild.Name}:")
					.WithColor(DiscordColor.Azure)
					.WithDescription(contentBuilder.ToString());

				await ctx.RespondAsync(embed);
			}

			[Command]
			[Description("View in-depth infraction-ralated config.")]
			public async Task Infractions(CommandContext ctx)
			{
				var config = await _mediator.Send(new GetGuildModConfigRequest(ctx.Guild.Id));
				
				var contentBuilder = new StringBuilder()
					.AppendLine("__Infractions:__")
					.AppendLine($"> Infraction steps: {(config.InfractionSteps.Count is var dictCount and not 0 ? $"{dictCount} steps" : "Not configured")}")
					.AppendLine($"> Infraction steps (named): {((config.NamedInfractionSteps?.Count ?? 0) is var infNameCount and not 0 ? $"{infNameCount} steps" : "Not configured")}")
					.AppendLine($"> Auto-escalate automod infractions: <:_:{(config.AutoEscalateInfractions ? Emojis.ConfirmId : Emojis.DeclineId)}>");

				if (config.InfractionSteps.Any())
				{
					contentBuilder
						.AppendLine()
						.AppendLine("Infraction steps:")
						.AppendLine(config.InfractionSteps.Select((inf, count) => $"` {count + 1} ` strikes -> {inf.Type} {(inf.Duration == NpgsqlTimeSpan.Zero ? "" : $"For {inf.Duration.Time.Humanize()}")}").Join("\n"));
				}
				
				if (config.NamedInfractionSteps?.Any() ?? false)
				{
					contentBuilder
					.AppendLine()
					.AppendLine("Auto-Mod action steps:")
					.AppendLine(config.NamedInfractionSteps.Select(inf => $"`{inf.Key}` -> {inf.Value.Type} {(inf.Value.Duration == NpgsqlTimeSpan.Zero ? "" : $"For {inf.Value.Duration.Time.Humanize()}")}").Join("\n"));
				}
				
				var embed = new DiscordEmbedBuilder()
					.WithTitle($"Configuration for {ctx.Guild.Name}:")
					.WithColor(DiscordColor.Azure)
					.WithDescription(contentBuilder.ToString());

				await ctx.RespondAsync(embed);
			}
		}

		[Group("edit")]
		public sealed class TestEditConfigModule : BaseCommandModule
		{
			private readonly DiscordButtonComponent _yesButton = new(ButtonStyle.Success, "confirm action", null, false, new(Emojis.ConfirmId));
			private readonly DiscordButtonComponent _noButton = new(ButtonStyle.Danger, "decline action", null, false, new(Emojis.DeclineId));

			private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _tokens = new();

			
			
			/// <summary>
			/// Waits indefinitely for user confirmation unless the associated token is cancelled.
			/// </summary>
			/// <param name="user">The id of the user to assign a token to and wait for input from.</param>
			/// <param name="channel">The channel to send a message to, to request user input.</param>
			/// <returns>True if the user selected true, or false if the user selected no OR the cancellation token was cancelled.</returns>
			private async Task<bool> GetButtonConfirmationUserInputAsync(DiscordUser user, DiscordChannel channel)
			{
				var builder = new DiscordMessageBuilder().WithContent("Are you sure?").AddComponents(_yesButton, _noButton);

				var message = await builder.SendAsync(channel);

				var interactivityResult = await channel.GetClient().GetInteractivity().WaitForButtonAsync(message, user, CancellationToken.None);

				if (interactivityResult.TimedOut) // CT was yeeted. //
					return false;
				
				// Nobody likes 'This interaction failed'. //
				await interactivityResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
				
				return interactivityResult.Result.Id == _yesButton.CustomId;
			}

			
			/// <summary>
			/// Cancels and removes the token with the specified id if it exists.
			/// </summary>
			/// <param name="id">The id of the user to look up.</param>
			private void CancelCurrentTokenIfApplicable(ulong id)
			{
				if (_tokens.TryRemove(id, out var token))
				{
					token.Cancel();
					token.Dispose();
				}
			}
			
			/// <summary>
			/// Gets a <see cref="CancellationToken"/>, creating one if necessary.
			/// </summary>
			/// <param name="id">The id of the user to assign the token to.</param>
			/// <returns>The returned or generated token.</returns>
			private CancellationToken GetTokenFromWaitQueue(ulong id) => _tokens.GetOrAdd(id, id => _tokens[id] = new()).Token;
		}
	}
}
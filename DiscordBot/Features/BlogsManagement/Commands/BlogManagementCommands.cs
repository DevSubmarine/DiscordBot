using Discord;
using Discord.Interactions;
using Discord.Net;
using TehGM.Utilities.Randomization;

namespace DevSubmarine.DiscordBot.BlogsManagement.Commands
{
    [Group("blog", "Commands for managing blog channels")]
    [EnabledInDm(false)]
    public class BlogManagementCommands : DevSubInteractionModule
    {
        private readonly IBlogChannelManager _manager;
        private readonly IBlogChannelNameConverter _nameConverter;
        private readonly IRandomizer _randomizer;
        private readonly BlogsManagementOptions _options;
        private readonly ILogger _log;

        public BlogManagementCommands(IBlogChannelManager manager, IBlogChannelNameConverter nameConverter, IRandomizer randomizer,
            IOptionsMonitor<BlogsManagementOptions> options, ILogger<BlogManagementCommands> log)
        {
            this._manager = manager;
            this._nameConverter = nameConverter;
            this._randomizer = randomizer;
            this._options = options.CurrentValue;
            this._log = log;
        }

        [SlashCommand("create", "Creates a blog channel for user")]
        public async Task CmdCreateAsync(
            [Summary("User", "Which user to create channel for; can only be used by administrators")] IGuildUser user = null,
            [Summary("NSFW", "Should the channel be marked as NSFW?")] bool nsfw = false)
        {
            await base.DeferAsync(options: base.GetRequestOptions()).ConfigureAwait(false);

            // creating channel for other user should only be possible for admins
            IGuildUser callerUser = await base.Context.Guild.GetGuildUserAsync(base.Context.User.Id, base.CancellationToken).ConfigureAwait(false);
            if (user != null)
            {
                if (!CreatingForSelf() && !callerUser.IsOwner() && !callerUser.GuildPermissions.Administrator)
                {
                    await base.ModifyOriginalResponseAsync(msg => msg.Content = $"{ResponseEmoji.Failure} You have no permissions to create blogs for other users.");
                    return;
                }
            }
            else
                user = callerUser;


            if (!this._nameConverter.TryConvertUsername(user.Username, out string channelName))
            {
                string usernameMention = CreatingForSelf() ? "Your username" : $"{user.Mention}'s username";
                await this.RespondFailureAsync($"{ResponseEmoji.Failure} {usernameMention} contains invalid characters. {ResponseEmoji.FeelsDumbMan}").ConfigureAwait(false);
                return;
            }

            IEnumerable<IGuildChannel> existingChannels = await this._manager.GetBlogChannelsAsync(channelName, base.CancellationToken).ConfigureAwait(false);
            if (existingChannels.Any())
            {
                await this.RespondFailureAsync($"{ResponseEmoji.Failure} Channel {MentionUtils.MentionChannel(existingChannels.First().Id)} already exists {ResponseEmoji.FeelsBeanMan}");
                return;
            }

            IEnumerable<IGuildChannel> userChannels = await this._manager.FindUserBlogChannelsAsync(user.Id, base.CancellationToken).ConfigureAwait(false);
            if (userChannels.Any())
            {
                string responseStart = CreatingForSelf() ? "You already have" : $"{user.Mention} already has";
                string channelsMentions = string.Join(", ", userChannels.OrderBy(channel => channel.Name).Select(channel => MentionUtils.MentionChannel(channel.Id)));
                await this.RespondFailureAsync($"{ResponseEmoji.Failure} {responseStart} access to {channelsMentions} blog channel(s). {ResponseEmoji.BlobSweatAnimated}").ConfigureAwait(false);
                return;
            }


            // if admin creates the channel, they override member age check
            TimeSpan memberAge = DateTimeOffset.UtcNow - user.JoinedAt.Value;
            if (CreatingForSelf() && memberAge < this._options.MinMemberAge)
            {
                string[] emojis = new string[] { ResponseEmoji.FeelsBeanMan, ResponseEmoji.FeelsDumbMan, ResponseEmoji.EyesBlurry, ResponseEmoji.BlobSweatAnimated };
                await this.RespondFailureAsync($"{ResponseEmoji.Failure} You need to be here for at least {this._options.MinMemberAge.ToDisplayString()} to create a blog channel.\nYou've been here for {memberAge.ToDisplayString()} so far. {this._randomizer.GetRandomValue(emojis)}");
                return;
            }

            try
            {
                BlogChannelProperties props = new BlogChannelProperties()
                {
                    NSFW = nsfw
                };

                IGuildChannel result = await this._manager.CreateBlogChannel(channelName, user.Id, props, base.CancellationToken).ConfigureAwait(false);
                await base.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Content = null;
                    msg.Embed = new EmbedBuilder()
                        .WithAuthor(callerUser)
                        .WithTitle($"New Blog Channel!")
                        .WithDescription($"{ResponseEmoji.ParrotParty}{ResponseEmoji.ParrotParty}{ResponseEmoji.ParrotParty}{ResponseEmoji.ParrotParty}{ResponseEmoji.ParrotParty}")
                        .WithColor(user.GetUserColour())
                        .AddField("Member", user.Mention, inline: true)
                        .AddField("Channel", MentionUtils.MentionChannel(result.Id), inline: true)
                        .WithTimestamp(result.CreatedAt)
                        .WithFooter("DevSub Blogs yo!")
                        .WithThumbnailUrl(user.GetSafeAvatarUrl())
                        .Build();
                    msg.Components = new ComponentBuilder()
                        .WithButton("Go now!", null,
                            ButtonStyle.Link,
                            Emote.Parse(ResponseEmoji.EyesBlurry),
                            result.GetURL())
                        .Build();
                }, base.GetRequestOptions());
            }
            catch (HttpException ex) when (ex.IsMissingPermissions() && ex.LogAsError(this._log, "Exception when creating blog channel"))
            {
                await base.ModifyOriginalResponseAsync(msg => msg.Content = $"Oops! {ResponseEmoji.Failure}\nI lack permissions to create a channel! {ResponseEmoji.FeelsBeanMan}",
                    base.GetRequestOptions()).ConfigureAwait(false);
                return;
            }

            bool CreatingForSelf()
                => callerUser == user;
        }

        [SlashCommand("update", "Updates a blog channel for user")]
        public async Task CmdUpdateAsync(
            [Summary("Channel", "Which channel to update; not needed if you only have 1 blog")] IGuildChannel channel = null,
            [Summary("NSFW", "Should the channel be marked as NSFW?")] bool nsfw = false)
        {
            await base.DeferAsync(options: base.GetRequestOptions()).ConfigureAwait(false);

            IGuildUser callerUser = await base.Context.Guild.GetGuildUserAsync(base.Context.User.Id, base.CancellationToken).ConfigureAwait(false);
            bool isAdmin = callerUser.IsOwner() || callerUser.GuildPermissions.Administrator;
            IEnumerable<IGuildChannel> channels = isAdmin && channel != null
                ? await this._manager.GetBlogChannelsAsync(base.CancellationToken).ConfigureAwait(false)
                : await this._manager.FindUserBlogChannelsAsync(callerUser.Id, base.CancellationToken).ConfigureAwait(false);
            channels = channels.ToArray();

            if (channel == null)
            {
                if (channels.Count() > 1)
                {
                    await this.RespondFailureAsync("You can edit more than 1 channel - specify which one you want to edit.").ConfigureAwait(false);
                    return;
                }

                channel = channels.First();
            }
            else if (!channels.Any(c => c.Id == channel.Id))
            {
                await this.RespondFailureAsync($"You have no rights to edit this channel! {ResponseEmoji.BlobSweatAnimated}").ConfigureAwait(false);
                return;
            }

            try
            {
                BlogChannelProperties props = new BlogChannelProperties()
                {
                    NSFW = nsfw
                };

                await this._manager.EditBlogChannel(channel, props, base.CancellationToken).ConfigureAwait(false);
                await base.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Content = null;
                    msg.Embed = new EmbedBuilder()
                        .WithAuthor(callerUser)
                        .WithTitle($"Blog Channel updated!")
                        .WithDescription($"{ResponseEmoji.ParrotParty}{ResponseEmoji.ParrotParty}{ResponseEmoji.ParrotParty}{ResponseEmoji.ParrotParty}{ResponseEmoji.ParrotParty}")
                        .WithColor(callerUser.GetUserColour())
                        .AddField("Channel", MentionUtils.MentionChannel(channel.Id), inline: true)
                        .WithTimestamp(DateTime.UtcNow)
                        .WithFooter("DevSub Blogs yo!")
                        .Build();
                    msg.Components = new ComponentBuilder()
                        .WithButton("Go now!", null,
                            ButtonStyle.Link,
                            Emote.Parse(ResponseEmoji.EyesBlurry),
                            channel.GetURL())
                        .Build();
                }, base.GetRequestOptions());
            }
            catch (HttpException ex) when (ex.IsMissingPermissions() && ex.LogAsError(this._log, "Exception when updating blog channel"))
            {
                await base.ModifyOriginalResponseAsync(msg => msg.Content = $"Oops! {ResponseEmoji.Failure}\nI lack permissions to update the channel! {ResponseEmoji.FeelsBeanMan}",
                    base.GetRequestOptions()).ConfigureAwait(false);
                return;
            }
        }

        private Task RespondFailureAsync(string text)
            => base.ModifyOriginalResponseAsync(msg =>
            {
                msg.Content = text;
                msg.AllowedMentions = AllowedMentions.None;
            }, base.GetRequestOptions());
    }
}

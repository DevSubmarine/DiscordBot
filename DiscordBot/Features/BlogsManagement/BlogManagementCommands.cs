using Discord;
using Discord.Interactions;
using Discord.Net;

namespace DevSubmarine.DiscordBot.BlogsManagement.Services
{
    [Group("blog", "Commands for managing blog channels")]
    public class BlogManagementCommands : DevSubInteractionModule
    {
        private readonly IBlogChannelManager _manager;
        private readonly IBlogChannelNameConverter _nameConverter;
        private readonly BlogsManagementOptions _options;
        private readonly ILogger _log;

        public BlogManagementCommands(IBlogChannelManager manager, IBlogChannelNameConverter nameConverter, 
            IOptionsMonitor<BlogsManagementOptions> options, ILogger<BlogManagementCommands> log)
        {
            this._manager = manager;
            this._nameConverter = nameConverter;
            this._options = options.CurrentValue;
            this._log = log;
        }

        [SlashCommand("create", "Creates a blog channel for user")]
        public async Task CmdClearAsync(
            [Summary("User", "Which user to create channel for; can only be used by administrators")] IGuildUser user = null)
        {
            await base.DeferAsync(options: base.GetRequestOptions()).ConfigureAwait(false);

            // creating channel for other user should only be possible for admins
            IGuildUser callerUser = await base.Context.Guild.GetGuildUserAsync(base.Context.User.Id, base.Context.CancellationToken).ConfigureAwait(false);
            if (user != null)
            {
                if (!callerUser.IsOwner() && !callerUser.GuildPermissions.Administrator)
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
                await RespondFailureAsync($"{ResponseEmoji.Failure} {usernameMention} contains invalid characters. {ResponseEmoji.FeelsDumbMan}").ConfigureAwait(false);
                return;
            }

            IEnumerable<IGuildChannel> existingChannels = await this._manager.GetBlogChannelsAsync(channelName, base.Context.CancellationToken).ConfigureAwait(false);
            if (existingChannels.Any())
            {
                await RespondFailureAsync($"{ResponseEmoji.Failure} Channel {MentionUtils.MentionChannel(existingChannels.First().Id)} already exists {ResponseEmoji.FeelsBeanMan}");
                return;
            }

            IEnumerable<IGuildChannel> userChannels = await this._manager.FindUserBlogChannelsAsync(user.Id, base.Context.CancellationToken).ConfigureAwait(false);
            if (userChannels.Any())
            {
                string responseStart = CreatingForSelf() ? "You already have" : $"{user.Mention} already has";
                string channelsMentions = string.Join(", ", userChannels.OrderBy(channel => channel.Name).Select(channel => MentionUtils.MentionChannel(channel.Id)));
                await RespondFailureAsync($"{ResponseEmoji.Failure} {responseStart} access to {channelsMentions} blog channel(s). {ResponseEmoji.BlobSweatAnimated}").ConfigureAwait(false);
                return;
            }


            // if admin creates the channel, they override member age check
            TimeSpan memberAge = DateTimeOffset.UtcNow - user.JoinedAt.Value;
            if (CreatingForSelf() && memberAge < this._options.MinMemberAge)
            {
                string[] emojis = new string[] { ResponseEmoji.FeelsBeanMan, ResponseEmoji.FeelsDumbMan, ResponseEmoji.EyesBlurry, ResponseEmoji.BlobSweatAnimated };
                await RespondFailureAsync($"{ResponseEmoji.Failure} You need to be here for at least {this._options.MinMemberAge.ToDisplayString()} to create a blog channel.\nYou've been here for {memberAge.ToDisplayString()} so far. {emojis[new Random().Next(emojis.Length)]}");
                return;
            }

            try
            {
                IGuildChannel result = await this._manager.CreateBlogChannel(channelName, user.Id, base.Context.CancellationToken).ConfigureAwait(false);
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
                            $"https://discord.com/channels/{result.GuildId}/{result.Id}")
                        .Build();
                }, base.GetRequestOptions());
            }
            catch (HttpException ex) when (
                (ex.DiscordCode == DiscordErrorCode.MissingPermissions || ex.DiscordCode == DiscordErrorCode.InsufficientPermissions)
                && ex.LogAsError(this._log, "Exception when creating blog channel"))
            {
                await base.ModifyOriginalResponseAsync(msg => msg.Content = $"Oops! {ResponseEmoji.Failure}\nI lack permissions to create a channel! {ResponseEmoji.FeelsBeanMan}",
                    base.GetRequestOptions()).ConfigureAwait(false);
                return;
            }


            bool CreatingForSelf()
                => callerUser == user;
            Task RespondFailureAsync(string text)
                => base.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Content = text;
                    msg.AllowedMentions = AllowedMentions.None;
                }, base.GetRequestOptions());
        }
    }
}

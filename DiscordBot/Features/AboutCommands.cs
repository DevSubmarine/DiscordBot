using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Text;

namespace DevSubmarine.DiscordBot.Features
{
    public class AboutCommands : DevSubInteractionModule
    {
        private readonly DiscordSocketClient _client;

        private const string _repoURL = "https://github.com/DevSubmarine/DiscordBot";

        public AboutCommands(DiscordSocketClient client)
        {
            this._client = client;
        }

        [EnabledInDm(true)]
        [SlashCommand("about", "Info about the bot", ignoreGroupNames: true)]
        public async Task CmdAboutAsync()
        {
            IUser user = this._client.CurrentUser;
            IGuildUser guildUser = base.Context.Guild?.GetUser(user.Id);
            IUser authorUser = await this._client.GetUserAsync(247081094799687682, base.CancellationToken).ConfigureAwait(false);
            IGuildUser authorGuildUser = base.Context.Guild?.GetUser(authorUser.Id);

            string botName = guildUser?.Nickname ?? user.Username;
            string authorName = authorGuildUser?.Mention ?? user?.GetUsernameWithDiscriminator() ?? "TehGM";

            StringBuilder features = new StringBuilder();
            this.AddFeatureInfo(features, "Colour Roles", $"You can change the colour of your nickname using `/colour` commands! {ResponseEmoji.ParrotParty}");
            this.AddFeatureInfo(features, "Blog Channels", $"I'll automatically manage active and inactive blog channels. You can get your own using `/blog create` command, too! {ResponseEmoji.Zoop}");
            this.AddFeatureInfo(features, "DevSub Dictionary", $"DevSubmarine members are notorious for ~~bad spelling~~ inventing new amazing words. Find out more using `/dictionary` set of commands! {ResponseEmoji.PandaSip}");
            this.AddFeatureInfo(features, "Birthdays", $"As DevSub fam, we like to celebrate birthdays of others *(and especially our own ones)*. `/birthday` provides a way to keep track of everyone's birthday, do we can all can party! {ResponseEmoji.FastParrot}");
            this.AddFeatureInfo(features, "Voting", $"Want to vote kick someone, or check their reputation? Go ahead, `/vote` away! {ResponseEmoji.BlobSweatAnimated} \n*Disclaimer: these votes are just for jokes. No one's gonna get banned or kicked because of them.*");

            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle($"About {botName}")
                .WithDescription($"{botName} is a bot developed specifically for use by DevSubmarine community.\n\nIt is designed to automate some administrative tasks, as well as provide features based on our inside jokes.")
                .WithThumbnailUrl(user.GetSafeAvatarUrl())
                .WithColor(guildUser?.GetUserColour() ?? Color.Default)
                .WithUrl(_repoURL)
                .AddField("Features", features.ToString())
                .AddField("Source Code", "The bot is open source, so contribute away - find buttons below!", inline: true)
                .AddField("Author", $"Bot developed by {authorName}\nWith contributions from DevSubmarine community", inline: true)
                .WithFooter($"v{AppVersion.Version} • Copyright (c) 2022, DevSubmarine & TehGM", authorUser?.GetSafeAvatarUrl());
            ComponentBuilder components = new ComponentBuilder()
                .AddRow(new ActionRowBuilder()
                    .WithButton("Source Code", null, ButtonStyle.Link, null, _repoURL)
                    .WithButton("A bug? Oh no!", null, ButtonStyle.Link, null, $"{_repoURL}/issues")
                    .WithButton("Feature Requests", null, ButtonStyle.Link, null, $"{_repoURL}/discussions"));

            await base.RespondAsync(
                embed: embed.Build(),
                components: components.Build(),
                allowedMentions: AllowedMentions.None,
                options: base.GetRequestOptions())
                .ConfigureAwait(false);
        }

        private void AddFeatureInfo(StringBuilder builder, string name, string description)
        {
            if (builder.Length > 0)
                builder.Append("\n\n");
            builder.AppendFormat("__{0}__: {1}", name, description);
        }
    }
}

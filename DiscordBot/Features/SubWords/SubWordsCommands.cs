using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace DevSubmarine.DiscordBot.SubWords.Services
{
    [Group("subdictionary", "Commands for managing dictionary of dev sub invented words.")]
    public class SubWordsCommands : DevSubInteractionModule
    {
        private readonly ISubWordsService _subwords;
        private readonly DiscordSocketClient _client;

        public SubWordsCommands(ISubWordsService subwords, DiscordSocketClient client)
        {
            this._subwords = subwords;
            this._client = client;
        }

        [SlashCommand("add", "Adds a new word to DevSub Dictionary")]
        public async Task CmdAddAsync(
            [Summary("User", "User that said the silly word")] IUser user,
            [Summary("Word", "The word that they said")] string word)
        {
            SubWord result = new SubWord(word, user.Id, base.Context.User.Id);
            result.ChannelID = base.Context.Channel?.Id;
            result.GuildID = base.Context.Guild?.Id;
            result = await this._subwords.AddOrGetWordAsync(result, base.Context.CancellationToken).ConfigureAwait(false);

            Embed embed = await this.BuildWordEmbedAsync(result, base.Context.CancellationToken).ConfigureAwait(false); 

            await base.RespondAsync(embed).ConfigureAwait(false);
        }

        [SlashCommand("find", "Finds existing word by specific user in DevSub Dictionary")]
        public async Task CmdFindAsync(
            [Summary("User", "User that said the silly word")] IUser user,
            [Summary("Word", "The word to look for")] string word)
        {
            SubWord result = await this._subwords.GetSubWordAsync(word, user.Id, base.Context.CancellationToken).ConfigureAwait(false);
            if (result == null)
            {
                string username = await this.GetUserNameAsync(user.Id, base.Context.CancellationToken).ConfigureAwait(false);
                await base.RespondAsync($"{ResponseEmoji.SeriousThonk} Nope, word *`{word}`* by {username} not found.").ConfigureAwait(false);
            }
            else
            {
                Embed embed = await this.BuildWordEmbedAsync(result, base.Context.CancellationToken).ConfigureAwait(false);
                await base.RespondAsync(embed).ConfigureAwait(false);
            }
        }

        [SlashCommand("random", "Gets a random word DevSub Dictionary")]
        public async Task CmdRandomAsync(
            [Summary("User", "User that said the silly word, optional")] IUser user = null)
        {
            SubWord result = await this._subwords.GetRandomWordAsync(user?.Id, base.Context.CancellationToken).ConfigureAwait(false);
            string username = user != null
                ? await this.GetUserNameAsync(user.Id, base.Context.CancellationToken).ConfigureAwait(false)
                : null;

            if (result == null)
            {
                if (username == null)
                    await base.RespondAsync($"{ResponseEmoji.SeriousThonk} Seems no word has been added yet?").ConfigureAwait(false);
                else
                    await base.RespondAsync($"{ResponseEmoji.SeriousThonk} Seems no word by {username} has been added yet?").ConfigureAwait(false);
            }
            else
            {
                Embed embed = await this.BuildWordEmbedAsync(result, base.Context.CancellationToken).ConfigureAwait(false);
                RequestOptions options = new RequestOptions() { CancelToken = base.Context.CancellationToken };
                string message = username != null ? $"Random word by {username} from DevSub Dictionary:" : "Random word from DevSub Dictionary:";

                await base.RespondAsync(
                    message, embed: embed, 
                    allowedMentions: AllowedMentions.None, 
                    options: options).ConfigureAwait(false);
            }
        }

        [SlashCommand("list", "Lists all random words by specific user in DevSub Dictionary")]
        public async Task CmdListAsync(
            [Summary("User", "User that said the silly word")] IUser user)
        {
            await base.DeferAsync(options: new RequestOptions() { CancelToken = base.Context.CancellationToken }).ConfigureAwait(false);
            try
            {
                string result = await this._subwords.UploadWordsListAsync(user.Id, base.Context.CancellationToken).ConfigureAwait(false);
                string username = await this.GetUserNameAsync(user.Id, base.Context.CancellationToken).ConfigureAwait(false);
                RequestOptions options = new RequestOptions() { CancelToken = base.Context.CancellationToken };

                await base.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Content = $"All words by {username}:\n{ResponseEmoji.ParrotParty} {result} {ResponseEmoji.ParrotParty}";
                    msg.AllowedMentions = AllowedMentions.None;
                }, 
                options).ConfigureAwait(false);
            }
            catch
            {
                await base.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Content = $"{ResponseEmoji.FeelsBeanMan} I couldn't save the list to PasteMyst for some damn reason.";
                }, new RequestOptions() { CancelToken = base.Context.CancellationToken })
                    .ConfigureAwait(false);
            }
        }


        // UTILS
        private async Task<Embed> BuildWordEmbedAsync(SubWord word, CancellationToken cancellationToken = default)
        {
            if (word == null)
                throw new ArgumentNullException(nameof(word));

            IUser author = await this._client.GetUserAsync(word.AuthorID, cancellationToken).ConfigureAwait(false);
            string authorName = await this.GetUserNameAsync(author.Id, cancellationToken).ConfigureAwait(false);
            string addedByName = await this.GetUserNameAsync(word.AddedByUserID, cancellationToken).ConfigureAwait(false);
            string authorAvatarUrl = author.GetMaxAvatarUrl();

            EmbedBuilder result = new EmbedBuilder()
                .WithTitle(word.ToString())
                .AddField("Word By", $"{authorName}", true)
                .AddField("Added By", addedByName, true)
                .WithThumbnailUrl(authorAvatarUrl)
                .WithTimestamp(word.CreationTimeUTC)
                .WithFooter($"This amazing word was invented by {author.GetUsernameWithDiscriminator()}", authorAvatarUrl);
            if (word.GuildID != null && word.ChannelID != null && word.MessageID != null)
                result.WithUrl($"https://discord.com/channels/{word.GuildID}/{word.ChannelID}/{word.MessageID}");
            return result.Build();
        }

        public async Task<string> GetUserNameAsync(ulong id, CancellationToken cancellationToken = default)
        {
            IUser user = null;
            if (base.Context.Guild != null)
                user = await base.Context.Guild.GetGuildUserAsync(id, cancellationToken).ConfigureAwait(false);
            if (user != null)
                return MentionUtils.MentionUser(id);

            // if not a member, get as user and build string
            user = await this._client.GetUserAsync(id, cancellationToken).ConfigureAwait(false);
            return user.GetUsernameWithDiscriminator();
        }
    }
}

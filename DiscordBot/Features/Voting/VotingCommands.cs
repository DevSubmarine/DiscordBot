using Discord;
using Discord.Interactions;
using System.Text;

namespace DevSubmarine.DiscordBot.Voting.Services
{
    [Group("vote", "Vote to kick others... or something")]
    public class VotingCommands : DevSubInteractionModule
    {
        private readonly IVotingService _voting;
        private readonly IVotesStore _store;
        private readonly ILogger _log;

        public VotingCommands(IVotingService voting, IVotesStore store, ILogger<VotingCommands> log)
        {
            this._voting = voting;
            this._store = store;
            this._log = log;
        }

        [SlashCommand("kick", "Vote to kick someone")]
        public async Task CmdVoteKickAsync(
            [Summary("User", "User to vote kick")] IGuildUser user)
        {
            Vote vote = new Vote(VoteType.Kick, base.Context.User.Id, user.Id, base.Context.Interaction.CreatedAt);

            IVotingResult result = await this._voting.VoteAsync(vote, base.Context.CancellationToken).ConfigureAwait(false);
            if (result is CooldownVotingResult cooldown)
                await this.RespondCooldownAsync(cooldown.CooldownRemaining, user).ConfigureAwait(false);
            else
            {
                SuccessVotingResult voteResult = (SuccessVotingResult)result;
                await base.RespondAsync(
                    text: $"{user.Mention} you've been voted to be kicked! {ResponseEmoji.KekYu}",
                    embed: this.BuildResultEmbed(voteResult, user),
                    options: base.GetRequestOptions()).ConfigureAwait(false);
            }
        }

        [SlashCommand("ban", "Vote to ban someone")]
        public async Task CmdVoteBanAsync(
            [Summary("User", "User to vote ban")] IGuildUser user)
        {
            Vote vote = new Vote(VoteType.Ban, base.Context.User.Id, user.Id, base.Context.Interaction.CreatedAt);

            IVotingResult result = await this._voting.VoteAsync(vote, base.Context.CancellationToken).ConfigureAwait(false);
            if (result is CooldownVotingResult cooldown)
                await this.RespondCooldownAsync(cooldown.CooldownRemaining, user).ConfigureAwait(false);
            else
            {
                SuccessVotingResult voteResult = (SuccessVotingResult)result;
                await base.RespondAsync(
                    text: $"{user.Mention} you've been voted to be banned! {ResponseEmoji.KekPoint}",
                    embed: this.BuildResultEmbed(voteResult, user),
                    options: base.GetRequestOptions()).ConfigureAwait(false);
            }
        }

        [SlashCommand("mod", "Vote to mod someone")]
        public async Task CmdVoteModAsync(
            [Summary("User", "User to vote mod")] IGuildUser user)
        {
            Vote vote = new Vote(VoteType.Mod, base.Context.User.Id, user.Id, base.Context.Interaction.CreatedAt);

            IVotingResult result = await this._voting.VoteAsync(vote, base.Context.CancellationToken).ConfigureAwait(false);
            if (result is CooldownVotingResult cooldown)
                await this.RespondCooldownAsync(cooldown.CooldownRemaining, user).ConfigureAwait(false);
            else
            {
                SuccessVotingResult voteResult = (SuccessVotingResult)result;
                await base.RespondAsync(
                    text: $"{user.Mention} you've been voted to be modded! {ResponseEmoji.EyesBlurry}",
                    embed: this.BuildResultEmbed(voteResult, user),
                    options: base.GetRequestOptions()).ConfigureAwait(false);
            }
        }

        private Task RespondCooldownAsync(TimeSpan cooldown, IGuildUser user)
            => base.RespondAsync(
                text: $"You need to wait {cooldown.ToDisplayString()} more to vote against {user.Mention}. {ResponseEmoji.FeelsDumbMan}",
                allowedMentions: AllowedMentions.None,
                options: base.GetRequestOptions());

        private Embed BuildResultEmbed(SuccessVotingResult vote, IGuildUser target)
        {
            return new EmbedBuilder()
                .WithTitle($"Voted to {vote.CreatedVote.Type.GetText()} {target.Nickname}")
                .WithThumbnailUrl(target.GetSafeAvatarUrl())
                .WithAuthor(base.Context.User)
                .AddField($"Votes by {base.Context.User.Username}", vote.VotesAgainstTarget.ToString())
                .AddField($"Total votes", vote.TotalVotesAgainstTarget.ToString())
                .WithTimestamp(vote.CreatedVote.Timestamp)
                .WithColor(target.GetUserColour())
                .Build();
        }

        [Group("statistics", "Check various voting statistics")]
        public class VotingStatisticsCommands : DevSubInteractionModule
        {
            private readonly IVotesStore _store;

            public VotingStatisticsCommands(IVotingService voting, IVotesStore store, ILogger<VotingCommands> log)
            {
                this._store = store;
            }

            [SlashCommand("search", "Query for statistics using specified search criteria")]
            public async Task CmdFindAsync(
                [Summary("Target", "User the vote was sent against")] IUser target = null,
                [Summary("Voter", "User that sent the vote")] IUser voter = null,
                [Summary("VoteType", "Type of the vote")] VoteType? voteType = null)
            {
                await base.DeferAsync(options: base.GetRequestOptions()).ConfigureAwait(false);
                IEnumerable<Vote> results = await this._store.GetVotesAsync(target?.Id, voter?.Id, voteType, base.Context.CancellationToken).ConfigureAwait(false);

                EmbedBuilder embed = new EmbedBuilder()
                    .WithTitle($"Found {results.LongCount()} votes")
                    .AddField("Search Criteria", this.BuildCriteriaString(target, voter, voteType));


                if (!results.Any())
                {
                    await base.ModifyOriginalResponseAsync(msg =>
                    {
                        msg.Embed = embed
                            .WithDescription($"No votes matching your criteria found. {ResponseEmoji.FeelsBeanMan}")
                            .Build();
                        msg.AllowedMentions = AllowedMentions.None;
                    }, base.GetRequestOptions()).ConfigureAwait(false);
                    return;
                }


                if (target == null)
                {
                    IEnumerable<IGrouping<ulong, Vote>> top = GetTop(vote => vote.TargetID);
                    embed.AddField("Top Targets",
                        string.Join('\n', top.Select(value => $"{MentionUtils.MentionUser(value.Key)}: {value.LongCount()} times")));
                }
                if (voter == null)
                {
                    IEnumerable<IGrouping<ulong, Vote>> top = GetTop(vote => vote.VoterID);
                    embed.AddField("Top Voters",
                        string.Join('\n', top.Select(value => $"{MentionUtils.MentionUser(value.Key)}: {value.LongCount()} times")));
                }
                if (voteType == null)
                {
                    IEnumerable<IGrouping<VoteType, Vote>> top = GetTop(vote => vote.Type);
                    embed.AddField("Top Vote Types",
                        string.Join('\n', top.Select(value => $"{value.Key.GetText()}: {value.LongCount()} times")));
                }

                const int lastCount = 5;
                IEnumerable<Vote> lastVotes = results
                    .OrderByDescending(vote => vote.Timestamp)
                    .Take(lastCount);
                embed.AddField($"Last {Math.Min(lastCount, lastVotes.Count())} Votes",
                    string.Join('\n', lastVotes.Select(vote 
                        => $"{MentionUtils.MentionUser(vote.VoterID)} voted to {vote.Type.GetText()} {MentionUtils.MentionUser(vote.TargetID)} {TimestampTag.FromDateTimeOffset(vote.Timestamp, TimestampTagStyles.Relative)}")));

                await base.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embed = embed.Build();
                    msg.AllowedMentions = AllowedMentions.None;
                }, 
                    base.GetRequestOptions()).ConfigureAwait(false);

                IEnumerable<IGrouping<TKey, Vote>> GetTop<TKey>(Func<Vote, TKey> keySelector, int count = 3)
                {
                    return results
                        .GroupBy(keySelector)
                        .OrderByDescending(grouping => grouping.LongCount())
                        .Take(count);
                }
            }

            private string BuildCriteriaString(IUser target, IUser voter, VoteType? voteType)
            {
                const string any = "`any`";
                StringBuilder builder = new StringBuilder();
                builder.AppendFormat("User: {0}\n", target?.Mention ?? any);
                builder.AppendFormat("Voter: {0}\n", voter?.Mention ?? any);
                builder.AppendFormat("Vote Type: {0}\n", voteType?.GetText() ?? any);
                return builder.ToString();
            }
        }
    }
}

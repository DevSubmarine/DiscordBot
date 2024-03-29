﻿using DevSubmarine.DiscordBot.Settings;
using Discord;
using Discord.Interactions;
using System.Text;

namespace DevSubmarine.DiscordBot.Voting.Commands
{
    [Group("vote", "Vote to kick others... or something")]
    [EnabledInDm(false)]
    public class VotingCommands : DevSubInteractionModule
    {
        private readonly IVotingService _voting;
        private readonly IUserSettingsProvider _userSettings;

        public VotingCommands(IVotingService voting, IUserSettingsProvider userSettings)
        {
            this._voting = voting;
            this._userSettings = userSettings;
        }

        [SlashCommand("kick", "Vote to kick someone")]
        [EnabledInDm(false)]
        public async Task CmdVoteKickAsync(
            [Summary("User", "User to vote kick")] IGuildUser user)
        {
            Vote vote = new Vote(VoteType.Kick, base.Context.User.Id, user.Id, base.Context.Interaction.CreatedAt);

            IVotingResult result = await this._voting.VoteAsync(vote, base.CancellationToken).ConfigureAwait(false);
            if (result is CooldownVotingResult cooldown)
                await this.RespondCooldownAsync(cooldown.CooldownRemaining, user).ConfigureAwait(false);
            else
                await this.RespondConfirmAsync(result, $"{user.Mention} you've been voted to be kicked! {ResponseEmoji.KekYu}");
        }

        [SlashCommand("ban", "Vote to ban someone")]
        [EnabledInDm(false)]
        public async Task CmdVoteBanAsync(
            [Summary("User", "User to vote ban")] IGuildUser user)
        {
            Vote vote = new Vote(VoteType.Ban, base.Context.User.Id, user.Id, base.Context.Interaction.CreatedAt);

            IVotingResult result = await this._voting.VoteAsync(vote, base.CancellationToken).ConfigureAwait(false);
            if (result is CooldownVotingResult cooldown)
                await this.RespondCooldownAsync(cooldown.CooldownRemaining, user).ConfigureAwait(false);
            else
                await this.RespondConfirmAsync(result, $"{user.Mention} you've been voted to be banned! {ResponseEmoji.KekPoint}");
        }

        [SlashCommand("mod", "Vote to mod someone")]
        [EnabledInDm(false)]
        public async Task CmdVoteModAsync(
            [Summary("User", "User to vote mod")] IGuildUser user)
        {
            Vote vote = new Vote(VoteType.Mod, base.Context.User.Id, user.Id, base.Context.Interaction.CreatedAt);

            if (vote.VoterID == vote.TargetID)
            {
                await base.RespondAsync(
                    text: $"You cannot vote to mod yourself, dumbo. {ResponseEmoji.FeelsDumbMan}",
                    options: base.GetRequestOptions()).ConfigureAwait(false);
                return;
            }

            IVotingResult result = await this._voting.VoteAsync(vote, base.CancellationToken).ConfigureAwait(false);
            if (result is CooldownVotingResult cooldown)
                await this.RespondCooldownAsync(cooldown.CooldownRemaining, user).ConfigureAwait(false);
            else
                await this.RespondConfirmAsync(result, $"{user.Mention} you've been voted to be modded! {ResponseEmoji.EyesBlurry}");
        }

        private async Task RespondConfirmAsync(IVotingResult result, string message)
        {
            SuccessVotingResult voteResult = (SuccessVotingResult)result;
            const string settingsCmd = "`/user-settings vote-ping`";

            IGuildUser user = await base.Context.Guild.GetGuildUserAsync(voteResult.CreatedVote.TargetID, base.CancellationToken).ConfigureAwait(false);
            UserSettings settings = await this._userSettings.GetUserSettingsAsync(user.Id, base.CancellationToken).ConfigureAwait(false);
            AllowedMentions mentions = settings.PingOnVote ? new AllowedMentions(AllowedMentionTypes.Users) : AllowedMentions.None;

            Embed embed = this.BuildResultEmbed(voteResult, user);
            if (settings.PingOnVote)
                message += $"\n*You can disable pings on vote by using {settingsCmd} command.*";

            await base.RespondAsync(
                text: message,
                embed: embed,
                options: base.GetRequestOptions(),
                allowedMentions: mentions)
                .ConfigureAwait(false);
        }

        private Task RespondCooldownAsync(TimeSpan cooldown, IGuildUser user)
            => base.RespondAsync(
                text: $"You need to wait {cooldown.ToDisplayString()} more to vote against {user.Mention}. {ResponseEmoji.FeelsDumbMan}",
                allowedMentions: AllowedMentions.None,
                options: base.GetRequestOptions());

        private Embed BuildResultEmbed(SuccessVotingResult vote, IGuildUser target)
        {
            string voterName = base.Context.Guild.GetUser(Context.User.Id)?.Nickname ?? Context.User.Username;
            string targetName = target.Nickname ?? target.Username;
            return new EmbedBuilder()
                .WithTitle($"Voted to {vote.CreatedVote.Type.GetText()} {targetName}")
                .WithThumbnailUrl(target.GetSafeAvatarUrl())
                .AddField($"Total votes", vote.TotalVotesAgainstTarget.ToString())
                .WithTimestamp(vote.CreatedVote.Timestamp)
                .WithFooter($"By {voterName} for the {vote.VotesAgainstTarget.GetOrdinalString()} time", Context.User.GetSafeAvatarUrl())
                .WithColor(target.GetUserColour())
                .Build();
        }

        [Group("statistics", "Check various voting statistics")]
        public class VotingStatisticsCommands : DevSubInteractionModule
        {
            private readonly IVotesStore _store;
            private readonly IVotingAlignmentCalculator _alignment;

            public VotingStatisticsCommands(IVotesStore store, IVotingAlignmentCalculator alignment)
            {
                this._store = store;
                this._alignment = alignment;
            }

            [SlashCommand("check", "Check voting statistics for specific user")]
            [EnabledInDm(false)]
            public async Task CmdCheckAsync(
                [Summary("User", "User to check statistics for")] IGuildUser user = null)
            {
                await base.DeferAsync(options: base.GetRequestOptions()).ConfigureAwait(false);

                if (user == null)
                    user = await base.Context.Guild.GetGuildUserAsync(base.Context.User.Id, base.CancellationToken);

                Task<IEnumerable<Vote>> votesTargetTask = this._store.GetVotesAsync(user.Id, null, null, base.CancellationToken);
                Task<IEnumerable<Vote>> votesVoterTask = this._store.GetVotesAsync(null, user.Id, null, base.CancellationToken);
                await Task.WhenAll(votesTargetTask, votesVoterTask).ConfigureAwait(false);

                IEnumerable<Vote> votesTarget = votesTargetTask.Result;
                IEnumerable<Vote> votesVoter = votesVoterTask.Result;
                IEnumerable<Vote> votesAll = votesTarget.Union(votesVoter);

                EmbedBuilder embed = new EmbedBuilder()
                    .WithTitle($"Voting stats for {user.GetUsernameWithDiscriminator()}")
                    .WithColor(user.GetUserColour())
                    .WithAuthor(user);

                VotingAlignment voterAlignment = null;
                VotingAlignment targetAlignment = null;
                VotingAlignment totalAlignment = null;

                StringBuilder builder = new StringBuilder();
                if (votesVoter.Any())
                {
                    builder.Clear();
                    builder.AppendFormat("Votes made: `{0}`", votesVoter.LongCount());
                    builder.Append('\n');

                    IEnumerable<Vote> votesMod = votesVoter.Where(vote => vote.IsPositive);
                    IEnumerable<Vote> votesKickOrBan = votesVoter.Where(vote => vote.IsNegative);
                    voterAlignment = this._alignment.CalculateAlignment(votesMod, votesKickOrBan);

                    if (votesMod.Any())
                    {
                        IGrouping<ulong, Vote> topTarget = this.GetTop(votesMod, vote => vote.TargetID, 1).First();
                        builder.Append($"*{MentionUtils.MentionUser(topTarget.Key)} is {user.Mention}'s crush - `{topTarget.LongCount()}` votes for {VoteType.Mod.GetText()}! {ResponseEmoji.BlobHug}*\n");
                    }
                    if (votesKickOrBan.Any())
                    {
                        IGrouping<ulong, Vote> topTarget = this.GetTop(votesKickOrBan, vote => vote.TargetID, 1).First();
                        builder.Append($"*{user.Mention} is on a crusade against {MentionUtils.MentionUser(topTarget.Key)} - `{topTarget.LongCount()}` votes for {VoteType.Kick.GetText()} or {VoteType.Ban.GetText()}. {ResponseEmoji.JerryWhat}*\n");
                    }

                    if (builder.Length > 0)
                        builder.Append('\n');
                    builder.AppendFormat("Top votes made:\n{0}", this.BuildTopTargetsString(votesVoter));
                    embed.AddField($"As a Voter", builder.ToString());
                }
                else
                    embed.AddField("As a Voter", $"{user.Mention} didn't vote for anyone yet... what a boomer. {ResponseEmoji.FeelsDumbMan}");

                if (votesTarget.Any())
                {
                    builder.Clear();
                    builder.AppendFormat("Votes received: `{0}`", votesTarget.LongCount());
                    builder.Append('\n');

                    IEnumerable<Vote> votesMod = votesTarget.Where(vote => vote.IsPositive);
                    IEnumerable<Vote> votesKickOrBan = votesTarget.Where(vote => vote.IsNegative);
                    targetAlignment = this._alignment.CalculateAlignment(votesMod, votesKickOrBan);

                    if (votesMod.Any())
                    {
                        IGrouping<ulong, Vote> topVoter = this.GetTop(votesMod, vote => vote.VoterID, 1).First();
                        builder.Append($"*{user.Mention} seems to be loved by {MentionUtils.MentionUser(topVoter.Key)} - `{topVoter.LongCount()}` votes for {VoteType.Mod.GetText()}! {ResponseEmoji.BlobHearts}*\n");
                    }
                    if (votesKickOrBan.Any())
                    {
                        IGrouping<ulong, Vote> topVoter = this.GetTop(votesKickOrBan, vote => vote.VoterID, 1).First();
                        builder.Append($"*{MentionUtils.MentionUser(topVoter.Key)} is harassing {user.Mention} - `{topVoter.LongCount()}` votes for {VoteType.Kick.GetText()} or {VoteType.Ban.GetText()}. {ResponseEmoji.Reeeeee}*\n");
                    }

                    if (builder.Length > 0)
                        builder.Append('\n');
                    builder.AppendFormat("Top votes received:\n{0}", this.BuildTopVotersString(votesTarget));
                    embed.AddField("As a Survivor", builder.ToString());
                }
                else
                    embed.AddField("As a Survivor", $"{user.Mention} hasn't been voted on yet? Huh?! {ResponseEmoji.FeelsBeanMan}");


                if (votesAll.Any())
                {
                    embed.AddField("Voter Rep",
                        voterAlignment != null ? VotingAlignment.FormatScore(voterAlignment.Score) : "N/A",
                        inline: true);
                    embed.AddField("Survivor Rep",
                        targetAlignment != null ? VotingAlignment.FormatScore(targetAlignment.Score) : "N/A",
                        inline: true);

                    totalAlignment = this._alignment.CalculateAlignment(votesAll);
                    embed.AddField("Alignment", totalAlignment.ToString(), inline: true);
                    embed.WithThumbnailUrl(totalAlignment.ImageURL);

                    embed.AddField("Last Votes", this.BuildLastVotesString(votesAll, 10));
                }

                await base.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embed = embed.Build();
                    msg.AllowedMentions = AllowedMentions.None;
                },
                    base.GetRequestOptions()).ConfigureAwait(false);
            }

            [SlashCommand("search", "Query for statistics using specified search criteria")]
            [EnabledInDm(true)]
            public async Task CmdFindAsync(
                [Summary("Target", "User the vote was sent against")] IUser target = null,
                [Summary("Voter", "User that sent the vote")] IUser voter = null,
                [Summary("VoteType", "Type of the vote")] VoteType? voteType = null)
            {
                await base.DeferAsync(options: base.GetRequestOptions()).ConfigureAwait(false);
                IEnumerable<Vote> results = await this._store.GetVotesAsync(target?.Id, voter?.Id, voteType, base.CancellationToken).ConfigureAwait(false);

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
                    embed.AddField("Top Targets", this.BuildTopTargetsString(results));
                if (voter == null)
                    embed.AddField("Top Voters", this.BuildTopVotersString(results));
                if (voteType == null)
                    embed.AddField("Top Vote Types", this.BuildTopTypesString(results));

                embed.AddField($"Last Votes", this.BuildLastVotesString(results));


                await base.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Embed = embed.Build();
                    msg.AllowedMentions = AllowedMentions.None;
                },
                    base.GetRequestOptions()).ConfigureAwait(false);
            }

            private IEnumerable<IGrouping<TKey, Vote>> GetTop<TKey>(IEnumerable<Vote> votes, Func<Vote, TKey> keySelector, int count)
            {
                return votes
                    .GroupBy(keySelector)
                    .OrderByDescending(grouping => grouping.LongCount())
                    .Take(count);
            }

            private string BuildTopTargetsString(IEnumerable<Vote> votes, int count = 3)
            {
                IEnumerable<IGrouping<ulong, Vote>> top = this.GetTop(votes, vote => vote.TargetID, count);
                return string.Join('\n', top.Select(value => $"{MentionUtils.MentionUser(value.Key)}: {value.LongCount()} times"));
            }

            private string BuildTopVotersString(IEnumerable<Vote> votes, int count = 3)
            {
                IEnumerable<IGrouping<ulong, Vote>> top = this.GetTop(votes, vote => vote.VoterID, count);
                return string.Join('\n', top.Select(value => $"{MentionUtils.MentionUser(value.Key)}: {value.LongCount()} times"));
            }

            private string BuildTopTypesString(IEnumerable<Vote> votes, int count = 3)
            {
                IEnumerable<IGrouping<VoteType, Vote>> top = this.GetTop(votes, vote => vote.Type, count);
                return string.Join('\n', top.Select(value => $"{value.Key.GetText()}: {value.LongCount()} times"));
            }

            private string BuildLastVotesString(IEnumerable<Vote> votes, int count = 5)
            {
                IEnumerable<Vote> lastVotes = votes
                    .OrderByDescending(vote => vote.Timestamp)
                    .Take(count);
                return string.Join('\n', lastVotes.Select(vote
                        => $"{MentionUtils.MentionUser(vote.VoterID)} voted to {vote.Type.GetText()} {MentionUtils.MentionUser(vote.TargetID)} {TimestampTag.FromDateTimeOffset(vote.Timestamp, TimestampTagStyles.Relative)}"));
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

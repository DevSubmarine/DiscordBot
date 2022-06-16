using Discord;
using Discord.Interactions;

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
    }
}

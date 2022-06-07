using Discord;
using Discord.Interactions;

namespace DevSubmarine.DiscordBot
{
    public class DevSubInteractionModule<TContext> : InteractionModuleBase<TContext> where TContext : class, IInteractionContext
    {
        public Task RespondAsync(string text, Embed embed, CancellationToken cancellationToken)
            => base.RespondAsync(text: text, embed: embed, options: this.GetRequestOptions(cancellationToken));
        public Task RespondAsync(string text, CancellationToken cancellationToken)
            => this.RespondAsync(text, null, cancellationToken);
        public Task RespondAsync(Embed embed, CancellationToken cancellationToken)
            => this.RespondAsync(null, embed, cancellationToken);

        protected RequestOptions GetRequestOptions(CancellationToken cancellationToken)
            => new RequestOptions() { CancelToken = cancellationToken };
    }

    public class DevSubInteractionModule : DevSubInteractionModule<DevSubInteractionContext>
    {
        public Task RespondAsync(string text, Embed embed)
            => base.RespondAsync(text, embed, base.Context.CancellationToken);
        public Task RespondAsync(string text)
            => base.RespondAsync(text, base.Context.CancellationToken);
        public Task RespondAsync(Embed embed)
            => base.RespondAsync(embed, base.Context.CancellationToken);
    }
}

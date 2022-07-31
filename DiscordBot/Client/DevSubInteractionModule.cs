using Discord;
using Discord.Interactions;

namespace DevSubmarine.DiscordBot
{
    #pragma warning disable DS0101 // Slash commands class doesn't inherit from DevSubInteractionModule
    public class DevSubInteractionModule<TContext> : InteractionModuleBase<TContext> where TContext : class, IInteractionContext
    {
        protected Task RespondAsync(string text, Embed embed, CancellationToken cancellationToken)
            => base.RespondAsync(text: text, embed: embed, options: this.GetRequestOptions(cancellationToken));
        protected Task RespondAsync(string text, CancellationToken cancellationToken)
            => this.RespondAsync(text, null, cancellationToken);
        protected Task RespondAsync(Embed embed, CancellationToken cancellationToken)
            => this.RespondAsync(null, embed, cancellationToken);

        protected RequestOptions GetRequestOptions(CancellationToken cancellationToken)
            => cancellationToken.ToRequestOptions();
    }

    public class DevSubInteractionModule : DevSubInteractionModule<DevSubInteractionContext>
    {
        public CancellationToken CancellationToken => base.Context.CancellationToken;

        public Task RespondAsync(string text, Embed embed)
            => base.RespondAsync(text, embed, this.CancellationToken);
        public Task RespondAsync(string text)
            => base.RespondAsync(text, this.CancellationToken);
        public Task RespondAsync(Embed embed)
            => base.RespondAsync(embed, this.CancellationToken);

        protected RequestOptions GetRequestOptions()
            => this.GetRequestOptions(this.CancellationToken);
    }
    #pragma warning restore DS0101 // Slash commands class doesn't inherit from DevSubInteractionModule
}

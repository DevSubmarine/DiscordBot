using Discord.Interactions;
using Discord.WebSocket;

namespace DevSubmarine.DiscordBot
{
    // not generic for now cause YAGNI
    public class DevSubInteractionContext : SocketInteractionContext<SocketInteraction>
    {
        public CancellationToken CancellationToken { get; }
        public DevSubInteractionContext(DiscordSocketClient client, SocketInteraction interaction, CancellationToken cancellationToken)
             : base(client, interaction)
        {
            this.CancellationToken = cancellationToken;
        }
    }
}

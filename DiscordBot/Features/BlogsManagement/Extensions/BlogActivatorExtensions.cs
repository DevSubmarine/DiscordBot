using Discord;

namespace DevSubmarine.DiscordBot.BlogsManagement
{
    public static class BlogActivatorExtensions
    {
        /// <summary>Sets channel state to active.</summary>
        /// <remarks>This might be a no-op if the channel is already active.</remarks>
        /// <param name="activator">The service instance.</param>
        /// <param name="channelID">Channel to change.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <exception cref="ArgumentException">Provided channel ID is not valid.</exception>
        public static Task ActivateBlogChannel(this IBlogChannelActivator activator, IGuildChannel channel, CancellationToken cancellationToken)
            => activator.ActivateBlogChannel(channel.Id, cancellationToken);
        /// <summary>Sets channel state to inactive.</summary>
        /// <remarks>This might be a no-op if the channel is already inactive.</remarks>
        /// <param name="activator">The service instance.</param>
        /// <param name="channelID">Channel to change.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <exception cref="ArgumentException">Provided channel ID is not valid.</exception>
        public static Task DeactivateBlogChannel(this IBlogChannelActivator activator, IGuildChannel channel, CancellationToken cancellationToken)
            => activator.DeactivateBlogChannel(channel.Id, cancellationToken);
    }
}

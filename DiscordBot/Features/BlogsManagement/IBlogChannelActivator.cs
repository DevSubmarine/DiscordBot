namespace DevSubmarine.DiscordBot.BlogsManagement
{
    /// <summary>Service responsible for changing blog channel state between active and inactive.</summary>
    public interface IBlogChannelActivator
    {
        /// <summary>Sets channel state to active.</summary>
        /// <remarks>This might be a no-op if the channel is already active.</remarks>
        /// <param name="channelID">ID of channel to change.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <exception cref="ArgumentException">Provided channel ID is not valid.</exception>
        Task ActivateBlogChannel(ulong channelID, CancellationToken cancellationToken = default);
        /// <summary>Sets channel state to inactive.</summary>
        /// <remarks>This might be a no-op if the channel is already inactive.</remarks>
        /// <param name="channelID">ID of channel to change.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <exception cref="ArgumentException">Provided channel ID is not valid.</exception>
        Task DeactivateBlogChannel(ulong channelID, CancellationToken cancellationToken = default);
    }
}

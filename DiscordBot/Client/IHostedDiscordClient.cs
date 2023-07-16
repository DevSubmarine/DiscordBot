using Discord;

namespace DevSubmarine.DiscordBot
{
    public interface IHostedDiscordClient
    {
        IDiscordClient Client { get; }

        Task WaitForConnectionAsync(CancellationToken cancellationToken = default);

        Task StartClientAsync();
        Task StopClientAsync();
    }
}

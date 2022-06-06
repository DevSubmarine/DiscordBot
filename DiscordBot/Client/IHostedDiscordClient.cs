using Discord;

namespace DevSubmarine.DiscordBot
{
    public interface IHostedDiscordClient
    {
        IDiscordClient Client { get; }

        Task StartClientAsync();
        Task StopClientAsync();
    }
}

using Discord;

namespace DevSubmarine.DiscordBot.BlogsManagement
{
    public static class BlogActivatorExtensions
    {
        public static Task ActivateBlogChannel(this IBlogChannelActivator activator, IGuildChannel channel, CancellationToken cancellationToken)
            => activator.ActivateBlogChannel(channel.Id, cancellationToken);
        public static Task DeactivateBlogChannel(this IBlogChannelActivator activator, IGuildChannel channel, CancellationToken cancellationToken)
            => activator.DeactivateBlogChannel(channel.Id, cancellationToken);
    }
}

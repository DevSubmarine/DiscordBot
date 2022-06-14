using Discord;

namespace DevSubmarine.DiscordBot.BlogsManagement
{
    public static class BlogActivatorExtensions
    {
        public static Task ActivateBlogChannel(this IBlogActivator activator, IGuildChannel channel, CancellationToken cancellationToken)
            => activator.ActivateBlogChannel(channel.Id, cancellationToken);
        public static Task DeactivateBlogChannel(this IBlogActivator activator, IGuildChannel channel, CancellationToken cancellationToken)
            => activator.DeactivateBlogChannel(channel.Id, cancellationToken);
    }
}

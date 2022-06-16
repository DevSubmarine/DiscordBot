using Discord;

namespace DevSubmarine.DiscordBot
{
    public static class CancellationTokenExtensions
    {
        public static RequestOptions ToRequestOptions(this CancellationToken cancellationToken, Action<RequestOptions> configure)
        {
            RequestOptions result = ToRequestOptions(cancellationToken);
            configure(result);
            return result;
        }

        public static RequestOptions ToRequestOptions(this CancellationToken cancellationToken)
            => new RequestOptions() { CancelToken = cancellationToken };
    }
}

using DevSubmarine.DiscordBot.RandomStatus.Placeholders;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace DevSubmarine.DiscordBot.RandomStatus.Services
{
    internal class StatusPlaceholderEngine : IStatusPlaceholderEngine
    {
        private readonly IReadOnlyDictionary<StatusPlaceholderAttribute, Type> _placeholders;
        private readonly IServiceProvider _services;
        private readonly ILogger _log;

        public StatusPlaceholderEngine(IServiceProvider services, ILogger<StatusPlaceholderEngine> log)
        {
            this._services = services;
            this._log = log;

            this._placeholders = this.LoadPlaceholders();
        }

        private IReadOnlyDictionary<StatusPlaceholderAttribute, Type> LoadPlaceholders()
        {
            this._log.LogDebug("Loading all placeholder definitions");

            Assembly asm = this.GetType().Assembly;
            IEnumerable<Type> types = asm.GetTypes()
                .Where(t =>
                    !t.IsAbstract &&
                    !typeof(IStatusPlaceholder).IsAssignableFrom(t) &&
                    !Attribute.IsDefined(t, typeof(CompilerGeneratedAttribute)) &&
                    Attribute.IsDefined(t, typeof(StatusPlaceholderAttribute), true));

            this._log.LogDebug("Found {Count} placeholder definitions", types.Count());
            return types.ToDictionary(t => t.GetCustomAttribute<StatusPlaceholderAttribute>());
        }

        // this method is designed to support placeholders that can take args in form of regex groups 
        // placeholder instance will only be created if needed, and will always be transient
        // almost a full blown placeholder engine lol. Shout over-engineered, I dare you!
        // (I know, it actually is, but hey - likely will rarely need to be touched, just create new placeholders away!)
        public async Task<string> ConvertPlaceholdersAsync(string status, CancellationToken cancellationToken = default)
        {
            this._log.LogDebug("Running placeholders engine for status {Status}", status);

            using IServiceScope services = this._services.CreateScope();
            StringBuilder builder = new StringBuilder(status);
            foreach (KeyValuePair<StatusPlaceholderAttribute, Type> placeholderInfo in this._placeholders)
            {
                IEnumerable<Match> matches = placeholderInfo.Key.PlaceholderRegex
                    .Matches(status)
                    .Where(m => m != null && m.Success);

                if (!matches.Any())
                    continue;

                IStatusPlaceholder placeholder = (IStatusPlaceholder)ActivatorUtilities.CreateInstance(services.ServiceProvider, placeholderInfo.Value);

                foreach (Match match in matches.OrderByDescending(m => m.Index))
                {
                    string replacement = await placeholder.GetReplacementAsync(match, cancellationToken).ConfigureAwait(false);
                    builder.Remove(match.Index, match.Length);
                    builder.Insert(match.Index, replacement);
                }
            }
            return builder.ToString();
        }
    }
}

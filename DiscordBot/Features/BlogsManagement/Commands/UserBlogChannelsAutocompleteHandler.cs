using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace DevSubmarine.DiscordBot.BlogsManagement.Commands
{
    public class UserBlogChannelsAutocompleteHandler : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            IBlogChannelManager manager = services.GetRequiredService<IBlogChannelManager>();
            DevSubInteractionContext ctx = (DevSubInteractionContext)context;

            IGuildUser callerUser = await ctx.Guild.GetGuildUserAsync(ctx.User.Id, ctx.CancellationToken).ConfigureAwait(false);
            IEnumerable<IGuildChannel> channels = callerUser.IsOwner() || callerUser.GuildPermissions.Administrator
                ? await manager.GetBlogChannelsAsync(ctx.CancellationToken).ConfigureAwait(false)
                : await manager.FindUserBlogChannelsAsync(callerUser.Id, ctx.CancellationToken).ConfigureAwait(false);

            string input = autocompleteInteraction.Data.Current.Value?.ToString();
            if (!string.IsNullOrEmpty(input))
                channels = channels.Where(c => c.Name.Contains(input, StringComparison.OrdinalIgnoreCase));

            AutocompletionResult result = AutocompletionResult.FromSuccess(channels
                .OrderBy(c => c.Name)
                .Select(c => new AutocompleteResult($"#{c.Name}", c.Id.ToString()))
                .Take(25));
            return result;
        }
    }
}

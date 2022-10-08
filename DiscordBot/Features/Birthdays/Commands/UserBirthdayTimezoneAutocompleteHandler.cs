using DevSubmarine.DiscordBot.Time;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace DevSubmarine.DiscordBot.Birthdays.Commands
{
    public class UserBirthdayTimezoneAutocompleteHandler : AutocompleteHandler
    {
        public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            ITimezoneProvider provider = services.GetRequiredService<ITimezoneProvider>();
            DevSubInteractionContext ctx = (DevSubInteractionContext)context;

            IEnumerable<BotTimezone> timezones = provider.GetAllTimezones();
            string input = autocompleteInteraction.Data.Current.Value?.ToString();
            if (!string.IsNullOrEmpty(input))
                timezones = timezones.Where(tz => tz.Name.Contains(input, StringComparison.OrdinalIgnoreCase) || tz.ID.Contains(input, StringComparison.OrdinalIgnoreCase));

            AutocompletionResult result = AutocompletionResult.FromSuccess(timezones
                .Select(tz => new AutocompleteResult(tz.Name, tz.ID))
                .Take(25));
            return Task.FromResult(result);
        }
    }
}

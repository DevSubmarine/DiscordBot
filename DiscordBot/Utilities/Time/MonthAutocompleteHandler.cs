using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace DevSubmarine.DiscordBot.Time
{
    public class MonthAutocompleteHandler : AutocompleteHandler
    {
        private static readonly IEnumerable<AutocompleteMonth> _months = Enum.GetValues<Month>()
            .Select(m => AutocompleteMonth.FromMonth(m));

        public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            ITimezoneProvider provider = services.GetRequiredService<ITimezoneProvider>();
            DevSubInteractionContext ctx = (DevSubInteractionContext)context;

            IEnumerable<AutocompleteMonth> months = _months;
            string input = autocompleteInteraction.Data.Current.Value?.ToString();
            if (!string.IsNullOrEmpty(input))
                months = months.Where(m => m.IsMatch(input));

            AutocompletionResult result = AutocompletionResult.FromSuccess(
                months.Select(m => (AutocompleteResult)m));
            return Task.FromResult(result);
        }

        private record AutocompleteMonth(string Name, int Value)
        {
            public static AutocompleteMonth FromMonth(Month month)
                => new AutocompleteMonth(month.ToString(), (int)month);

            public static implicit operator AutocompleteResult(AutocompleteMonth month)
                => new AutocompleteResult(month.Name, month.Value);

            public bool IsMatch(string input)
                => this.Name.Contains(input, StringComparison.OrdinalIgnoreCase) 
                || (int.TryParse(input, out int value) && value == this.Value);
        }
    }
}

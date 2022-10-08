using DevSubmarine.DiscordBot.Time;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;

namespace DevSubmarine.DiscordBot.Tools.TimezonesGenerator
{
    internal class ApplicationRunner
    {
        private readonly TimezonesGeneratorOptions _options;
        private readonly ILogger _log;

        public ApplicationRunner(IOptions<TimezonesGeneratorOptions> options, ILogger<ApplicationRunner> log)
        {
            this._options = options.Value;
            this._log = log;
        }

        public async Task RunAsync()
        {
            this._log.LogDebug("Getting system timezone information");
            IEnumerable<TimeZoneInfo> timezones = TimeZoneInfo.GetSystemTimeZones();
            this._log.LogInformation("{Count} system timezones found", timezones.Count());

            this._log.LogInformation("Serializing timezones");
            IEnumerable<string> serializedTimezones = timezones.Select(tz =>
            {
                this._log.LogDebug("Serializing timezone {ID} ({Name})", tz.Id, tz.DisplayName);
                return tz.ToSerializedString();
            });
            this._log.LogTrace("{Count} timezones serialized", serializedTimezones.Count());

            this._log.LogInformation("Saving results");
            this._log.LogDebug("Serializing to JSON object");
            JObject result = new JObject(
                new JProperty(nameof(TimezoneOptions.SerializedTimezones), new JArray(serializedTimezones)));

            this._log.LogDebug("Saving timezones to file {FilePath}", _options.OutputFile);
            using FileStream stream = File.Create(this._options.OutputFile);
            using StreamWriter writer = new StreamWriter(stream);
            await writer.WriteAsync(result.ToString(Newtonsoft.Json.Formatting.Indented));

            this._log.LogInformation("Timezones saved to {FilePath}", this._options.OutputFile);
        }
    }
}

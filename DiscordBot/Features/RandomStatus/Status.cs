using Discord;

namespace DevSubmarine.DiscordBot.RandomStatus
{
    public class Status
    {
        public string Text { get; set; } = null;
        public string Link { get; set; } = null;
        public ActivityType ActivityType { get; set; } = ActivityType.Playing;

        public override string ToString()
        {
            switch (this.ActivityType)
            {
                case ActivityType.Playing:
                    return $"Playing {this.Text}";
                case ActivityType.Streaming:
                    return $"Streaming {this.Text}";
                case ActivityType.Watching:
                    return $"Watching {this.Text}";
                case ActivityType.Listening:
                    return $"Listening to {this.Text}";
                default:
                    throw new NotSupportedException($"Activity of type {this.ActivityType} is not supported");
            }
        }
    }
}

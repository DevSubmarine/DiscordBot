using Discord;

namespace DevSubmarine.DiscordBot.Birthdays
{
    public interface IUserBirthdayEmbedBuilder
    {
        Task<Embed> BuildUserBirthdayEmbedAsync(UserBirthday birthday, ulong? guildID, CancellationToken cancellationToken = default);
        Task<Embed> BuildUpcomingBirthdaysEmbedAsync(IEnumerable<UserBirthday> birthdays, CancellationToken cancellationToken = default);
    }
}

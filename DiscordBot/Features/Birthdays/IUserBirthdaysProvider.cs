namespace DevSubmarine.DiscordBot.Birthdays
{
    public interface IUserBirthdaysProvider
    {
        Task<UserBirthday> GetAsync(ulong userID, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserBirthday>> GetTodayBirthdaysAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<UserBirthday>> GetUpcomingBirthdaysAsync(int days, CancellationToken cancellationToken = default);
        Task AddBirthdayAsync(UserBirthday birthday, CancellationToken cancellationToken = default);
    }
}

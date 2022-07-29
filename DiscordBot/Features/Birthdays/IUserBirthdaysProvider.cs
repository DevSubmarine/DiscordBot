namespace DevSubmarine.DiscordBot.Birthdays
{
    public interface IUserBirthdaysProvider
    {
        Task<UserBirthday> GetAsync(ulong userID, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserBirthday>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(UserBirthday birthday, CancellationToken cancellationToken = default);
    }
}

namespace DevSubmarine.DiscordBot.Birthdays
{
    public interface IUserBirthdaysStore
    {
        Task<IEnumerable<UserBirthday>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<UserBirthday> GetAsync(ulong userID, CancellationToken cancellationToken = default);
        Task UpdateAsync(UserBirthday birthday, CancellationToken cancellationToken = default);
    }
}

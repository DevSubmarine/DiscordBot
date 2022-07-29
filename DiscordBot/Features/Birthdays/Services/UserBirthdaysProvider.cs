using DevSubmarine.DiscordBot.Caching;

namespace DevSubmarine.DiscordBot.Birthdays.Services
{
    internal class UserBirthdaysProvider : IUserBirthdaysProvider, IDisposable
    {
        private readonly ICacheProvider<UserBirthday> _cache;
        private readonly IUserBirthdaysStore _store;
        private readonly ILogger _log;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public UserBirthdaysProvider(ICacheProvider<UserBirthday> cache, IUserBirthdaysStore store, ILogger<UserBirthdaysProvider> log)
        {
            this._cache = cache;
            this._store = store;
            this._log = log;
        }

        public async Task<UserBirthday> GetAsync(ulong userID, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (this._cache.TryGetItem(UserBirthday.GetCacheKey(userID), out UserBirthday result))
                {
                    this._log.LogTrace("Birthday for user {UserID} found in cache", userID);
                    return result;
                }

                result = await this._store.GetAsync(userID, cancellationToken).ConfigureAwait(false);
                this._cache.AddItem(UserBirthday.GetCacheKey(userID), result);
                return result;
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task AddBirthdayAsync(UserBirthday birthday, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await this._store.UpdateAsync(birthday, cancellationToken);
                this._cache.AddItem(birthday);
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task<IEnumerable<UserBirthday>> GetTodayBirthdaysAsync(CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                IEnumerable<UserBirthday> results = await this._store.GetAllAsync(cancellationToken).ConfigureAwait(false);
                return results.Where(birthday => birthday.Date == BirthdayDate.Today);
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task<IEnumerable<UserBirthday>> GetUpcomingBirthdaysAsync(int days, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                IEnumerable<UserBirthday> results = await this._store.GetAllAsync(cancellationToken).ConfigureAwait(false);
                DateTime startDate = (DateTime)BirthdayDate.Today.AddDays(1);
                DateTime endDate = startDate.AddDays(days);
                return results.Where(birthday 
                    => birthday.Date != BirthdayDate.Today 
                    && (DateTime)birthday.Date >= startDate
                    && (DateTime)birthday.Date <= endDate);
            }
            finally
            {
                this._lock.Release();
            }
        }

        public void Dispose()
        {
            try { this._lock?.Dispose(); } catch { }
        }
    }
}

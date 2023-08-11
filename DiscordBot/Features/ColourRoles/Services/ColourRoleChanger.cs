using Discord;

namespace DevSubmarine.DiscordBot.ColourRoles
{
    public interface IColourRoleChanger
    {
        Task SetUserRoleAsync(IGuildUser user, IRole role, CancellationToken cancellationToken = default);
        Task RemoveUserRolesAsync(IGuildUser user, Func<IRole, bool> filter = null, CancellationToken cancellationToken = default);
    }

    namespace Services
    {
        internal class ColourRoleChanger : IColourRoleChanger
        {
            private readonly IColourRoleProvider _provider;
            private readonly ILogger _log;
            private readonly IOptionsMonitor<ColourRolesOptions> _options;

            public ColourRoleChanger(IColourRoleProvider provider, ILogger<ColourRoleChanger> log, IOptionsMonitor<ColourRolesOptions> options)
            {
                this._provider = provider;
                this._log = log;
                this._options = options;
            }

            public async Task SetUserRoleAsync(IGuildUser user, IRole role, CancellationToken cancellationToken = default)
            {
                if (this._options.CurrentValue.RemoveOldRoles)
                    await this.RemoveUserRolesAsync(user, oldRole => oldRole.Id != role.Id).ConfigureAwait(false);

                // special case is when user already has requested role. Just skip doing any changes then to prevent exceptions, Discord vomiting or whatever else
                if (!user.RoleIds.Contains(role.Id))
                {
                    this._log.LogDebug("Adding role {RoleName} ({RoleID}) to user {UserID}", role.Name, role.Id, user.Id);
                    await user.AddRoleAsync(role, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
                }
            }

            public Task RemoveUserRolesAsync(IGuildUser user, Func<IRole, bool> filter = null, CancellationToken cancellationToken = default)
            {
                IEnumerable<IRole> rolesToRemove = this._provider.GetUsersCurrentRoles(user);
                if (filter != null)
                    rolesToRemove = rolesToRemove.Where(role => filter(role));

                if (rolesToRemove.Any())
                {
                    this._log.LogTrace("Removing {Count} old colour roles from user {UserID}", rolesToRemove.Count(), user.Id);
                    return user.RemoveRolesAsync(rolesToRemove.Select(role => role.Id), cancellationToken.ToRequestOptions());
                }

                return Task.CompletedTask;
            }
        }
    }
}

using Discord;
using TehGM.Utilities.Randomization;

namespace DevSubmarine.DiscordBot.ColourRoles
{
    public interface IColourRoleProvider
    {
        IEnumerable<IRole> GetAvailableRoles(IGuild guild);
        IRole GetUsersHighestRole(IGuildUser user);
        IRole GetRandomRole(IGuild guild, ulong? excludingRoleID = null);
    }

    public static class ColourRoleProviderExtensions
    {
        public static bool IsRoleAvailable(this IColourRoleProvider provider, IRole role)
            => IsRoleAvailable(provider, role.Guild, role.Id);

        public static bool IsRoleAvailable(this IColourRoleProvider provider, IGuild guild, ulong roleID)
            => provider.GetAvailableRoles(guild).Any(r => r.Id == roleID);

        public static IEnumerable<IRole> GetUsersCurrentRoles(this IColourRoleProvider provider, IGuildUser user)
        {
            return provider.GetAvailableRoles(user.Guild).IntersectBy(user.RoleIds, role => role.Id);
        }

        public static IRole GetNewRandomRole(this IColourRoleProvider provider, IGuildUser user)
        {
            // to ensure role does change, exclude user's current role
            // they might have multiple for some damn reason (wtf Nerdu?), but it's okay - we only care about the highest one as that influences colour
            // note they might have none, so it might be null
            IRole currentRole = provider.GetUsersHighestRole(user);
            return provider.GetRandomRole(user.Guild, currentRole?.Id);
        }
    }

    namespace Services
    {
        internal class ColourRoleProvider : IColourRoleProvider
        {
            private readonly IRandomizer _randomizer;
            private readonly IOptionsMonitor<ColourRolesOptions> _options;

            public ColourRoleProvider(IRandomizer randomizer, IOptionsMonitor<ColourRolesOptions> options)
            {
                this._randomizer = randomizer;
                this._options = options;
            }

            public IEnumerable<IRole> GetAvailableRoles(IGuild guild)
            {
                return guild.Roles.Where(role =>
                    this._options.CurrentValue.AllowedRoleIDs.Contains(role.Id)
                    && role.Color != Color.Default);
            }

            public IRole GetUsersHighestRole(IGuildUser user)
            {
                return user.GetHighestRole(r => r.Color != Color.Default && this._options.CurrentValue.AllowedRoleIDs.Contains(r.Id));
            }

            public IRole GetRandomRole(IGuild guild, ulong? excludingRoleID = null)
            {
                ColourRolesOptions options = this._options.CurrentValue;
                IEnumerable<IRole> availableRoles = excludingRoleID == null 
                    ? this.GetAvailableRoles(guild) 
                    : this.GetAvailableRoles(guild).ExceptBy(new[] { excludingRoleID.Value }, r => r.Id);
                return this._randomizer.GetRandomValue(availableRoles);
            }
        }
    }
}

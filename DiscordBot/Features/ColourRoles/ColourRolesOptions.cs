namespace DevSubmarine.DiscordBot.ColourRoles
{
    public class ColourRolesOptions
    {
        /// <summary>Whether user's previous roles will removed when they change to a new one.</summary>
        /// <remarks>Only roles listed in <see cref="AllowedRoleIDs"/> will be removed. Other roles will be ignored.</remarks>
        public bool RemoveOldRoles { get; set; } = true;
        /// <summary>Collection of roles that can be set.</summary>
        /// <remarks>This collection is used to fine-tune all roles that are possible to choose. This is so the bot doesn't automatically list any special roles.<br/>
        /// It can contain roles from multiple guilds. This should cause no issue, as it's used as a filter only.</remarks>
        public IEnumerable<ulong> AllowedRoleIDs { get; set; }
    }
}

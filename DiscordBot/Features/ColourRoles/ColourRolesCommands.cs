using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using TehGM.Utilities.Randomization;

namespace DevSubmarine.DiscordBot.ColourRoles
{
    [Group("colour", "Commands for changing your nickname colour")]
    [EnabledInDm(false)]
    public class ColourRolesCommands : DevSubInteractionModule
    {
        private readonly IRandomizer _randomizer;
        private readonly IOptionsMonitor<ColourRolesOptions> _options;
        private readonly ILogger _log;

        public ColourRolesCommands(IRandomizer randomizer, IOptionsMonitor<ColourRolesOptions> options, ILogger<ColourRolesCommands> log)
        {
            this._randomizer = randomizer;
            this._options = options;
            this._log = log;
        }

        [SlashCommand("set", "Sets your colour role to the one you selected")]
        [EnabledInDm(false)]
        public async Task CmdSetAsync(
            [Summary("Role", "Role to apply")] IRole role,
            [Summary("User", "Which user to apply the role to; can only be used by administrators")] IGuildUser user = null)
        {
            ColourRolesOptions options = this._options.CurrentValue;

            if (!this.GetAvailableRoles().Any(r => r.Id == role.Id))
            {
                await base.RespondAsync(
                    text: $"{ResponseEmoji.Failure} No, you cannot use {role.Mention} role!",
                    allowedMentions: new AllowedMentions(AllowedMentionTypes.Users),
                    options: base.GetRequestOptions()).ConfigureAwait(false);
                return;
            }

            await base.DeferAsync(options: base.GetRequestOptions()).ConfigureAwait(false);

            // if changing role of the other user, it should be only possible for user with specific permissions (admins basically)
            IGuildUser callerUser = await base.Context.Guild.GetGuildUserAsync(base.Context.User.Id, base.Context.CancellationToken).ConfigureAwait(false);
            if (user != null)
            {
                if (!this.CanEditOtherUsers(callerUser, out IRole highestRole))
                {
                    await base.ModifyOriginalResponseAsync(msg => msg.Content = $"{ResponseEmoji.Failure} You have no permissions to edit other users' roles.");
                    return;
                }

                // an additional case is when user has admin perms, but the target role is above theirs
                // this isn't really common, but it is a possibility, and security comes first, so we need to check that
                if (!callerUser.IsOwner() && role.Position >= highestRole.Position)
                {
                    await base.ModifyOriginalResponseAsync(msg => msg.Content = $"{ResponseEmoji.Failure} Your role isn't privileged enough to assign this role.");
                    return;
                }
            }
            else
                user = callerUser;

            await this.SetUserRoleAsync(user, role).ConfigureAwait(false);
            await this.ConfirmRoleChangeAsync(role.Color, role.Mention).ConfigureAwait(false);
        }

        [SlashCommand("list", "Lists all colour roles you can pick")]
        [EnabledInDm(false)]
        public Task CmdListAsync()
        {
            IEnumerable<SocketRole> availableRoles = this.GetAvailableRoles();
            if (!availableRoles.Any())
                return base.RespondAsync($"{ResponseEmoji.Failure} No applicable roles found?! {ResponseEmoji.JerryWhat}");

            string rolesList = string.Join('\n', availableRoles
                .OrderBy(role => role.Name)
                .Select(role => role.Mention));
            Embed embed = new EmbedBuilder()
                .WithTitle("Available Colour Roles")
                .WithDescription(rolesList)
                .Build();

            return base.RespondAsync(
                embed: embed,
                allowedMentions: AllowedMentions.None,
                ephemeral: true,
                options: base.GetRequestOptions());
        }

        [SlashCommand("clear", "Clears your role colour")]
        [EnabledInDm(false)]
        public async Task CmdClearAsync(
            [Summary("User", "Which user to apply the role to; can only be used by administrators")] IGuildUser user = null)
        {
            ColourRolesOptions options = this._options.CurrentValue;
            await base.DeferAsync(options: base.GetRequestOptions()).ConfigureAwait(false);

            // if changing role of the other user, it should be only possible for user with specific permissions (admins basically)
            IGuildUser callerUser = await base.Context.Guild.GetGuildUserAsync(base.Context.User.Id, base.Context.CancellationToken).ConfigureAwait(false);
            if (user != null)
            {
                if (!this.CanEditOtherUsers(callerUser, out IRole highestCallerRole))
                {
                    await base.ModifyOriginalResponseAsync(msg => msg.Content = $"{ResponseEmoji.Failure} You have no permissions to edit other users' roles.");
                    return;
                }

                // an additional case is when user has admin perms, but the target role is above theirs
                // this isn't really common, but it is a possibility, and security comes first, so we need to check that
                IRole highestTargetRole = user.GetHighestRole(r => options.AllowedRoleIDs.Contains(r.Id));
                if (!callerUser.IsOwner() && highestTargetRole.Position >= highestCallerRole.Position)
                {
                    await base.ModifyOriginalResponseAsync(msg => msg.Content = $"{ResponseEmoji.Failure} Your role isn't privileged enough to clear colour of this user.");
                    return;
                }
            }
            else
                user = callerUser;

            try
            {
                await this.RemoveColourRolesAsync(user).ConfigureAwait(false);
            }
            catch (HttpException ex) when (ex.IsMissingPermissions())
            {
                await base.ModifyOriginalResponseAsync(msg => msg.Content = $"Oops! {ResponseEmoji.Failure}\nI lack permissions to change your role! {ResponseEmoji.FeelsBeanMan}",
                    base.GetRequestOptions()).ConfigureAwait(false);
                return;
            }

            await this.ConfirmRoleChangeAsync(Color.Default, "colour-naked").ConfigureAwait(false);
        }

        [SlashCommand("random", "Changes your colour role to a random one")]
        [EnabledInDm(false)]
        public async Task CmdRandomAsync()
        {
            ColourRolesOptions options = this._options.CurrentValue;
            await base.DeferAsync(options: base.GetRequestOptions()).ConfigureAwait(false);

            IGuildUser user = await base.Context.Guild.GetGuildUserAsync(base.Context.User.Id).ConfigureAwait(false);

            // to ensure role does change, exclude user's current role
            // they might have multiple for some damn reason (wtf Nerdu?), but it's okay - we only care about the highest one as that influences colour
            // note they might have none
            ulong? excludedRoleID = user.GetHighestRole(r => r.Color != Color.Default && options.AllowedRoleIDs.Contains(r.Id))?.Id;
            IEnumerable<ulong> availableIDs = excludedRoleID == null ? options.AllowedRoleIDs : options.AllowedRoleIDs.Except(new[] { excludedRoleID.Value });

            // keep in mind that available roles might contain roles from other guilds, so we have to verify them
            availableIDs = availableIDs.Intersect(base.Context.Guild.Roles.Select(r => r.Id));

            ulong selectedRoleID = this._randomizer.GetRandomValue(availableIDs);
            IRole selectedRole = base.Context.Guild.GetRole(selectedRoleID);

            await this.SetUserRoleAsync(user, selectedRole).ConfigureAwait(false);
            await this.ConfirmRoleChangeAsync(selectedRole.Color, selectedRole.Mention).ConfigureAwait(false);
        }

        private async Task SetUserRoleAsync(IGuildUser user, IRole role)
        {
            try
            {
                if (this._options.CurrentValue.RemoveOldRoles)
                    await this.RemoveColourRolesAsync(user, oldRole => oldRole.Id != role.Id).ConfigureAwait(false);

                // special case is when user already has requested role. Just skip doing any changes then to prevent exceptions, Discord vomiting or whatever else
                if (!user.RoleIds.Contains(role.Id))
                {
                    this._log.LogDebug("Adding role {RoleName} ({RoleID}) to user {UserID}", role.Name, role.Id, user.Id);
                    await user.AddRoleAsync(role, base.GetRequestOptions()).ConfigureAwait(false);
                }
            }
            catch (HttpException ex) when (ex.IsMissingPermissions())
            {
                await base.ModifyOriginalResponseAsync(msg => msg.Content = $"Oops! {ResponseEmoji.Failure}\nI lack permissions to change your role! {ResponseEmoji.FeelsBeanMan}",
                    base.GetRequestOptions()).ConfigureAwait(false);
                return;
            }
        }

        private IEnumerable<SocketRole> GetAvailableRoles()
        {
            return base.Context.Guild.Roles.Where(role =>
                this._options.CurrentValue.AllowedRoleIDs.Contains(role.Id)
                && role.Color != Color.Default);
        }

        private IEnumerable<IRole> GetUserCurrentRoles(IGuildUser user)
        {
            return user.GetRoles(role => this._options.CurrentValue.AllowedRoleIDs.Contains(role.Id));
        }

        private Task RemoveColourRolesAsync(IGuildUser user, Func<IRole, bool> filter = null)
        {
            IEnumerable<IRole> rolesToRemove = this.GetUserCurrentRoles(user);
            if (filter != null)
                rolesToRemove = rolesToRemove.Where(role => filter(role));

            if (rolesToRemove.Any())
            {
                this._log.LogTrace("Removing {Count} old colour roles from user {UserID}", rolesToRemove.Count(), user.Id);
                return user.RemoveRolesAsync(rolesToRemove.Select(role => role.Id), base.GetRequestOptions());
            }

            return Task.CompletedTask;
        }

        private Task ConfirmRoleChangeAsync(Color roleColour, string roleText)
        {
            return base.ModifyOriginalResponseAsync(msg =>
            {
                msg.AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
                msg.Embed = new EmbedBuilder()
                    .WithTitle("Colour Role changed!")
                    .WithAuthor(base.Context.User)
                    .WithColor(roleColour)
                    .WithDescription($"You're now {roleText} {ResponseEmoji.EyesBlurry}")
                    .Build();
                msg.Content = null;
            }, base.GetRequestOptions());
        }

        private bool CanEditOtherUsers(IGuildUser user, out IRole adminRole)
        {
            adminRole = user.GetHighestRole(r => r.Permissions.Administrator || r.Permissions.ManageRoles);
            return adminRole != null || user.IsOwner();
        }
    }
}

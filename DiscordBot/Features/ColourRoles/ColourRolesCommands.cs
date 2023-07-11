using Discord;
using Discord.Interactions;
using Discord.Net;

namespace DevSubmarine.DiscordBot.ColourRoles
{
    [Group("colour", "Commands for changing your nickname colour")]
    [EnabledInDm(false)]
    public class ColourRolesCommands : DevSubInteractionModule
    {
        private readonly IColourRoleProvider _roleProvider;
        private readonly IColourRoleChanger _roleChanger;
        private readonly IOptionsMonitor<ColourRolesOptions> _options;
        private readonly ILogger _log;

        public ColourRolesCommands(IColourRoleProvider colourRoleProvider, IColourRoleChanger roleChanger,
            IOptionsMonitor<ColourRolesOptions> options, ILogger<ColourRolesCommands> log)
        {
            this._options = options;
            this._log = log;
            this._roleProvider = colourRoleProvider;
            this._roleChanger = roleChanger;
        }

        [SlashCommand("set", "Sets your colour role to the one you selected")]
        [EnabledInDm(false)]
        public async Task CmdSetAsync(
            [Summary("Role", "Role to apply")] IRole role,
            [Summary("User", "Which user to apply the role to; can only be used by administrators")] IGuildUser user = null)
        {
            if (!this._roleProvider.IsRoleAvailable(role))
            {
                await base.RespondAsync(
                    text: $"{ResponseEmoji.Failure} No, you cannot use {role.Mention} role!",
                    allowedMentions: new AllowedMentions(AllowedMentionTypes.Users),
                    options: base.GetRequestOptions()).ConfigureAwait(false);
                return;
            }

            await base.DeferAsync(options: base.GetRequestOptions()).ConfigureAwait(false);

            // if changing role of the other user, it should be only possible for user with specific permissions (admins basically)
            IGuildUser callerUser = await base.Context.Guild.GetGuildUserAsync(base.Context.User.Id, base.CancellationToken).ConfigureAwait(false);
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
            IEnumerable<IRole> availableRoles = this._roleProvider.GetAvailableRoles(base.Context.Guild);
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

        [SlashCommand("clear", "Clears your colour role")]
        [EnabledInDm(false)]
        public async Task CmdClearAsync(
            [Summary("User", "Which user to apply the role to; can only be used by administrators")] IGuildUser user = null)
        {
            ColourRolesOptions options = this._options.CurrentValue;
            await base.DeferAsync(options: base.GetRequestOptions()).ConfigureAwait(false);

            // if changing role of the other user, it should be only possible for user with specific permissions (admins basically)
            IGuildUser callerUser = await base.Context.Guild.GetGuildUserAsync(base.Context.User.Id, base.CancellationToken).ConfigureAwait(false);
            if (user != null)
            {
                if (!this.CanEditOtherUsers(callerUser, out IRole highestCallerRole))
                {
                    await base.ModifyOriginalResponseAsync(msg => msg.Content = $"{ResponseEmoji.Failure} You have no permissions to edit other users' roles.");
                    return;
                }

                // an additional case is when user has admin perms, but the target role is above theirs
                // this isn't really common, but it is a possibility, and security comes first, so we need to check that
                if (!callerUser.IsOwner() && user.GetHighestRole().Position >= highestCallerRole.Position)
                {
                    await base.ModifyOriginalResponseAsync(msg => msg.Content = $"{ResponseEmoji.Failure} Your role isn't privileged enough to clear colour of this user.");
                    return;
                }
            }
            else
                user = callerUser;

            try
            {
                await this._roleChanger.RemoveUserRolesAsync(user).ConfigureAwait(false);
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
            await base.DeferAsync(options: base.GetRequestOptions()).ConfigureAwait(false);

            IGuildUser user = await base.Context.Guild.GetGuildUserAsync(base.Context.User.Id).ConfigureAwait(false);
            IRole selectedRole = this._roleProvider.GetNewRandomRole(user);

            if (await this.SetUserRoleAsync(user, selectedRole).ConfigureAwait(false))
                await this.ConfirmRoleChangeAsync(selectedRole.Color, selectedRole.Mention).ConfigureAwait(false);
        }

        private async Task<bool> SetUserRoleAsync(IGuildUser user, IRole role)
        {
            try
            {
                await this._roleChanger.SetUserRoleAsync(user, role);
                return true;
            }
            catch (HttpException ex) when (ex.IsMissingPermissions())
            {
                await base.ModifyOriginalResponseAsync(msg => msg.Content = $"Oops! {ResponseEmoji.Failure}\nI lack permissions to change your role! {ResponseEmoji.FeelsBeanMan}",
                    base.GetRequestOptions()).ConfigureAwait(false);
                return false;
            }
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

using Discord;

namespace DevSubmarine.DiscordBot.BlogsManagement
{
    public class BlogChannelPermissionsOptions
    {
        public ulong TargetID { get; set; }
        public PermissionTarget TargetType { get; set; } = PermissionTarget.User;
        public PermissionValues Permissions { get; set; } = new PermissionValues();

        public class PermissionValues
        {
            public bool? SendMessages { get; set; }
            public bool? ManageMessages { get; set; }
            public bool? ManageWebhooks { get; set; }
            public bool? UseApplicationCommands { get; set; }
            public bool? ViewChannel { get; set; }
            public bool? EmbedLinks { get; set; }
            public bool? AddReactions { get; set; }
            public bool? UseExternalEmojis { get; set; }
        }
    }
}

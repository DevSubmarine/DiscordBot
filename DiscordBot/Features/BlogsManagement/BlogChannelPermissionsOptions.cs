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
            public bool? ViewChannel { get; set; }
            public bool? ManageChannel { get; set; }
            public bool? ManagePermissions { get; set; }
            public bool? ManageWebhooks { get; set; }
            public bool? CreateInvite { get; set; }
            public bool? SendMessages { get; set; }
            public bool? SendMessagesInThreads { get; set; }
            public bool? CreatePublicThreads { get; set; }
            public bool? CreatePrivateThreads { get; set; }
            public bool? EmbedLinks { get; set; }
            public bool? AttachFiles { get; set; }
            public bool? AddReactions { get; set; }
            public bool? UseExternalEmojis { get; set; }
            public bool? UseExternalStickers { get; set; }
            public bool? MentionEveryone { get; set; }
            public bool? ManageMessages { get; set; }
            public bool? ManageThreads { get; set; }
            public bool? ReadMessageHistory { get; set; }
            public bool? SendTTSMessages { get; set; }
            public bool? UseApplicationCommands { get; set; }
            public bool? UseActivities { get; set; }
        }
    }
}

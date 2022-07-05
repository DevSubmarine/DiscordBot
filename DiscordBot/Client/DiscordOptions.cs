namespace DevSubmarine.DiscordBot.Client
{
    public class DiscordOptions
    {
        /// <summary>Bot token from Discord API portal.</summary>
        public string BotToken { get; set; }
        /// <summary>ID of guild to register commands to. Should be null, unless testing.</summary>
        public ulong? CommandsGuildID { get; set; }
        /// <summary>Compiles commands, which improves their execution speed, but increases memory use.</summary>
        public bool CompileCommands { get; set; }
        /// <summary>Purges global commands before registering new ones. 
        /// Only use if registering production bot's commands to guild after they've been already registered globally.</summary>
        public bool PurgeGlobalCommands { get; set; }
    }
}
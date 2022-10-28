namespace DevSubmarine.DiscordBot.Client
{
    public class DiscordOptions
    {
        /// <summary>Bot token from Discord API portal.</summary>
        public string BotToken { get; set; }
        /// <summary>ID of guild to register commands to. Will be ignored if <see cref="RegisterCommandsGlobally"/> is set to true.</summary>
        public ulong? CommandsGuildID { get; set; }
        /// <summary>Compiles commands, which improves their execution speed, but increases memory use.</summary>
        public bool CompileCommands { get; set; }
        /// <summary>Whether commands should be registered globally. Other commands will be purged.</summary>
        public bool RegisterCommandsGlobally { get;set; } = true;
    }
}
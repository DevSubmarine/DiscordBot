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
    }
}
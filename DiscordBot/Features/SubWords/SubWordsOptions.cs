using DevSubmarine.DiscordBot.PasteMyst;

namespace DevSubmarine.DiscordBot.SubWords
{
    public class SubWordsOptions
    {
        /// <summary>How long the words list paste will be alive.</summary>
        public PasteExpiration ListExpiration { get; set; } = PasteExpiration.OneMonth;
    }
}

using Microsoft.CodeAnalysis;

namespace DevSubmarine.Analyzers.DiscordInteractions
{
    internal static class DiagnosticRule
    {
        public static readonly DiagnosticDescriptor NotDevSubBaseClass = new DiagnosticDescriptor(DiagnosticID.NotDevSubBaseClass,
            GetResourceString(nameof(Resources.NotDevSubBaseClass_AnalyzerTitle)),
            GetResourceString(nameof(Resources.NotDevSubBaseClass_AnalyzerMessageFormat)),
            "SlashCommands", DiagnosticSeverity.Warning, true,
            GetResourceString(nameof(Resources.NotDevSubBaseClass_AnalyzerDescription)));


        private static LocalizableString GetResourceString(string name)
            => new LocalizableResourceString(name, Resources.ResourceManager, typeof(Resources));
    }
}

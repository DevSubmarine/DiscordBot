using Microsoft.CodeAnalysis;

namespace DevSubmarine.Analyzers.StatusPlaceholder
{
    internal static class DiagnosticRule
    {
        public static readonly DiagnosticDescriptor MissingInterface = new DiagnosticDescriptor(DiagnosticID.MissingInterface,
            GetResourceString(nameof(Resources.MissingInterface_AnalyzerTitle)),
            GetResourceString(nameof(Resources.MissingInterface_AnalyzerMessageFormat)),
            "StatusPlaceholder", DiagnosticSeverity.Error, true,
            GetResourceString(nameof(Resources.MissingInterface_AnalyzerDescription)));

        public static readonly DiagnosticDescriptor MissingAttribute = new DiagnosticDescriptor(DiagnosticID.MissingAttribute,
            GetResourceString(nameof(Resources.MissingAttribute_AnalyzerTitle)),
            GetResourceString(nameof(Resources.MissingAttribute_AnalyzerMessageFormat)),
            "StatusPlaceholder", DiagnosticSeverity.Error, true,
            GetResourceString(nameof(Resources.MissingAttribute_AnalyzerDescription)));

        public static readonly DiagnosticDescriptor IsAbstract = new DiagnosticDescriptor(DiagnosticID.IsAbstract,
            GetResourceString(nameof(Resources.IsAbstract_AnalyzerTitle)),
            GetResourceString(nameof(Resources.IsAbstract_AnalyzerMessageFormat)),
            "StatusPlaceholder", DiagnosticSeverity.Error, true,
            GetResourceString(nameof(Resources.IsAbstract_AnalyzerDescription)));


        private static LocalizableString GetResourceString(string name)
            => new LocalizableResourceString(name, Resources.ResourceManager, typeof(Resources));
    }
}

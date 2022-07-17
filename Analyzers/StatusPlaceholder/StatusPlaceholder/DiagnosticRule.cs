using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace StatusPlaceholder
{
    internal static class DiagnosticRule
    {
        public static readonly DiagnosticDescriptor MissingInterface = new DiagnosticDescriptor("DS001",
            GetResourceString(nameof(Resources.MissingInterface_AnalyzerTitle)),
            GetResourceString(nameof(Resources.MissingInterface_AnalyzerMessageFormat)),
            "StatusPlaceholder", DiagnosticSeverity.Error, true,
            GetResourceString(nameof(Resources.MissingInterface_AnalyzerDescription)));


        private static LocalizableString GetResourceString(string name)
            => new LocalizableResourceString(name, Resources.ResourceManager, typeof(Resources));
    }
}

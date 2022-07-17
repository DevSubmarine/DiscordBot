using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace StatusPlaceholder
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StatusPlaceholderAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(DiagnosticRule.MissingInterface);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeMissingInterface, SyntaxKind.ClassDeclaration);
        }

        private static void AnalyzeMissingInterface(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is ClassDeclarationSyntax classDeclaration))
                return;

            bool containsAttribute = false;
            foreach (AttributeListSyntax attributeList in classDeclaration.AttributeLists)
            {
                if (attributeList.Attributes.Any(attr => attr.Name.ToString() == "StatusPlaceholder" || attr.Name.ToString() == "StatusPlaceholderAttribute"))
                {
                    containsAttribute = true;
                    break;
                }
            }
            if (!containsAttribute)
                return;

            bool hasInterface = classDeclaration.BaseList?.Types.Any(t => t.Type.ToString() == "IStatusPlaceholder") == true;
            if (!hasInterface)
            {
                INamedTypeSymbol symbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticRule.MissingInterface, symbol.Locations.First(), classDeclaration.Identifier.ToString()));
            }
        }
    }
}

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace DevSubmarine.Analyzers.StatusPlaceholder
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StatusPlaceholderAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(DiagnosticRule.MissingInterface, DiagnosticRule.MissingAttribute);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeMissingInterface, SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeMissingAttribute, SyntaxKind.ClassDeclaration);
        }

        private static void AnalyzeMissingInterface(SyntaxNodeAnalysisContext context)
        {
            if (!context.TryGetClassDeclaration(out ClassDeclarationSyntax declaration))
                return;

            if (!declaration.HasRequiredAttribute())
                return;

            if (!declaration.HasRequiredInterface())
            {
                INamedTypeSymbol symbol = context.SemanticModel.GetDeclaredSymbol(declaration);
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticRule.MissingInterface, symbol.Locations.First(), declaration.Identifier.ToString()));
            }
        }

        private static void AnalyzeMissingAttribute(SyntaxNodeAnalysisContext context)
        {
            if (!context.TryGetClassDeclaration(out ClassDeclarationSyntax declaration))
                return;

            if (!declaration.HasRequiredInterface())
                return;

            if (!declaration.HasRequiredAttribute())
            {
                INamedTypeSymbol symbol = context.SemanticModel.GetDeclaredSymbol(declaration);
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticRule.MissingAttribute, symbol.Locations.First(), declaration.Identifier.ToString()));
            }
        }
    }
}

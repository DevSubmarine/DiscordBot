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
            => ImmutableArray.Create(DiagnosticRule.MissingInterface, DiagnosticRule.MissingAttribute, DiagnosticRule.IsAbstract);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
        }

        private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (!context.TryGetClassDeclaration(out ClassDeclarationSyntax declaration))
                return;

            bool hasRequiredAttribute = declaration.HasRequiredAttribute();
            bool hasRequiredInterface = declaration.HasRequiredInterface();

            if (!hasRequiredAttribute && !hasRequiredInterface)
                return;

            INamedTypeSymbol symbol = context.SemanticModel.GetDeclaredSymbol(declaration);
            AnalyzeMissingInterface(context, declaration, symbol, hasRequiredAttribute, hasRequiredInterface);
            AnalyzeMissingAttribute(context, declaration, symbol, hasRequiredAttribute, hasRequiredInterface);
            AnalyzeAbstract(context, declaration, symbol, hasRequiredAttribute, hasRequiredInterface);
        }

        private static void AnalyzeMissingInterface(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax declaration, INamedTypeSymbol symbol, bool hasRequiredAttribute, bool hasRequiredInterface)
        {
            if (hasRequiredInterface)
                return;

            context.ReportDiagnostic(Diagnostic.Create(DiagnosticRule.MissingInterface, symbol.Locations.First(), declaration.Identifier.ToString()));
        }

        private static void AnalyzeMissingAttribute(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax declaration, INamedTypeSymbol symbol, bool hasRequiredAttribute, bool hasRequiredInterface)
        {
            if (hasRequiredAttribute)
                return;

            context.ReportDiagnostic(Diagnostic.Create(DiagnosticRule.MissingAttribute, symbol.Locations.First(), declaration.Identifier.ToString()));
        }

        private static void AnalyzeAbstract(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax declaration, INamedTypeSymbol symbol, bool hasRequiredAttribute, bool hasRequiredInterface)
        {
            if (!declaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.AbstractKeyword)))
                return;

            context.ReportDiagnostic(Diagnostic.Create(DiagnosticRule.IsAbstract, symbol.Locations.First(), declaration.Identifier.ToString()));
        }
    }
}

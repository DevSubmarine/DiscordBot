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

        private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext nodeContext)
        {
            if (!StatusPlaceholderDeclarationContext.TryGetFromContext(nodeContext, out StatusPlaceholderDeclarationContext context))
                return;

            AnalyzeMissingInterface(context);
            AnalyzeMissingAttribute(context);
            AnalyzeAbstract(context);
        }

        private static void AnalyzeMissingInterface(StatusPlaceholderDeclarationContext context)
        {
            if (context.HasRequiredInterface)
                return;
            context.ReportDiagnostic(DiagnosticRule.MissingInterface);
        }

        private static void AnalyzeMissingAttribute(StatusPlaceholderDeclarationContext context)
        {
            if (context.HasRequiredAttribute || context.IsAbstract)
                return;
            context.ReportDiagnostic(DiagnosticRule.MissingAttribute);
        }

        private static void AnalyzeAbstract(StatusPlaceholderDeclarationContext context)
        {
            if (!context.IsAbstract || !context.HasRequiredAttribute)
                return;
            context.ReportDiagnostic(DiagnosticRule.IsAbstract);
        }
    }
}

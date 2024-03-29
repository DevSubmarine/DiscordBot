﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace DevSubmarine.Analyzers.DiscordInteractions
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DiscordInteractionsAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(DiagnosticRule.NotDevSubBaseClass);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration, SyntaxKind.InterfaceDeclaration, SyntaxKind.EnumDeclaration);
        }

        private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext nodeContext)
        {
            if (!(nodeContext.Node is TypeDeclarationSyntax declaration))
                return;

            SimpleNameSyntax nameNode = declaration.BaseList?.Types.FirstOrDefault()?.ChildNodes()
                .FirstOrDefault() as SimpleNameSyntax;

            if (nameNode?.Identifier.ToString() == DisallowedTypeName.InteractionModuleBase)
            {
                INamedTypeSymbol symbol = nodeContext.SemanticModel.GetDeclaredSymbol(declaration);
                nodeContext.ReportDiagnostic(Diagnostic.Create(DiagnosticRule.NotDevSubBaseClass, symbol.Locations.First(), declaration.Identifier.ToString()));
            }
        }
    }
}

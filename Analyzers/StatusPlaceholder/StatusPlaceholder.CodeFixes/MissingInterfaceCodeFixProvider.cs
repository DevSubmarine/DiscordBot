﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DevSubmarine.Analyzers.StatusPlaceholder
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MissingInterfaceCodeFixProvider)), Shared]
    public class MissingInterfaceCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(DiagnosticID.MissingInterface);

        public sealed override FixAllProvider GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            Diagnostic diagnostic = context.Diagnostics.First();
            TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;
            TypeDeclarationSyntax declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CodeFixResources.MissingInterface_CodeFixTitle,
                    createChangedDocument: c => this.ImplementRequiredInterface(context.Document, declaration, c),
                    equivalenceKey: nameof(CodeFixResources.MissingInterface_CodeFixTitle)),
                diagnostic);
        }

        private async Task<Document> ImplementRequiredInterface(Document document, TypeDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            SyntaxGenerator generator = SyntaxGenerator.GetGenerator(document);
            SyntaxNode node = generator.AddBaseType(declaration, SyntaxFactory.ParseTypeName(RequiredTypeName.StatusPlaceholderInterface));
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);
            return document.WithSyntaxRoot(root.ReplaceNode(declaration, node));
        }
    }
}

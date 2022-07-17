using Microsoft.CodeAnalysis;
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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(IsAbstractCodeFixProvider)), Shared]
    public class IsAbstractCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(DiagnosticID.IsAbstract);

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
                    title: CodeFixResources.IsAbstract_CodeFixTitle,
                    createChangedDocument: c => this.RemoveAbstractKeyword(context.Document, declaration, c),
                    equivalenceKey: nameof(CodeFixResources.IsAbstract_CodeFixTitle)),
                diagnostic);
        }

        private async Task<Document> RemoveAbstractKeyword(Document document, TypeDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            SyntaxToken keyword = declaration.Modifiers.First(token => token.IsKind(SyntaxKind.AbstractKeyword));
            SyntaxTokenList modifiers = declaration.Modifiers.Remove(keyword);
            SyntaxNode node = declaration.WithModifiers(modifiers);
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);
            return document.WithSyntaxRoot(root.ReplaceNode(declaration, node));
        }
    }
}

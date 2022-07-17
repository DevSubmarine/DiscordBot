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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MissingAttributeCodeFixProvider)), Shared]
    public class MissingAttributeCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticID.MissingAttribute); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CodeFixResources.MissingAttribute_CodeFixTitle,
                    createChangedDocument: c => this.AddRequiredAttribute(context.Document, declaration, c),
                    equivalenceKey: nameof(CodeFixResources.MissingAttribute_CodeFixTitle)),
                diagnostic);
        }

        private async Task<Document> AddRequiredAttribute(Document document, TypeDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            SyntaxGenerator generator = SyntaxGenerator.GetGenerator(document);
            SyntaxNode node = declaration.WithAttributeLists(declaration.AttributeLists.Add(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("StatusPlaceholder"))
                .WithArgumentList(SyntaxFactory.AttributeArgumentList())))));
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);
            return document.WithSyntaxRoot(root.ReplaceNode(declaration, node));
        }
    }
}

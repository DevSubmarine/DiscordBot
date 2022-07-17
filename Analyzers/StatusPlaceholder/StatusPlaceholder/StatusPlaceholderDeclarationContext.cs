using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Linq;

namespace DevSubmarine.Analyzers.StatusPlaceholder
{
    internal class StatusPlaceholderDeclarationContext
    {
        public SyntaxNodeAnalysisContext NodeContext { get; }
        public BaseTypeDeclarationSyntax Declaration { get; }
        public INamedTypeSymbol Symbol { get; }
        public bool HasRequiredAttribute { get; }
        public bool HasRequiredInterface { get; }
        public bool IsAbstract { get; }

        public StatusPlaceholderDeclarationContext(SyntaxNodeAnalysisContext context, BaseTypeDeclarationSyntax declaration, INamedTypeSymbol symbol,
            bool hasRequiredAttribute, bool hasRequiredInterface, bool isAbstract)
        {
            this.NodeContext = context;
            this.Declaration = declaration;
            this.Symbol = symbol;
            this.HasRequiredAttribute = hasRequiredAttribute;
            this.HasRequiredInterface = hasRequiredInterface;
            this.IsAbstract = isAbstract;
        }

        public static bool TryGetFromContext(SyntaxNodeAnalysisContext context, out StatusPlaceholderDeclarationContext result)
        {
            result = null;
            if (!(context.Node is BaseTypeDeclarationSyntax declaration))
                return false;

            bool hasRequiredAttribute = declaration.AttributeLists.SelectMany(list => list.Attributes)
                .Any(attr => attr.Name.ToString() == RequiredTypeName.StatusPlaceholderAttribute || attr.Name.ToString() == RequiredTypeName.StatusPlaceholderAttribute + "Attribute");
            bool hasRequiredInterface = declaration.BaseList?.Types.Any(t => t.Type.ToString() == RequiredTypeName.StatusPlaceholderInterface) == true;

            if (!hasRequiredAttribute && !hasRequiredInterface)
                return false;

            bool isAbstract = declaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.AbstractKeyword));
            INamedTypeSymbol symbol = context.SemanticModel.GetDeclaredSymbol(declaration);

            result = new StatusPlaceholderDeclarationContext(context, declaration, symbol, hasRequiredAttribute, hasRequiredInterface, isAbstract);
            return true;
        }

        public void ReportDiagnostic(DiagnosticDescriptor diagnostic)
            => this.NodeContext.ReportDiagnostic(Diagnostic.Create(diagnostic, this.Symbol.Locations.First(), this.Declaration.Identifier.ToString()));
    }
}

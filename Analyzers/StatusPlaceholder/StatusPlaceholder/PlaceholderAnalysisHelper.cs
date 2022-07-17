﻿using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Linq;

namespace StatusPlaceholder
{
    internal static class PlaceholderAnalysisHelper
    {
        public static bool HasRequiredInterface(this ClassDeclarationSyntax declaration, string name = "IStatusPlaceholder")
            => declaration.BaseList?.Types.Any(t => t.Type.ToString() == name) == true;
        public static bool HasRequiredInterface(this SyntaxNodeAnalysisContext context, string name = "IStatusPlaceholder")
            => HasRequiredInterface(GetClassDeclaration(context), name);

        public static bool HasRequiredAttribute(this ClassDeclarationSyntax declaration, string name = "StatusPlaceholder")
        {
            string alternateName = name + "Attribute";
            return declaration.AttributeLists.SelectMany(list => list.Attributes)
                .Any(attr => attr.Name.ToString() == name || attr.Name.ToString() == alternateName);
        }
        public static bool HasRequiredAttribute(this SyntaxNodeAnalysisContext context, string name = "StatusPlaceholder")
            => HasRequiredAttribute(GetClassDeclaration(context), name);

        public static bool TryGetClassDeclaration(this SyntaxNodeAnalysisContext context, out ClassDeclarationSyntax declaration)
        {
            declaration = context.Node as ClassDeclarationSyntax;
            return declaration != null;
        }
        public static ClassDeclarationSyntax GetClassDeclaration(this SyntaxNodeAnalysisContext context)
        {
            if (TryGetClassDeclaration(context, out ClassDeclarationSyntax declaration))
                return declaration;
            throw new ArgumentException("Context node isn't a class declaration syntax", nameof(context));
        }
    }
}
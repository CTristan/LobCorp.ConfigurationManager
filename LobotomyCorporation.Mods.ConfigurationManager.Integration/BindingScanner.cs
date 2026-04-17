// SPDX-License-Identifier: LGPL-3.0-or-later

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LobotomyCorporation.Mods.ConfigurationManager.Integration
{
    /// <summary>
    /// Text-level syntax scan for <c>Config.Bind(...)</c> invocations. Uses identifier
    /// name matching rather than symbol resolution — aliasing <c>Config</c> via a using
    /// directive is intentionally unsupported (documented in the package README).
    /// </summary>
    internal static class BindingScanner
    {
        public static bool IsBindCandidate(SyntaxNode node)
        {
            if (node is not InvocationExpressionSyntax invocation)
            {
                return false;
            }

            if (invocation.Expression is not MemberAccessExpressionSyntax member)
            {
                return false;
            }

            if (member.Name.Identifier.ValueText != "Bind")
            {
                return false;
            }

            if (member.Expression is not IdentifierNameSyntax typeName)
            {
                return false;
            }

            return typeName.Identifier.ValueText == "Config";
        }

        public static BindingModel? TryExtract(
            InvocationExpressionSyntax invocation,
            SemanticModel semanticModel
        )
        {
            var arguments = invocation.ArgumentList.Arguments;
            if (arguments.Count < 3)
            {
                return null;
            }

            var section = TryGetStringLiteral(arguments[0].Expression);
            var key = TryGetStringLiteral(arguments[1].Expression);
            if (section is null || key is null)
            {
                return null;
            }

            var defaultValueExpression = arguments[2].Expression;
            var typeInfo = semanticModel.GetTypeInfo(defaultValueExpression);
            var valueType = typeInfo.ConvertedType ?? typeInfo.Type;
            if (valueType is null || valueType.TypeKind == TypeKind.Error)
            {
                return null;
            }

            var containingType = FindContainingType(invocation);
            if (containingType is null)
            {
                return null;
            }

            if (semanticModel.GetDeclaredSymbol(containingType) is not INamedTypeSymbol namedType)
            {
                return null;
            }

            var namedArguments = new Dictionary<string, string>(StringComparer.Ordinal);
            for (var i = 3; i < arguments.Count; i++)
            {
                var arg = arguments[i];
                if (arg.NameColon is null)
                {
                    continue;
                }

                var name = arg.NameColon.Name.Identifier.ValueText;
                namedArguments[name] = arg.Expression.ToString();
            }

            return new BindingModel(
                containingTypeFullName: namedType.ToDisplayString(
                    SymbolDisplayFormat.FullyQualifiedFormat
                ),
                section: section,
                key: key,
                valueTypeFullName: valueType.ToDisplayString(
                    SymbolDisplayFormat.FullyQualifiedFormat
                ),
                defaultValueLiteral: defaultValueExpression.ToString(),
                namedArguments: namedArguments,
                location: invocation.GetLocation()
            );
        }

        private static string? TryGetStringLiteral(ExpressionSyntax expression)
        {
            if (
                expression is LiteralExpressionSyntax literal
                && literal.IsKind(SyntaxKind.StringLiteralExpression)
            )
            {
                return literal.Token.ValueText;
            }

            return null;
        }

        private static TypeDeclarationSyntax? FindContainingType(SyntaxNode node)
        {
            var current = node.Parent;
            while (current is not null)
            {
                if (current is TypeDeclarationSyntax typeDecl)
                {
                    return typeDecl;
                }

                current = current.Parent;
            }

            return null;
        }
    }

    /// <summary>
    /// Syntax scan for the <c>[assembly: ConfigManagerMod(...)]</c> marker attribute.
    /// </summary>
    internal static class ModAttributeScanner
    {
        public static bool IsCandidate(SyntaxNode node)
        {
            if (node is not AttributeSyntax attribute)
            {
                return false;
            }

            var name = attribute.Name switch
            {
                IdentifierNameSyntax id => id.Identifier.ValueText,
                QualifiedNameSyntax q => q.Right.Identifier.ValueText,
                _ => null,
            };

            return name is "ConfigManagerMod" or "ConfigManagerModAttribute";
        }

        public static ModAttributeModel? TryExtract(
            AttributeSyntax attribute,
            SemanticModel semanticModel
        )
        {
            if (attribute.Parent is not AttributeListSyntax attrList)
            {
                return null;
            }

            if (attrList.Target?.Identifier.ValueText != "assembly")
            {
                return null;
            }

            string? modId = null;
            string? modName = null;
            string? modVersion = null;
            var fallback = "InMemory";

            if (attribute.ArgumentList is null)
            {
                return null;
            }

            foreach (var arg in attribute.ArgumentList.Arguments)
            {
                var name = arg.NameEquals?.Name.Identifier.ValueText;
                if (name is null)
                {
                    continue;
                }

                var constant = semanticModel.GetConstantValue(arg.Expression);
                var value = constant.HasValue
                    ? constant.Value?.ToString()
                    : arg.Expression.ToString();

                switch (name)
                {
                    case "ModId":
                        modId = value;
                        break;
                    case "ModName":
                        modName = value;
                        break;
                    case "ModVersion":
                        modVersion = value;
                        break;
                    case "Fallback":
                        fallback = StripEnumPrefix(arg.Expression.ToString());
                        break;
                }
            }

            if (string.IsNullOrEmpty(modId) || string.IsNullOrEmpty(modName))
            {
                return null;
            }

            return new ModAttributeModel(
                modId!,
                modName!,
                modVersion ?? string.Empty,
                fallback,
                attribute.GetLocation()
            );
        }

        private static string StripEnumPrefix(string expression)
        {
            var dot = expression.LastIndexOf('.');
            return dot >= 0 ? expression.Substring(dot + 1) : expression;
        }
    }
}

// SPDX-License-Identifier: LGPL-3.0-or-later

using AwesomeAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace LobotomyCorporation.Mods.ConfigurationManager.Integration.Tests
{
    public sealed class BindingScannerTests
    {
        [Fact]
        public void IsBindCandidate_ReturnsTrue_ForConfigDotBindInvocation()
        {
            var node = ParseExpression("Config.Bind(\"s\", \"k\", 1)");
            BindingScanner.IsBindCandidate(node).Should().BeTrue();
        }

        [Fact]
        public void IsBindCandidate_ReturnsFalse_ForDifferentReceiver()
        {
            var node = ParseExpression("Other.Bind(\"s\", \"k\", 1)");
            BindingScanner.IsBindCandidate(node).Should().BeFalse();
        }

        [Fact]
        public void IsBindCandidate_ReturnsFalse_ForDifferentMethod()
        {
            var node = ParseExpression("Config.Other(\"s\", \"k\", 1)");
            BindingScanner.IsBindCandidate(node).Should().BeFalse();
        }

        [Fact]
        public void IsBindCandidate_ReturnsFalse_ForQualifiedName()
        {
            // Aliased or fully-qualified uses are intentionally not matched (documented limitation).
            var node = ParseExpression("global::Config.Bind(\"s\", \"k\", 1)");
            BindingScanner.IsBindCandidate(node).Should().BeFalse();
        }

        [Fact]
        public void IsBindCandidate_ReturnsFalse_ForNonInvocationSyntax()
        {
            var node = SyntaxFactory.ParseExpression("42");
            BindingScanner.IsBindCandidate(node).Should().BeFalse();
        }

        [Fact]
        public void IsBindCandidate_ReturnsFalse_ForInvocationWithoutMemberAccess()
        {
            var node = (InvocationExpressionSyntax)SyntaxFactory.ParseExpression("Foo()");
            BindingScanner.IsBindCandidate(node).Should().BeFalse();
        }

        private static InvocationExpressionSyntax ParseExpression(string expression)
        {
            return (InvocationExpressionSyntax)SyntaxFactory.ParseExpression(expression);
        }
    }
}

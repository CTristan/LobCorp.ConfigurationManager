// SPDX-License-Identifier: LGPL-3.0-or-later

using AwesomeAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace LobotomyCorporation.Mods.ConfigurationManager.Integration.Tests
{
    public sealed class ModAttributeScannerTests
    {
        [Fact]
        public void IsCandidate_ReturnsTrue_ForConfigManagerMod()
        {
            var node = ParseAttribute("ConfigManagerMod(ModId = \"x\", ModName = \"y\")");
            ModAttributeScanner.IsCandidate(node).Should().BeTrue();
        }

        [Fact]
        public void IsCandidate_ReturnsTrue_ForConfigManagerModAttribute()
        {
            var node = ParseAttribute("ConfigManagerModAttribute(ModId = \"x\", ModName = \"y\")");
            ModAttributeScanner.IsCandidate(node).Should().BeTrue();
        }

        [Fact]
        public void IsCandidate_ReturnsTrue_ForQualifiedName()
        {
            var node = ParseAttribute(
                "LobotomyCorporation.Mods.ConfigurationManager.ConfigManagerMod(ModId = \"x\")"
            );
            ModAttributeScanner.IsCandidate(node).Should().BeTrue();
        }

        [Fact]
        public void IsCandidate_ReturnsFalse_ForUnrelatedAttribute()
        {
            var node = ParseAttribute("Obsolete");
            ModAttributeScanner.IsCandidate(node).Should().BeFalse();
        }

        [Fact]
        public void IsCandidate_ReturnsFalse_ForNonAttributeSyntax()
        {
            var node = SyntaxFactory.ParseExpression("42");
            ModAttributeScanner.IsCandidate(node).Should().BeFalse();
        }

        private static AttributeSyntax ParseAttribute(string attributeBody)
        {
            var tree = CSharpSyntaxTree.ParseText($"[assembly: {attributeBody}]");
            var root = tree.GetCompilationUnitRoot();
            return root.AttributeLists[0].Attributes[0];
        }
    }
}

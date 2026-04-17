// SPDX-License-Identifier: LGPL-3.0-or-later

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VerifyTests;

namespace LobotomyCorporation.Mods.ConfigurationManager.Integration.Tests
{
    internal static class TestHelper
    {
        internal static readonly VerifySettings VerifySettings = CreateSettings();

        public static GeneratorDriverRunResult RunGenerator(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
            };
            var compilation = CSharpCompilation.Create(
                "TestMod",
                [syntaxTree],
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            var generator = new ConfigurationManagerGenerator();
            var driver = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation);
            return driver.GetRunResult();
        }

        private static VerifySettings CreateSettings()
        {
            VerifySourceGenerators.Initialize();
            var settings = new VerifySettings();
            settings.UseDirectory("Snapshots");
            return settings;
        }
    }
}

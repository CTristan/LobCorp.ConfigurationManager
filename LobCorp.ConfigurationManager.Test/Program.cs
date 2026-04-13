// SPDX-License-Identifier: LGPL-3.0-or-later

// Custom entry point for xUnit v3. Auto-generation is disabled because
// Common's net35 assembly contains an ExcludeFromCodeCoverageAttribute polyfill
// that conflicts with the BCL type, causing CS0433 in auto-generated code.

using System.Linq;

namespace LobCorp.ConfigurationManager.Test
{
    internal static class Program
    {
        public static int Main(string[] args)
        {
            if (args.Any(arg => arg is "--server" or "--internal-msbuild-node"))
            {
                return Xunit
                    .MicrosoftTestingPlatform.TestPlatformTestFramework.RunAsync(
                        args,
                        AddSelfRegisteredExtensions
                    )
                    .GetAwaiter()
                    .GetResult();
            }

            return Xunit
                .Runner.InProc.SystemConsole.ConsoleRunner.Run(args)
                .GetAwaiter()
                .GetResult();
        }

        private static void AddSelfRegisteredExtensions(
            Microsoft.Testing.Platform.Builder.ITestApplicationBuilder builder,
            string[] args
        )
        {
            Microsoft.Testing.Platform.MSBuild.TestingPlatformBuilderHook.AddExtensions(
                builder,
                args
            );
            Microsoft.Testing.Extensions.Telemetry.TestingPlatformBuilderHook.AddExtensions(
                builder,
                args
            );
        }
    }
}

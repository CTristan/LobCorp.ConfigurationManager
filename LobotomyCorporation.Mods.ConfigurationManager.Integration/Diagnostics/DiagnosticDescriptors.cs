// SPDX-License-Identifier: LGPL-3.0-or-later

using Microsoft.CodeAnalysis;

namespace LobotomyCorporation.Mods.ConfigurationManager.Integration.Diagnostics
{
    internal static class DiagnosticDescriptors
    {
        private const string Category = "ConfigurationManager.Integration";

        public static readonly DiagnosticDescriptor DuplicateBinding = new(
            id: "LCM001",
            title: "Duplicate config binding",
            messageFormat: "A Config.Bind for section '{0}' key '{1}' is defined more than once; only the first will be registered",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor DuplicateModAttribute = new(
            id: "LCM002",
            title: "Duplicate ConfigManagerMod attribute",
            messageFormat: "More than one [assembly: ConfigManagerMod] attribute was found; only the first is honored",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor MissingModAttribute = new(
            id: "LCM003",
            title: "Missing ConfigManagerMod attribute",
            messageFormat: "No [assembly: ConfigManagerMod] attribute was found; Config.RegisterAll will use the assembly name as a fallback",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true
        );
    }
}

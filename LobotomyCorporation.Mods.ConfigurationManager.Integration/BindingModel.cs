// SPDX-License-Identifier: LGPL-3.0-or-later

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace LobotomyCorporation.Mods.ConfigurationManager.Integration
{
    /// <summary>
    /// Immutable description of a single <c>Config.Bind</c> invocation discovered by the scanner.
    /// Equality is value-based so the incremental generator can deduplicate cache entries.
    /// Location is intentionally excluded from equality — two identical bindings at different
    /// call sites should still be treated as structurally equal.
    /// </summary>
    /// <param name="containingTypeFullName">Fully-qualified name of the type declaring the binding.</param>
    /// <param name="section">Config section name the binding lives under.</param>
    /// <param name="key">Config key within the section.</param>
    /// <param name="valueTypeFullName">Fully-qualified CLR type of the bound value.</param>
    /// <param name="defaultValueLiteral">Textual form of the default value expression.</param>
    /// <param name="namedArguments">Named arguments (UI hints) supplied to Bind.</param>
    /// <param name="location">Source location of the Bind invocation for diagnostics.</param>
    internal sealed class BindingModel(
        string containingTypeFullName,
        string section,
        string key,
        string valueTypeFullName,
        string defaultValueLiteral,
        IReadOnlyDictionary<string, string> namedArguments,
        Location location
    ) : IEquatable<BindingModel>
    {
        public string ContainingTypeFullName { get; } = containingTypeFullName;
        public string Section { get; } = section;
        public string Key { get; } = key;
        public string ValueTypeFullName { get; } = valueTypeFullName;
        public string DefaultValueLiteral { get; } = defaultValueLiteral;
        public IReadOnlyDictionary<string, string> NamedArguments { get; } = namedArguments;
        public Location Location { get; } = location;

        public bool Equals(BindingModel? other)
        {
            if (other is null)
            {
                return false;
            }

            if (
                ContainingTypeFullName != other.ContainingTypeFullName
                || Section != other.Section
                || Key != other.Key
                || ValueTypeFullName != other.ValueTypeFullName
                || DefaultValueLiteral != other.DefaultValueLiteral
                || NamedArguments.Count != other.NamedArguments.Count
            )
            {
                return false;
            }

            foreach (var kvp in NamedArguments)
            {
                if (
                    !other.NamedArguments.TryGetValue(kvp.Key, out var otherVal)
                    || otherVal != kvp.Value
                )
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as BindingModel);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = (hash * 31) ^ ContainingTypeFullName.GetHashCode();
                hash = (hash * 31) ^ Section.GetHashCode();
                hash = (hash * 31) ^ Key.GetHashCode();
                hash = (hash * 31) ^ ValueTypeFullName.GetHashCode();
                hash = (hash * 31) ^ DefaultValueLiteral.GetHashCode();
                hash = (hash * 31) ^ NamedArguments.Count;
                return hash;
            }
        }
    }

    /// <summary>
    /// Model for a discovered <c>[assembly: ConfigManagerMod(...)]</c> attribute.
    /// </summary>
    /// <param name="modId">Unique mod identifier.</param>
    /// <param name="modName">Human-readable mod display name.</param>
    /// <param name="modVersion">Optional mod version string.</param>
    /// <param name="fallback">Behavior when ConfigurationManager is not installed.</param>
    /// <param name="location">Source location of the attribute for diagnostics.</param>
    internal sealed class ModAttributeModel(
        string modId,
        string modName,
        string modVersion,
        string fallback,
        Location location
    ) : IEquatable<ModAttributeModel>
    {
        public string ModId { get; } = modId;
        public string ModName { get; } = modName;
        public string ModVersion { get; } = modVersion;
        public string Fallback { get; } = fallback;
        public Location Location { get; } = location;

        public bool Equals(ModAttributeModel? other)
        {
            if (other is null)
            {
                return false;
            }

            return ModId == other.ModId
                && ModName == other.ModName
                && ModVersion == other.ModVersion
                && Fallback == other.Fallback;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as ModAttributeModel);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = (hash * 31) ^ (ModId?.GetHashCode() ?? 0);
                hash = (hash * 31) ^ (ModName?.GetHashCode() ?? 0);
                hash = (hash * 31) ^ (ModVersion?.GetHashCode() ?? 0);
                hash = (hash * 31) ^ (Fallback?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}

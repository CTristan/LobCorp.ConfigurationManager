// SPDX-License-Identifier: LGPL-3.0-or-later

// net35 BCL polyfill: System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute was
// added in .NET 4.0 but this assembly targets net35. Providing the type locally lets the
// rest of the codebase annotate coverage-excluded members without pulling in a dependency
// solely for this attribute.

#pragma warning disable IDE0130 // Namespace must match BCL location, not folder path
namespace System.Diagnostics.CodeAnalysis
#pragma warning restore IDE0130
{
    [AttributeUsage(
        AttributeTargets.Class
            | AttributeTargets.Struct
            | AttributeTargets.Constructor
            | AttributeTargets.Method
            | AttributeTargets.Property
            | AttributeTargets.Event,
        Inherited = false,
        AllowMultiple = false
    )]
    internal sealed class ExcludeFromCodeCoverageAttribute : Attribute
    {
        public string Justification { get; set; }
    }
}

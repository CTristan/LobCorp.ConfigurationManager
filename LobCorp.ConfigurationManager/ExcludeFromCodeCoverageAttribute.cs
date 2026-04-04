// SPDX-License-Identifier: LGPL-3.0-or-later

#if !NETCOREAPP && !NET5_0_OR_GREATER

#pragma warning disable IDE0130 // Compatibility shim must live in the BCL namespace.
// ReSharper disable once CheckNamespace
namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    internal sealed class ExcludeFromCodeCoverageAttribute : Attribute
    {
        public string Justification { get; set; }
    }
}

#endif

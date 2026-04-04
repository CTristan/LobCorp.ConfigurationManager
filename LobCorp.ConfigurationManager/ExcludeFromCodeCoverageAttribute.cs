// SPDX-License-Identifier: MIT

#if !NETCOREAPP && !NET5_0_OR_GREATER

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

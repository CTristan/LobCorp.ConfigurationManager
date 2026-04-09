// SPDX-License-Identifier: LGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace ConfigurationManager.Utilities
{
    [ExcludeFromCodeCoverage(Justification = "Thin wrapper around Debug.Log")]
    internal static class SimpleLogger
    {
        private const string Prefix = "[ConfigurationManager] ";

        public static void Log(object message)
        {
            Debug.Log(Prefix + message);
        }

        public static void LogWarning(object message)
        {
            Debug.LogWarning(Prefix + message);
        }

        public static void LogError(object message)
        {
            Debug.LogError(Prefix + message);
        }
    }
}

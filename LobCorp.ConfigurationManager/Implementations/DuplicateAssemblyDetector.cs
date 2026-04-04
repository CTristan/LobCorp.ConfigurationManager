// SPDX-License-Identifier: LGPL-3.0-or-later

using System;

namespace ConfigurationManager.Implementations
{
    /// <summary>
    /// Detects whether a second ConfigurationManager assembly (the original BepInEx version)
    /// is loaded alongside this fork. Both can load simultaneously because they use different
    /// loading mechanisms (LMM BaseMods vs BepInEx Chainloader).
    /// </summary>
    public static class DuplicateAssemblyDetector
    {
        private static bool _checked;
        private static bool _isDuplicateLoaded;
        private static string _duplicateLocation;

        /// <summary>
        /// Returns true if a second assembly named ConfigurationManager is loaded
        /// that is not this fork's assembly.
        /// </summary>
        public static bool IsDuplicateLoaded
        {
            get
            {
                EnsureChecked();
                return _isDuplicateLoaded;
            }
        }

        /// <summary>
        /// Returns the file path of the duplicate assembly, or an empty string if none found.
        /// </summary>
        public static string DuplicateLocation
        {
            get
            {
                EnsureChecked();
                return _duplicateLocation;
            }
        }

        private static void EnsureChecked()
        {
            if (_checked)
            {
                return;
            }

            _checked = true;
            _duplicateLocation = string.Empty;

            var ourAssembly = typeof(Harmony_Patch).Assembly;
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (var i = 0; i < allAssemblies.Length; i++)
            {
                var assembly = allAssemblies[i];
                if (assembly.GetName().Name == "ConfigurationManager" && assembly != ourAssembly)
                {
                    _isDuplicateLoaded = true;
                    _duplicateLocation = assembly.Location ?? string.Empty;
                    return;
                }
            }
        }
    }
}

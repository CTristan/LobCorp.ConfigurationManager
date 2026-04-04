// SPDX-License-Identifier: LGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace ConfigurationManager.Implementations
{
    /// <summary>
    /// MonoBehaviour that provides Unity lifecycle hooks (Update, LateUpdate, OnGUI)
    /// for the ConfigurationManager. Injected via Harmony patch on IntroPlayer.Awake.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "MonoBehaviour lifecycle wrapper")]
    public sealed class ConfigManagerBehaviour : MonoBehaviour
    {
        private static bool _warningDismissed;

        private void OnGUI()
        {
            if (_warningDismissed || !DuplicateAssemblyDetector.IsDuplicateLoaded)
            {
                return;
            }

            const float boxWidth = 500f;
            const float boxHeight = 200f;
            var rect = new Rect(
                (Screen.width - boxWidth) / 2f,
                (Screen.height - boxHeight) / 2f,
                boxWidth,
                boxHeight
            );

            GUI.Box(rect, "Duplicate ConfigurationManager Detected");

            var contentRect = new Rect(
                rect.x + 10f,
                rect.y + 30f,
                rect.width - 20f,
                rect.height - 80f
            );

            GUI.Label(
                contentRect,
                "Both the LobCorp ConfigurationManager and the original BepInEx ConfigurationManager "
                    + "are loaded. This causes double UI entries and duplicate settings processing.\n\n"
                    + "Remove the original from BepInEx/plugins/ to fix this.\n\n"
                    + "Duplicate location: "
                    + DuplicateAssemblyDetector.DuplicateLocation
            );

            var buttonRect = new Rect(
                rect.x + (rect.width - 100f) / 2f,
                rect.y + rect.height - 40f,
                100f,
                30f
            );

            if (GUI.Button(buttonRect, "Dismiss"))
            {
                _warningDismissed = true;
            }
        }
    }
}

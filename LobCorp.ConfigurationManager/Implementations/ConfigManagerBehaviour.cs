// SPDX-License-Identifier: LGPL-3.0-or-later

using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace ConfigurationManager
{
    /// <summary>
    /// MonoBehaviour that provides Unity lifecycle hooks (Update, LateUpdate, OnGUI)
    /// for the ConfigurationManager. Injected via Harmony patch on IntroPlayer.Awake.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "MonoBehaviour lifecycle wrapper")]
    public sealed class ConfigManagerBehaviour : MonoBehaviour;
}

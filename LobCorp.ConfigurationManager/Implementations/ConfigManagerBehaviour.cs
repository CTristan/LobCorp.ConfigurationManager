using System;
using UnityEngine;

namespace ConfigurationManager
{
    /// <summary>
    /// MonoBehaviour that provides Unity lifecycle hooks (Update, LateUpdate, OnGUI)
    /// for the ConfigurationManager. Injected via Harmony patch on IntroPlayer.Awake.
    /// </summary>
    public sealed class ConfigManagerBehaviour : MonoBehaviour;
}

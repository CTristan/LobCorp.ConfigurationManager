using System;
using UnityEngine;

namespace ConfigurationManager
{
    /// <summary>
    /// MonoBehaviour that provides Unity lifecycle hooks (Update, LateUpdate, OnGUI)
    /// for the ConfigurationManager. Injected via Harmony patch on IntroPlayer.Awake.
    /// </summary>
    public sealed class ConfigManagerBehaviour : MonoBehaviour
    {
        private ConfigurationManager _manager;

        private void Awake()
        {
            try
            {
                DontDestroyOnLoad(gameObject);
                _manager = new ConfigurationManager();
                _manager.Start();
            }
            catch (Exception ex)
            {
                Debug.LogError("[ConfigurationManager] Failed to initialize: " + ex);
            }
        }

        private void Update()
        {
            try
            {
                if (_manager != null)
                    _manager.Update();
            }
            catch (Exception ex)
            {
                Debug.LogError("[ConfigurationManager] Error in Update: " + ex);
            }
        }

        private void LateUpdate()
        {
            try
            {
                if (_manager != null)
                    _manager.LateUpdate();
            }
            catch (Exception ex)
            {
                Debug.LogError("[ConfigurationManager] Error in LateUpdate: " + ex);
            }
        }

        private void OnGUI()
        {
            try
            {
                if (_manager != null)
                    _manager.OnGUI();
            }
            catch (Exception ex)
            {
                Debug.LogError("[ConfigurationManager] Error in OnGUI: " + ex);
            }
        }
    }
}

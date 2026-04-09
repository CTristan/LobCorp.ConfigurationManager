// SPDX-License-Identifier: LGPL-3.0-or-later

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ConfigurationManager.Input;
using UnityEngine;

namespace ConfigurationManager.Implementations
{
    [ExcludeFromCodeCoverage(Justification = "ImGUI keyboard shortcut drawing")]
    internal static class KeyboardShortcutDrawer
    {
        private static IEnumerable<KeyCode> _keysToCheck;
        private static SettingEntryBase _currentKeyboardShortcutToSet;

        public static bool SettingKeyboardShortcut => _currentKeyboardShortcutToSet != null;

        public static void DrawKeyCode(SettingEntryBase setting, float windowYmax)
        {
            if (ReferenceEquals(_currentKeyboardShortcutToSet, setting))
            {
                GUILayout.Label("Press any key", GUILayout.ExpandWidth(true));
                GUIUtility.keyboardControl = -1;

                if (_keysToCheck == null)
                {
                    _keysToCheck = KeyboardShortcut.ModifierBlockKeyCodes;
                }

                foreach (var key in _keysToCheck)
                {
                    if (global::UnityEngine.Input.GetKeyUp(key))
                    {
                        setting.Set(key);
                        _currentKeyboardShortcutToSet = null;
                        break;
                    }
                }

                if (GUILayout.Button("Cancel", GUILayout.ExpandWidth(false)))
                {
                    _currentKeyboardShortcutToSet = null;
                }
            }
            else
            {
                var acceptableValues =
                    setting.AcceptableValues?.Count > 1
                        ? (System.Collections.IList)setting.AcceptableValues
                        : System.Enum.GetValues(setting.SettingType);
                ComboboxDrawer.DrawComboboxField(setting, acceptableValues, windowYmax);

                if (
                    GUILayout.Button(
                        new GUIContent(
                            "Set...",
                            null,
                            "Set the key by pressing any key on your keyboard."
                        ),
                        GUILayout.ExpandWidth(false)
                    )
                )
                {
                    _currentKeyboardShortcutToSet = setting;
                }
            }
        }

        public static void DrawKeyboardShortcut(SettingEntryBase setting)
        {
            if (ReferenceEquals(_currentKeyboardShortcutToSet, setting))
            {
                GUILayout.Label("Press any key combination", GUILayout.ExpandWidth(true));
                GUIUtility.keyboardControl = -1;

                if (_keysToCheck == null)
                {
                    _keysToCheck = KeyboardShortcut.ModifierBlockKeyCodes;
                }

                foreach (var key in _keysToCheck)
                {
                    if (global::UnityEngine.Input.GetKeyUp(key))
                    {
                        setting.Set(
                            new KeyboardShortcut(
                                key,
                                _keysToCheck
                                    .Where(x => global::UnityEngine.Input.GetKey(x))
                                    .ToArray()
                            )
                        );
                        _currentKeyboardShortcutToSet = null;
                        break;
                    }
                }

                if (GUILayout.Button("Cancel", GUILayout.ExpandWidth(false)))
                {
                    _currentKeyboardShortcutToSet = null;
                }
            }
            else
            {
                if (GUILayout.Button(setting.GetValue().ToString(), GUILayout.ExpandWidth(true)))
                {
                    _currentKeyboardShortcutToSet = setting;
                }

                if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false)))
                {
                    setting.Set(KeyboardShortcut.Empty);
                    _currentKeyboardShortcutToSet = null;
                }
            }
        }

        public static bool IsSettingBeingSet(SettingEntryBase setting)
        {
            return ReferenceEquals(_currentKeyboardShortcutToSet, setting);
        }

        public static void SetCurrentShortcutTarget(SettingEntryBase setting)
        {
            _currentKeyboardShortcutToSet = setting;
        }
    }
}

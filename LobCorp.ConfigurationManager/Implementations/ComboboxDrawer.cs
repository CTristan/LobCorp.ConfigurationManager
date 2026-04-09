// SPDX-License-Identifier: LGPL-3.0-or-later

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using ConfigurationManager.Utilities;
using UnityEngine;

namespace ConfigurationManager.Implementations
{
    [ExcludeFromCodeCoverage(Justification = "ImGUI combobox drawing")]
    internal static class ComboboxDrawer
    {
        private static readonly Dictionary<SettingEntryBase, ComboBox> _comboBoxCache =
            new Dictionary<SettingEntryBase, ComboBox>();

        public static void ClearCache()
        {
            _comboBoxCache.Clear();
        }

        public static bool DrawCurrentDropdown()
        {
            if (ComboBox.CurrentDropdownDrawer != null)
            {
                ComboBox.CurrentDropdownDrawer.Invoke();
                ComboBox.CurrentDropdownDrawer = null;
                return true;
            }
            return false;
        }

        public static void DrawListField(SettingEntryBase setting, float windowYmax)
        {
            var acceptableValues = setting.AcceptableValues;
            if (acceptableValues.Count == 0)
            {
                throw new ArgumentException(
                    "AcceptableValueListAttribute returned an empty list of acceptable values. You need to supply at least 1 option."
                );
            }

            if (
                !setting.SettingType.IsInstanceOfType(
                    acceptableValues.Cast<object>().FirstOrDefault(x => x != null)
                )
            )
            {
                throw new ArgumentException(
                    "AcceptableValueListAttribute returned a list with items of type other than the setting type itself."
                );
            }

            if (setting.SettingType == typeof(KeyCode))
            {
                KeyboardShortcutDrawer.DrawKeyCode(setting, windowYmax);
            }
            else
            {
                DrawComboboxField(setting, (IList)acceptableValues, windowYmax);
            }
        }

        public static void DrawEnumField(
            SettingEntryBase setting,
            int rightColumnWidth,
            float windowYmax
        )
        {
            if (setting.SettingType.GetCustomAttributes(typeof(FlagsAttribute), false).Length != 0)
            {
                DrawFlagsField(setting, Enum.GetValues(setting.SettingType), rightColumnWidth);
            }
            else
            {
                DrawComboboxField(setting, Enum.GetValues(setting.SettingType), windowYmax);
            }
        }

        public static void DrawComboboxField(SettingEntryBase setting, IList list, float windowYmax)
        {
            var buttonText = ObjectToGuiContent(setting.GetValue());
            var dispRect = GUILayoutUtility.GetRect(
                buttonText,
                GUI.skin.button,
                GUILayout.ExpandWidth(true)
            );

            if (!_comboBoxCache.TryGetValue(setting, out var box))
            {
                box = new ComboBox(
                    dispRect,
                    buttonText,
                    list.Cast<object>().Select(ObjectToGuiContent).ToArray(),
                    GUI.skin.button,
                    windowYmax
                );
                _comboBoxCache[setting] = box;
            }
            else
            {
                box.Rect = dispRect;
                box.ButtonContent = buttonText;
            }

            box.Show(id =>
            {
                if (id >= 0 && id < list.Count)
                {
                    setting.Set(list[id]);
                }
            });
        }

        private static void DrawFlagsField(SettingEntryBase setting, IList enumValues, int maxWidth)
        {
            var currentValue = Convert.ToInt64(setting.GetValue(), CultureInfo.InvariantCulture);
            var allValues = enumValues
                .Cast<Enum>()
                .Select(x => new
                {
                    name = x.ToString(),
                    val = Convert.ToInt64(x, CultureInfo.InvariantCulture),
                })
                .ToArray();

            GUILayout.BeginVertical(GUILayout.MaxWidth(maxWidth));
            {
                for (var index = 0; index < allValues.Length; )
                {
                    GUILayout.BeginHorizontal();
                    {
                        var currentWidth = 0;
                        for (; index < allValues.Length; index++)
                        {
                            var value = allValues[index];

                            if (value.val != 0)
                            {
                                var textDimension = (int)
                                    GUI.skin.toggle.CalcSize(new GUIContent(value.name)).x;
                                currentWidth += textDimension;
                                if (currentWidth > maxWidth)
                                {
                                    break;
                                }

                                GUI.changed = false;
                                var newVal = GUILayout.Toggle(
                                    (currentValue & value.val) == value.val,
                                    value.name,
                                    GUILayout.ExpandWidth(false)
                                );
                                if (GUI.changed)
                                {
                                    var newValue = newVal
                                        ? currentValue | value.val
                                        : currentValue & ~value.val;
                                    setting.Set(Enum.ToObject(setting.SettingType, newValue));
                                }
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                GUI.changed = false;
            }
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
        }

        private static GUIContent ObjectToGuiContent(object x)
        {
            if (x is Enum)
            {
                var enumType = x.GetType();
                var enumMember = enumType.GetMember(x.ToString()).FirstOrDefault();
                var attr = enumMember
                    ?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .Cast<DescriptionAttribute>()
                    .FirstOrDefault();
                if (attr != null)
                {
                    return new GUIContent(attr.Description);
                }

                return new GUIContent(x.ToString().ToProperCase());
            }
            return new GUIContent(x.ToString());
        }
    }
}

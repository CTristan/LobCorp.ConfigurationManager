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
                    acceptableValues.FirstOrDefault(x => x != null)
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
            var buttonText = ObjectToGuiContent(setting.GetValue(), setting.ValueDisplayNames);
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
                    list.Cast<object>()
                        .Select(v => ObjectToGuiContent(v, setting.ValueDisplayNames))
                        .ToArray(),
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

        private readonly struct FlagEntry
        {
            public string Name { get; }
            public long Value { get; }

            public FlagEntry(string name, long value)
            {
                Name = name;
                Value = value;
            }
        }

        private static void DrawFlagsField(SettingEntryBase setting, IList enumValues, int maxWidth)
        {
            var currentValue = Convert.ToInt64(setting.GetValue(), CultureInfo.InvariantCulture);
            var allValues = enumValues
                .Cast<Enum>()
                .Select(x => new FlagEntry(
                    GetValueDisplayName(x, setting.ValueDisplayNames),
                    Convert.ToInt64(x, CultureInfo.InvariantCulture)
                ))
                .ToArray();

            GUILayout.BeginVertical(GUILayout.MaxWidth(maxWidth));
            DrawFlagsRows(setting, allValues, currentValue, maxWidth);
            GUI.changed = false;
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
        }

        private static void DrawFlagsRows(
            SettingEntryBase setting,
            FlagEntry[] allValues,
            long currentValue,
            int maxWidth
        )
        {
            for (var index = 0; index < allValues.Length; )
            {
                GUILayout.BeginHorizontal();
                DrawFlagsRow(setting, allValues, currentValue, maxWidth, ref index);
                GUILayout.EndHorizontal();
            }
        }

        private static void DrawFlagsRow(
            SettingEntryBase setting,
            FlagEntry[] allValues,
            long currentValue,
            int maxWidth,
            ref int index
        )
        {
            var currentWidth = 0;
            for (; index < allValues.Length; index++)
            {
                var value = allValues[index];

                if (value.Value == 0)
                {
                    continue;
                }

                var textDimension = (int)GUI.skin.toggle.CalcSize(new GUIContent(value.Name)).x;
                currentWidth += textDimension;
                if (currentWidth > maxWidth)
                {
                    break;
                }

                GUI.changed = false;
                var newVal = GUILayout.Toggle(
                    (currentValue & value.Value) == value.Value,
                    value.Name,
                    GUILayout.ExpandWidth(false)
                );
                if (GUI.changed)
                {
                    var newValue = newVal
                        ? currentValue | value.Value
                        : currentValue & ~value.Value;
                    setting.Set(Enum.ToObject(setting.SettingType, newValue));
                }
            }
        }

        private static GUIContent ObjectToGuiContent(
            object x,
            IDictionary<string, string> valueDisplayNames = null
        )
        {
            if (
                valueDisplayNames != null
                && valueDisplayNames.TryGetValue(x.ToString(), out var displayName)
            )
            {
                return new GUIContent(displayName);
            }

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

        private static string GetValueDisplayName(
            Enum x,
            IDictionary<string, string> valueDisplayNames
        )
        {
            if (
                valueDisplayNames != null
                && valueDisplayNames.TryGetValue(x.ToString(), out var displayName)
            )
            {
                return displayName;
            }

            return x.ToString();
        }
    }
}

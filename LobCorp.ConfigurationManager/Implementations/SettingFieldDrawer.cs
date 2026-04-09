// SPDX-License-Identifier: LGPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using ConfigurationManager.Input;
using ConfigurationManager.Utilities;
using UnityEngine;

namespace ConfigurationManager.Implementations
{
    [ExcludeFromCodeCoverage(Justification = "ImGUI field drawing")]
    internal sealed class SettingFieldDrawer
    {
        public static Dictionary<Type, Action<SettingEntryBase>> SettingDrawHandlers { get; }

        private static SettingsWindowController _instance;

        public static bool SettingKeyboardShortcut =>
            KeyboardShortcutDrawer.SettingKeyboardShortcut;

        static SettingFieldDrawer()
        {
            SettingDrawHandlers = new Dictionary<Type, Action<SettingEntryBase>>
            {
                { typeof(bool), DrawBoolField },
                { typeof(KeyboardShortcut), KeyboardShortcutDrawer.DrawKeyboardShortcut },
                {
                    typeof(KeyCode),
                    s => KeyboardShortcutDrawer.DrawKeyCode(s, _instance.SettingWindowRect.yMax)
                },
                { typeof(Color), ColorFieldDrawer.DrawColor },
                { typeof(Vector2), DrawVector2 },
                { typeof(Vector3), DrawVector3 },
                { typeof(Vector4), DrawVector4 },
                { typeof(Quaternion), DrawQuaternion },
            };
        }

        public SettingFieldDrawer(SettingsWindowController instance)
        {
            _instance = instance;
        }

        public void DrawSettingValue(SettingEntryBase setting)
        {
            if (setting.CustomDrawer != null)
            {
                var configEntry = setting is LmmSettingEntry lmmEntry ? lmmEntry.Entry : null;
                setting.CustomDrawer(configEntry);
            }
            else if (setting.CustomHotkeyDrawer != null)
            {
                var isBeingSet = KeyboardShortcutDrawer.IsSettingBeingSet(setting);
                var isBeingSetOriginal = isBeingSet;

                var configEntry = setting is LmmSettingEntry lmmEntry ? lmmEntry.Entry : null;
                setting.CustomHotkeyDrawer(configEntry, ref isBeingSet);

                if (isBeingSet != isBeingSetOriginal)
                {
                    KeyboardShortcutDrawer.SetCurrentShortcutTarget(isBeingSet ? setting : null);
                }
            }
            else if (setting.ShowRangeAsPercent != null && setting.AcceptableValueRange.Key != null)
            {
                DrawRangeField(setting);
            }
            else if (setting.AcceptableValues != null)
            {
                ComboboxDrawer.DrawListField(setting, _instance.SettingWindowRect.yMax);
            }
            else if (DrawFieldBasedOnValueType(setting))
            {
                return;
            }
            else if (setting.SettingType.IsEnum)
            {
                ComboboxDrawer.DrawEnumField(
                    setting,
                    _instance.RightColumnWidth,
                    _instance.SettingWindowRect.yMax
                );
            }
            else
            {
                DrawUnknownField(setting, _instance.RightColumnWidth);
            }
        }

        public static void ClearCache()
        {
            ComboboxDrawer.ClearCache();
            ColorFieldDrawer.ClearCache();
        }

        public static void DrawCenteredLabel(string text, params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal(options);
            GUILayout.FlexibleSpace();
            GUILayout.Label(text);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private static GUIStyle _categoryHeaderSkin;

        public static void DrawCategoryHeader(string text)
        {
            if (_categoryHeaderSkin == null)
            {
                _categoryHeaderSkin = GUI.skin.label.CreateCopy();
                _categoryHeaderSkin.alignment = TextAnchor.UpperCenter;
                _categoryHeaderSkin.wordWrap = true;
                _categoryHeaderSkin.stretchWidth = true;
                _categoryHeaderSkin.fontSize = 14;
            }

            GUILayout.Label(text, _categoryHeaderSkin);
        }

        private static GUIStyle _pluginHeaderSkin;

        public static bool DrawPluginHeader(GUIContent content, bool isCollapsed)
        {
            if (_pluginHeaderSkin == null)
            {
                _pluginHeaderSkin = GUI.skin.label.CreateCopy();
                _pluginHeaderSkin.alignment = TextAnchor.UpperCenter;
                _pluginHeaderSkin.wordWrap = true;
                _pluginHeaderSkin.stretchWidth = true;
                _pluginHeaderSkin.fontSize = 15;
            }

            if (isCollapsed)
            {
                content.text += "\n...";
            }

            return GUILayout.Button(content, _pluginHeaderSkin, GUILayout.ExpandWidth(true));
        }

        public static bool DrawCurrentDropdown()
        {
            return ComboboxDrawer.DrawCurrentDropdown();
        }

        private static bool DrawFieldBasedOnValueType(SettingEntryBase setting)
        {
            if (SettingDrawHandlers.TryGetValue(setting.SettingType, out var drawMethod))
            {
                drawMethod(setting);
                return true;
            }
            return false;
        }

        private static void DrawBoolField(SettingEntryBase setting)
        {
            var boolVal = (bool)setting.GetValue();
            var result = GUILayout.Toggle(
                boolVal,
                boolVal ? "Enabled" : "Disabled",
                GUILayout.ExpandWidth(true)
            );
            if (result != boolVal)
            {
                setting.Set(result);
            }
        }

        private static void DrawRangeField(SettingEntryBase setting)
        {
            var value = setting.GetValue();
            var converted = (float)Convert.ToDouble(value, CultureInfo.InvariantCulture);
            var leftValue = (float)
                Convert.ToDouble(setting.AcceptableValueRange.Key, CultureInfo.InvariantCulture);
            var rightValue = (float)
                Convert.ToDouble(setting.AcceptableValueRange.Value, CultureInfo.InvariantCulture);

            var result = GUILayout.HorizontalSlider(
                converted,
                leftValue,
                rightValue,
                GUILayout.ExpandWidth(true)
            );
            if (setting.UseIntegerSlider == true)
            {
                result = Mathf.Round(result);
            }

            if (Math.Abs(result - converted) > Mathf.Abs(rightValue - leftValue) / 1000)
            {
                var newValue = Convert.ChangeType(
                    result,
                    setting.SettingType,
                    CultureInfo.InvariantCulture
                );
                setting.Set(newValue);
            }

            if (setting.ShowRangeAsPercent == true)
            {
                DrawCenteredLabel(
                    Mathf.Round(
                        100 * Mathf.Abs(result - leftValue) / Mathf.Abs(rightValue - leftValue)
                    ) + "%",
                    GUILayout.Width(50)
                );
            }
            else
            {
                var strVal = Convert
                    .ToString(value, CultureInfo.InvariantCulture)
                    .AppendZeroIfFloat(setting.SettingType);
                var strResult = GUILayout.TextField(strVal, GUILayout.Width(50));
                if (strResult != strVal)
                {
                    try
                    {
                        var resultVal = (float)
                            Convert.ToDouble(strResult, CultureInfo.InvariantCulture);
                        var clampedResultVal = Mathf.Clamp(resultVal, leftValue, rightValue);
                        setting.Set(
                            Convert.ChangeType(
                                clampedResultVal,
                                setting.SettingType,
                                CultureInfo.InvariantCulture
                            )
                        );
                    }
                    // User may type partial/invalid numeric input — keep previous value until valid
                    catch (FormatException) { }
                }
            }
        }

        private void DrawUnknownField(SettingEntryBase setting, int rightColumnWidth)
        {
            if (setting.ObjToStr != null && setting.StrToObj != null)
            {
                var text = setting
                    .ObjToStr(setting.GetValue())
                    .AppendZeroIfFloat(setting.SettingType);
                var result = GUILayout.TextField(
                    text,
                    GUILayout.Width(rightColumnWidth),
                    GUILayout.MaxWidth(rightColumnWidth)
                );

                if (result != text)
                {
                    setting.Set(setting.StrToObj(result));
                }
            }
            else
            {
                var rawValue = setting.GetValue();
                var value =
                    rawValue == null
                        ? "NULL"
                        : Convert
                            .ToString(rawValue, CultureInfo.InvariantCulture)
                            .AppendZeroIfFloat(setting.SettingType);
                if (CanConvert(value, setting.SettingType))
                {
                    var result = GUILayout.TextField(
                        value,
                        GUILayout.Width(rightColumnWidth),
                        GUILayout.MaxWidth(rightColumnWidth)
                    );
                    if (result != value)
                    {
                        setting.Set(
                            Convert.ChangeType(
                                result,
                                setting.SettingType,
                                CultureInfo.InvariantCulture
                            )
                        );
                    }
                }
                else
                {
                    _ = GUILayout.TextArea(value, GUILayout.MaxWidth(rightColumnWidth));
                }
            }

            GUILayout.FlexibleSpace();
        }

        private readonly Dictionary<Type, bool> _canConvertCache = new Dictionary<Type, bool>();

        private bool CanConvert(string value, Type type)
        {
            if (_canConvertCache.TryGetValue(type, out var value1))
            {
                return value1;
            }

            try
            {
                _ = Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
                _canConvertCache[type] = true;
                return true;
            }
            catch
            {
                _canConvertCache[type] = false;
                return false;
            }
        }

        private static void DrawVector2(SettingEntryBase obj)
        {
            var setting = (Vector2)obj.GetValue();
            var copy = setting;
            setting.x = DrawSingleVectorSlider(setting.x, "X");
            setting.y = DrawSingleVectorSlider(setting.y, "Y");
            if (setting != copy)
            {
                obj.Set(setting);
            }
        }

        private static void DrawVector3(SettingEntryBase obj)
        {
            var setting = (Vector3)obj.GetValue();
            var copy = setting;
            setting.x = DrawSingleVectorSlider(setting.x, "X");
            setting.y = DrawSingleVectorSlider(setting.y, "Y");
            setting.z = DrawSingleVectorSlider(setting.z, "Z");
            if (setting != copy)
            {
                obj.Set(setting);
            }
        }

        private static void DrawVector4(SettingEntryBase obj)
        {
            var setting = (Vector4)obj.GetValue();
            var copy = setting;
            setting.x = DrawSingleVectorSlider(setting.x, "X");
            setting.y = DrawSingleVectorSlider(setting.y, "Y");
            setting.z = DrawSingleVectorSlider(setting.z, "Z");
            setting.w = DrawSingleVectorSlider(setting.w, "W");
            if (setting != copy)
            {
                obj.Set(setting);
            }
        }

        private static void DrawQuaternion(SettingEntryBase obj)
        {
            var setting = (Quaternion)obj.GetValue();
            var copy = setting;
            setting.x = DrawSingleVectorSlider(setting.x, "X");
            setting.y = DrawSingleVectorSlider(setting.y, "Y");
            setting.z = DrawSingleVectorSlider(setting.z, "Z");
            setting.w = DrawSingleVectorSlider(setting.w, "W");
            if (setting != copy)
            {
                obj.Set(setting);
            }
        }

        private static float DrawSingleVectorSlider(float setting, string label)
        {
            GUILayout.Label(label, GUILayout.ExpandWidth(false));
            var text = GUILayout.TextField(
                setting.ToString("F", CultureInfo.InvariantCulture),
                GUILayout.ExpandWidth(true)
            );
            return float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var x)
                ? x
                : setting;
        }
    }
}

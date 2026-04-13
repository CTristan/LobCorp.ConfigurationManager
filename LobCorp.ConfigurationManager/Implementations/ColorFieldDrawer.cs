// SPDX-License-Identifier: LGPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using ConfigurationManager.Config;
using ConfigurationManager.Utilities;
using UnityEngine;

namespace ConfigurationManager.Implementations
{
    [ExcludeFromCodeCoverage(Justification = "ImGUI color field drawing")]
    internal static class ColorFieldDrawer
    {
        private static readonly Dictionary<SettingEntryBase, ColorCacheEntry> _colorCache =
            new Dictionary<SettingEntryBase, ColorCacheEntry>();
        private static bool _drawColorHex;

        public static void ClearCache()
        {
            foreach (var tex in _colorCache)
            {
                UnityEngine.Object.Destroy(tex.Value.Tex);
            }

            _colorCache.Clear();
        }

        public static void DrawColor(SettingEntryBase obj)
        {
            var colorValue = (Color)obj.GetValue();

            GUI.changed = false;

            if (!_colorCache.TryGetValue(obj, out var cacheEntry))
            {
                var tex = new Texture2D(100, 20, TextureFormat.ARGB32, false);
                cacheEntry = new ColorCacheEntry { Tex = tex, Last = colorValue };
                FillTex(colorValue, tex);
                _colorCache[obj] = cacheEntry;
            }

            GUILayout.BeginVertical();
            DrawTopRow(cacheEntry, ref colorValue);
            DrawRgbaSliders(ref colorValue);
            GUILayout.EndVertical();

            if (colorValue != cacheEntry.Last)
            {
                obj.Set(colorValue);
                FillTex(colorValue, cacheEntry.Tex);
                cacheEntry.Last = colorValue;
            }
        }

        private static void DrawTopRow(ColorCacheEntry cacheEntry, ref Color colorValue)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(cacheEntry.Tex, GUILayout.ExpandWidth(false));

            var colorStr = _drawColorHex
                ? "#" + ColorUtility.ToHtmlStringRGBA(colorValue)
                : string.Format(
                    CultureInfo.InvariantCulture,
                    "{0:F2} {1:F2} {2:F2} {3:F2}",
                    colorValue.r,
                    colorValue.g,
                    colorValue.b,
                    colorValue.a
                );
            var newColorStr = GUILayout.TextField(colorStr, GUILayout.ExpandWidth(true));
            if (GUI.changed && colorStr != newColorStr)
            {
                colorValue = ParseColorInput(newColorStr, colorValue);
            }

            _drawColorHex = GUILayout.Toggle(_drawColorHex, "Hex", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
        }

        private static Color ParseColorInput(string newColorStr, Color fallback)
        {
            if (_drawColorHex)
            {
                return ColorUtility.TryParseHtmlString(newColorStr, out var parsedColor)
                    ? parsedColor
                    : fallback;
            }

            try
            {
                return (Color)ConfigConverter.ConvertToObject(newColorStr, typeof(Color));
            }
            catch (FormatException)
            {
                // User may type partial/invalid color input — keep previous value until valid
                return fallback;
            }
        }

        private static void DrawRgbaSliders(ref Color colorValue)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("R", GUILayout.ExpandWidth(false));
            colorValue.r = GUILayout.HorizontalSlider(
                colorValue.r,
                0f,
                1f,
                GUILayout.ExpandWidth(true)
            );
            GUILayout.Label("G", GUILayout.ExpandWidth(false));
            colorValue.g = GUILayout.HorizontalSlider(
                colorValue.g,
                0f,
                1f,
                GUILayout.ExpandWidth(true)
            );
            GUILayout.Label("B", GUILayout.ExpandWidth(false));
            colorValue.b = GUILayout.HorizontalSlider(
                colorValue.b,
                0f,
                1f,
                GUILayout.ExpandWidth(true)
            );
            GUILayout.Label("A", GUILayout.ExpandWidth(false));
            colorValue.a = GUILayout.HorizontalSlider(
                colorValue.a,
                0f,
                1f,
                GUILayout.ExpandWidth(true)
            );
            GUILayout.EndHorizontal();
        }

        private static void FillTex(Color color, Texture2D tex)
        {
            if (color.a < 1f)
            {
                tex.FillTextureCheckerboard();
            }

            tex.FillTexture(color);
        }

        private sealed class ColorCacheEntry
        {
            public Color Last { get; set; }
            public Texture2D Tex { get; set; }
        }
    }
}

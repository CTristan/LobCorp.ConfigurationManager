// SPDX-License-Identifier: LGPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ConfigurationManager.Utilities
{
    /// <summary>
    /// Shared helpers for string formatting, texture operations, and process launching
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Inserts spaces before uppercase letters for display (e.g. "MyValue" → "My Value").
        /// </summary>
        /// <param name="str">The PascalCase string to convert</param>
        public static string ToProperCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            if (str.Length < 2)
            {
                return str;
            }

            string result = str.Substring(0, 1).ToUpper();

            for (int i = 1; i < str.Length; i++)
            {
                if (char.IsUpper(str[i]))
                {
                    result += " ";
                }

                result += str[i];
            }

            return result;
        }

        /// <summary>
        /// Appends ".0" if the string has no decimal point.
        /// </summary>
        /// <param name="s">The numeric string to check</param>
        public static string AppendZero(this string s)
        {
            return !s.Contains(".") ? s + ".0" : s;
        }

        /// <summary>
        /// Appends ".0" if the type is float, double, or decimal and the string lacks a decimal point.
        /// </summary>
        /// <param name="s">The numeric string to check</param>
        /// <param name="type">The CLR type of the value</param>
        public static string AppendZeroIfFloat(this string s, Type type)
        {
            return type == typeof(float) || type == typeof(double) || type == typeof(decimal)
                ? s.AppendZero()
                : s;
        }

        /// <summary>
        /// Fills a texture with the given color, alpha-blending if the color is translucent.
        /// </summary>
        /// <param name="tex">The texture to fill</param>
        /// <param name="color">Fill color; blended with existing pixels if alpha is less than 1</param>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(
            Justification = "Unity Texture2D API"
        )]
        public static void FillTexture(this Texture2D tex, Color color)
        {
            if (color.a < 1f)
            {
                for (var x = 0; x < tex.width; x++)
                {
                    for (var y = 0; y < tex.height; y++)
                    {
                        var origColor = tex.GetPixel(x, y);
                        var lerpedColor = Color.Lerp(origColor, color, color.a);
                        lerpedColor.a = Mathf.Max(origColor.a, color.a);
                        tex.SetPixel(x, y, lerpedColor);
                    }
                }
            }
            else
            {
                for (var x = 0; x < tex.width; x++)
                {
                    for (var y = 0; y < tex.height; y++)
                    {
                        tex.SetPixel(x, y, color);
                    }
                }
            }

            tex.Apply(false);
        }

        /// <summary>
        /// Fills a texture with a 10px black-and-white checkerboard pattern.
        /// </summary>
        /// <param name="tex">The texture to fill</param>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(
            Justification = "Unity Texture2D API"
        )]
        public static void FillTextureCheckerboard(this Texture2D tex)
        {
            for (var x = 0; x < tex.width; x++)
            {
                for (var y = 0; y < tex.height; y++)
                {
                    tex.SetPixel(x, y, (x / 10 + y / 10) % 2 == 1 ? Color.black : Color.white);
                }
            }

            tex.Apply(false);
        }

        /// <summary>
        /// Locates and opens the most recent Unity log file.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(
            Justification = "Unity Application.dataPath and Process.Start"
        )]
        public static void OpenLog()
        {
            var candidates = new List<string>();

            var rootDir = Path.Combine(Application.dataPath, "..");
            candidates.Add(Path.Combine(rootDir, "output_log.txt"));
            candidates.Add(Path.Combine(Application.dataPath, "output_log.txt"));

            var prop = typeof(Application).GetProperty(
                "consoleLogPath",
                BindingFlags.Static | BindingFlags.Public
            );
            if (prop != null)
            {
                var path = prop.GetValue(null, null) as string;
                candidates.Add(path);
            }

            if (Directory.Exists(Application.persistentDataPath))
            {
                var file = Directory
                    .GetFiles(
                        Application.persistentDataPath,
                        "output_log.txt",
                        SearchOption.AllDirectories
                    )
                    .FirstOrDefault();
                candidates.Add(file);
            }

            var latestLog = candidates
                .Where(x => x != null && File.Exists(x))
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault();
            if (latestLog != null && TryOpen(latestLog))
            {
                return;
            }

            candidates.Clear();
            candidates.AddRange(
                Directory.GetFiles(rootDir, "output_log.txt", SearchOption.AllDirectories)
            );
            latestLog = candidates
                .Where(File.Exists)
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault();
            if (latestLog != null && TryOpen(latestLog))
            {
                return;
            }

            throw new FileNotFoundException("No log files were found");
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = "Process.Start")]
        private static bool TryOpen(string path)
        {
            if (path == null)
            {
                return false;
            }

            try
            {
                _ = Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Extracts a URL from the plugin assembly's file version info metadata.
        /// </summary>
        /// <param name="pluginInstance">The plugin object whose assembly to inspect</param>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(
            Justification = "Requires assembly file on disk"
        )]
        public static string GetWebsite(object pluginInstance)
        {
            if (pluginInstance == null)
            {
                return null;
            }

            try
            {
                var type = pluginInstance.GetType();
                var location = type.Assembly.Location;
                if (string.IsNullOrEmpty(location) || !File.Exists(location))
                {
                    return null;
                }

                var fi = FileVersionInfo.GetVersionInfo(location);
                return new[]
                {
                    fi.CompanyName,
                    fi.FileDescription,
                    fi.Comments,
                    fi.LegalCopyright,
                    fi.LegalTrademarks,
                }.FirstOrDefault(x => Uri.IsWellFormedUriString(x, UriKind.Absolute));
            }
            catch (Exception e)
            {
                SimpleLogger.LogWarning("Failed to get URI - " + e.Message);
                return null;
            }
        }

        /// <summary>
        /// Opens a URL in the default browser.
        /// </summary>
        /// <param name="url">The URL to open</param>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = "Process.Start")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Design",
            "CA1054",
            Justification = "URL is a string from game metadata"
        )]
        public static void OpenWebsite(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    throw new Exception("Empty URL");
                }

                _ = Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                SimpleLogger.LogWarning("Failed to open URL " + url + " - " + ex.Message);
            }
        }

        /// <summary>
        /// Creates a shallow copy of a GUIStyle.
        /// </summary>
        /// <param name="original">The style to copy</param>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(
            Justification = "Unity GUIStyle API"
        )]
        public static GUIStyle CreateCopy(this GUIStyle original)
        {
            return new GUIStyle(original);
        }
    }
}

// Made by MarC0 / ManlyMarco
// Copyright 2018 GNU General Public License v3.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ConfigurationManager.Utilities
{
    public static class Utils
    {
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

        public static string AppendZero(this string s)
        {
            return !s.Contains(".") ? s + ".0" : s;
        }

        public static string AppendZeroIfFloat(this string s, Type type)
        {
            return type == typeof(float) || type == typeof(double) || type == typeof(decimal)
                ? s.AppendZero()
                : s;
        }

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
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                return true;
            }
            catch
            {
                return false;
            }
        }

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

                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                SimpleLogger.LogWarning("Failed to open URL " + url + " - " + ex.Message);
            }
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(
            Justification = "Unity GUIStyle API"
        )]
        public static GUIStyle CreateCopy(this GUIStyle original)
        {
            return new GUIStyle(original);
        }
    }
}

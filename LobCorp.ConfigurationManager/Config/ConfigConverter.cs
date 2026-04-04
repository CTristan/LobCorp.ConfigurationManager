// SPDX-License-Identifier: LGPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Globalization;
using ConfigurationManager.Input;
using UnityEngine;

namespace ConfigurationManager.Config
{
    /// <summary>
    /// Delegate pair for converting to/from string.
    /// </summary>
    public class TypeConverter
    {
        /// <summary>
        /// Delegate that serializes a value of the given type to a string
        /// </summary>
        public Func<object, Type, string> ConvertToString;

        /// <summary>
        /// Delegate that deserializes a string back into a value of the given type
        /// </summary>
        public Func<string, Type, object> ConvertToObject;
    }

    /// <summary>
    /// Registry of type converters for serializing config values to/from strings.
    /// </summary>
    public static class ConfigConverter
    {
        private static readonly Dictionary<Type, TypeConverter> Converters =
            new Dictionary<Type, TypeConverter>();

        static ConfigConverter()
        {
            AddConverter(
                typeof(string),
                new TypeConverter
                {
                    ConvertToString = (o, _) => (string)o ?? string.Empty,
                    ConvertToObject = (s, _) => s,
                }
            );

            AddConverter(
                typeof(bool),
                new TypeConverter
                {
                    ConvertToString = (o, _) => ((bool)o).ToString(CultureInfo.InvariantCulture),
                    ConvertToObject = (s, _) => bool.Parse(s),
                }
            );

            AddConverter(
                typeof(int),
                new TypeConverter
                {
                    ConvertToString = (o, _) => ((int)o).ToString(CultureInfo.InvariantCulture),
                    ConvertToObject = (s, _) => int.Parse(s, CultureInfo.InvariantCulture),
                }
            );

            AddConverter(
                typeof(long),
                new TypeConverter
                {
                    ConvertToString = (o, _) => ((long)o).ToString(CultureInfo.InvariantCulture),
                    ConvertToObject = (s, _) => long.Parse(s, CultureInfo.InvariantCulture),
                }
            );

            AddConverter(
                typeof(float),
                new TypeConverter
                {
                    ConvertToString = (o, _) => ((float)o).ToString(CultureInfo.InvariantCulture),
                    ConvertToObject = (s, _) => float.Parse(s, CultureInfo.InvariantCulture),
                }
            );

            AddConverter(
                typeof(double),
                new TypeConverter
                {
                    ConvertToString = (o, _) => ((double)o).ToString(CultureInfo.InvariantCulture),
                    ConvertToObject = (s, _) => double.Parse(s, CultureInfo.InvariantCulture),
                }
            );

            AddConverter(
                typeof(decimal),
                new TypeConverter
                {
                    ConvertToString = (o, _) => ((decimal)o).ToString(CultureInfo.InvariantCulture),
                    ConvertToObject = (s, _) => decimal.Parse(s, CultureInfo.InvariantCulture),
                }
            );

            AddConverter(
                typeof(short),
                new TypeConverter
                {
                    ConvertToString = (o, _) => ((short)o).ToString(CultureInfo.InvariantCulture),
                    ConvertToObject = (s, _) => short.Parse(s, CultureInfo.InvariantCulture),
                }
            );

            AddConverter(
                typeof(byte),
                new TypeConverter
                {
                    ConvertToString = (o, _) => ((byte)o).ToString(CultureInfo.InvariantCulture),
                    ConvertToObject = (s, _) => byte.Parse(s, CultureInfo.InvariantCulture),
                }
            );

            AddConverter(
                typeof(KeyCode),
                new TypeConverter
                {
                    ConvertToString = (o, _) => ((KeyCode)o).ToString(),
                    ConvertToObject = (s, _) => (KeyCode)Enum.Parse(typeof(KeyCode), s),
                }
            );

            AddConverter(
                typeof(KeyboardShortcut),
                new TypeConverter
                {
                    ConvertToString = (o, _) => ((KeyboardShortcut)o).Serialize(),
                    ConvertToObject = (s, _) => KeyboardShortcut.Deserialize(s),
                }
            );

            AddConverter(
                typeof(Color),
                new TypeConverter
                {
                    ConvertToString = (o, _) =>
                    {
                        var c = (Color)o;
                        return string.Format(
                            CultureInfo.InvariantCulture,
                            "{0} {1} {2} {3}",
                            c.r,
                            c.g,
                            c.b,
                            c.a
                        );
                    },
                    ConvertToObject = (s, _) =>
                    {
                        var parts = s.Split(' ');
                        return new Color(
                            float.Parse(parts[0], CultureInfo.InvariantCulture),
                            float.Parse(parts[1], CultureInfo.InvariantCulture),
                            float.Parse(parts[2], CultureInfo.InvariantCulture),
                            parts.Length > 3
                                ? float.Parse(parts[3], CultureInfo.InvariantCulture)
                                : 1f
                        );
                    },
                }
            );

            AddConverter(
                typeof(Vector2),
                new TypeConverter
                {
                    ConvertToString = (o, _) =>
                    {
                        var v = (Vector2)o;
                        return string.Format(CultureInfo.InvariantCulture, "{0} {1}", v.x, v.y);
                    },
                    ConvertToObject = (s, _) =>
                    {
                        var parts = s.Split(' ');
                        return new Vector2(
                            float.Parse(parts[0], CultureInfo.InvariantCulture),
                            float.Parse(parts[1], CultureInfo.InvariantCulture)
                        );
                    },
                }
            );

            AddConverter(
                typeof(Vector3),
                new TypeConverter
                {
                    ConvertToString = (o, _) =>
                    {
                        var v = (Vector3)o;
                        return string.Format(
                            CultureInfo.InvariantCulture,
                            "{0} {1} {2}",
                            v.x,
                            v.y,
                            v.z
                        );
                    },
                    ConvertToObject = (s, _) =>
                    {
                        var parts = s.Split(' ');
                        return new Vector3(
                            float.Parse(parts[0], CultureInfo.InvariantCulture),
                            float.Parse(parts[1], CultureInfo.InvariantCulture),
                            float.Parse(parts[2], CultureInfo.InvariantCulture)
                        );
                    },
                }
            );

            AddConverter(
                typeof(Vector4),
                new TypeConverter
                {
                    ConvertToString = (o, _) =>
                    {
                        var v = (Vector4)o;
                        return string.Format(
                            CultureInfo.InvariantCulture,
                            "{0} {1} {2} {3}",
                            v.x,
                            v.y,
                            v.z,
                            v.w
                        );
                    },
                    ConvertToObject = (s, _) =>
                    {
                        var parts = s.Split(' ');
                        return new Vector4(
                            float.Parse(parts[0], CultureInfo.InvariantCulture),
                            float.Parse(parts[1], CultureInfo.InvariantCulture),
                            float.Parse(parts[2], CultureInfo.InvariantCulture),
                            float.Parse(parts[3], CultureInfo.InvariantCulture)
                        );
                    },
                }
            );

            AddConverter(
                typeof(Quaternion),
                new TypeConverter
                {
                    ConvertToString = (o, _) =>
                    {
                        var q = (Quaternion)o;
                        return string.Format(
                            CultureInfo.InvariantCulture,
                            "{0} {1} {2} {3}",
                            q.x,
                            q.y,
                            q.z,
                            q.w
                        );
                    },
                    ConvertToObject = (s, _) =>
                    {
                        var parts = s.Split(' ');
                        return new Quaternion(
                            float.Parse(parts[0], CultureInfo.InvariantCulture),
                            float.Parse(parts[1], CultureInfo.InvariantCulture),
                            float.Parse(parts[2], CultureInfo.InvariantCulture),
                            float.Parse(parts[3], CultureInfo.InvariantCulture)
                        );
                    },
                }
            );
        }

        /// <summary>
        /// Registers a converter for the specified type, replacing any existing one.
        /// </summary>
        /// <param name="type">The CLR type to register a converter for</param>
        /// <param name="converter">The converter delegate pair</param>
        public static void AddConverter(Type type, TypeConverter converter)
        {
            Converters[type] = converter;
        }

        /// <summary>
        /// Looks up a registered converter for the type, falling back to generic enum handling.
        /// </summary>
        /// <param name="type">The CLR type to look up</param>
        public static TypeConverter GetConverter(Type type)
        {
            if (Converters.TryGetValue(type, out var converter))
            {
                return converter;
            }

            // Handle enums generically
            if (type.IsEnum)
            {
                return new TypeConverter
                {
                    ConvertToString = (o, _) => o.ToString(),
                    ConvertToObject = (s, _) => Enum.Parse(type, s),
                };
            }

            return null;
        }

        /// <summary>
        /// Converts a value to its string representation using the registered converter.
        /// </summary>
        /// <param name="value">The value to serialize</param>
        /// <param name="type">The CLR type of the value</param>
        public static string ConvertToString(object value, Type type)
        {
            var converter = GetConverter(type);
            if (converter != null)
            {
                return converter.ConvertToString(value, type);
            }

            return value != null ? value.ToString() : string.Empty;
        }

        /// <summary>
        /// Parses a string back into a typed value using the registered converter.
        /// </summary>
        /// <param name="value">The string to deserialize</param>
        /// <param name="type">The target CLR type</param>
        public static object ConvertToObject(string value, Type type)
        {
            var converter = GetConverter(type);
            if (converter != null)
            {
                return converter.ConvertToObject(value, type);
            }

            return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
        }
    }
}

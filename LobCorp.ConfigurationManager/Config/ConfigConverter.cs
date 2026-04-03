using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace ConfigurationManager.Config
{
    /// <summary>
    /// Delegate pair for converting to/from string.
    /// </summary>
    public class TypeConverter
    {
        public Func<object, Type, string> ConvertToString;
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
                    ConvertToString = (o, t) => (string)o ?? string.Empty,
                    ConvertToObject = (s, t) => s,
                }
            );

            AddConverter(
                typeof(bool),
                new TypeConverter
                {
                    ConvertToString = (o, t) => ((bool)o).ToString(CultureInfo.InvariantCulture),
                    ConvertToObject = (s, t) => bool.Parse(s),
                }
            );

            AddConverter(
                typeof(int),
                new TypeConverter
                {
                    ConvertToString = (o, t) => ((int)o).ToString(CultureInfo.InvariantCulture),
                    ConvertToObject = (s, t) => int.Parse(s, CultureInfo.InvariantCulture),
                }
            );

            AddConverter(
                typeof(long),
                new TypeConverter
                {
                    ConvertToString = (o, t) => ((long)o).ToString(CultureInfo.InvariantCulture),
                    ConvertToObject = (s, t) => long.Parse(s, CultureInfo.InvariantCulture),
                }
            );

            AddConverter(
                typeof(float),
                new TypeConverter
                {
                    ConvertToString = (o, t) => ((float)o).ToString(CultureInfo.InvariantCulture),
                    ConvertToObject = (s, t) => float.Parse(s, CultureInfo.InvariantCulture),
                }
            );

            AddConverter(
                typeof(double),
                new TypeConverter
                {
                    ConvertToString = (o, t) => ((double)o).ToString(CultureInfo.InvariantCulture),
                    ConvertToObject = (s, t) => double.Parse(s, CultureInfo.InvariantCulture),
                }
            );

            AddConverter(
                typeof(decimal),
                new TypeConverter
                {
                    ConvertToString = (o, t) => ((decimal)o).ToString(CultureInfo.InvariantCulture),
                    ConvertToObject = (s, t) => decimal.Parse(s, CultureInfo.InvariantCulture),
                }
            );

            AddConverter(
                typeof(short),
                new TypeConverter
                {
                    ConvertToString = (o, t) => ((short)o).ToString(CultureInfo.InvariantCulture),
                    ConvertToObject = (s, t) => short.Parse(s, CultureInfo.InvariantCulture),
                }
            );

            AddConverter(
                typeof(byte),
                new TypeConverter
                {
                    ConvertToString = (o, t) => ((byte)o).ToString(CultureInfo.InvariantCulture),
                    ConvertToObject = (s, t) => byte.Parse(s, CultureInfo.InvariantCulture),
                }
            );

            AddConverter(
                typeof(KeyCode),
                new TypeConverter
                {
                    ConvertToString = (o, t) => ((KeyCode)o).ToString(),
                    ConvertToObject = (s, t) => (KeyCode)Enum.Parse(typeof(KeyCode), s),
                }
            );

            AddConverter(
                typeof(KeyboardShortcut),
                new TypeConverter
                {
                    ConvertToString = (o, t) => ((KeyboardShortcut)o).Serialize(),
                    ConvertToObject = (s, t) => KeyboardShortcut.Deserialize(s),
                }
            );

            AddConverter(
                typeof(Color),
                new TypeConverter
                {
                    ConvertToString = (o, t) =>
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
                    ConvertToObject = (s, t) =>
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
                    ConvertToString = (o, t) =>
                    {
                        var v = (Vector2)o;
                        return string.Format(CultureInfo.InvariantCulture, "{0} {1}", v.x, v.y);
                    },
                    ConvertToObject = (s, t) =>
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
                    ConvertToString = (o, t) =>
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
                    ConvertToObject = (s, t) =>
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
                    ConvertToString = (o, t) =>
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
                    ConvertToObject = (s, t) =>
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
                    ConvertToString = (o, t) =>
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
                    ConvertToObject = (s, t) =>
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

        public static void AddConverter(Type type, TypeConverter converter)
        {
            Converters[type] = converter;
        }

        public static TypeConverter GetConverter(Type type)
        {
            TypeConverter converter;
            if (Converters.TryGetValue(type, out converter))
                return converter;

            // Handle enums generically
            if (type.IsEnum)
            {
                return new TypeConverter
                {
                    ConvertToString = (o, t) => o.ToString(),
                    ConvertToObject = (s, t) => Enum.Parse(type, s),
                };
            }

            return null;
        }

        public static string ConvertToString(object value, Type type)
        {
            var converter = GetConverter(type);
            if (converter != null)
                return converter.ConvertToString(value, type);
            return value != null ? value.ToString() : string.Empty;
        }

        public static object ConvertToObject(string value, Type type)
        {
            var converter = GetConverter(type);
            if (converter != null)
                return converter.ConvertToObject(value, type);
            return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
        }
    }
}

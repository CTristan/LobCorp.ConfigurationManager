// SPDX-License-Identifier: LGPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Linq;
using ConfigurationManager.Utilities;
using UnityEngine;

namespace ConfigurationManager.Input
{
    /// <summary>
    /// A keyboard shortcut that can be used in Update method to check if user presses a key combo.
    /// The shortcut is only triggered when the user presses the exact combination.
    /// </summary>
    public struct KeyboardShortcut
    {
        /// <summary>
        /// Shortcut that never triggers.
        /// </summary>
        public static readonly KeyboardShortcut Empty = new KeyboardShortcut();

        /// <summary>
        /// All KeyCode values that can be used in a keyboard shortcut.
        /// </summary>
        public static readonly IEnumerable<KeyCode> AllKeyCodes =
            Enum.GetValues(typeof(KeyCode)) as KeyCode[];

        /// <summary>
        /// All key codes except mouse buttons, used to detect unwanted modifier keys during shortcut matching
        /// </summary>
        public static readonly KeyCode[] ModifierBlockKeyCodes = AllKeyCodes
            .Except(
                new[]
                {
                    KeyCode.Mouse0,
                    KeyCode.Mouse1,
                    KeyCode.Mouse2,
                    KeyCode.Mouse3,
                    KeyCode.Mouse4,
                    KeyCode.Mouse5,
                    KeyCode.Mouse6,
                    KeyCode.None,
                }
            )
            .ToArray();

        private readonly KeyCode[] _allKeys;

        /// <summary>
        /// Create a new keyboard shortcut.
        /// </summary>
        /// <param name="mainKey">Primary key of the shortcut (must not be None if modifiers are specified).</param>
        /// <param name="modifiers">Zero or more modifier keys that must be held alongside the main key.</param>
        public KeyboardShortcut(KeyCode mainKey, params KeyCode[] modifiers)
            : this(new[] { mainKey }.Concat(modifiers).ToArray())
        {
            if (mainKey == KeyCode.None && modifiers.Any())
            {
                throw new ArgumentException(
                    "Can't set mainKey to KeyCode.None if there are any modifiers"
                );
            }
        }

        private KeyboardShortcut(KeyCode[] keys)
        {
            _allKeys = SanitizeKeys(keys);
        }

        private static KeyCode[] SanitizeKeys(params KeyCode[] keys)
        {
            return keys != null && keys.Length != 0 && keys[0] != KeyCode.None
                ? new[] { keys[0] }
                    .Concat(keys.Skip(1).Distinct().Where(x => x != keys[0]).OrderBy(x => (int)x))
                    .ToArray()
                : new[] { KeyCode.None };
        }

        /// <summary>
        /// Main key of the key combination.
        /// </summary>
        public KeyCode MainKey => _allKeys?.Length > 0 ? _allKeys[0] : KeyCode.None;

        /// <summary>
        /// Modifiers of the key combination, if any.
        /// </summary>
        public IEnumerable<KeyCode> Modifiers =>
            _allKeys != null ? _allKeys.Skip(1) : Enumerable.Empty<KeyCode>();

        /// <summary>
        /// Attempt to deserialize key combination from the string.
        /// </summary>
        /// <param name="str">Serialized key combination string (e.g. "LeftControl + F1") to parse.</param>
        public static KeyboardShortcut Deserialize(string str)
        {
            try
            {
                var parts = str.Split(
                        new[] { ' ', '+', ',', ';', '|' },
                        StringSplitOptions.RemoveEmptyEntries
                    )
                    .Select(x => (KeyCode)Enum.Parse(typeof(KeyCode), x))
                    .ToArray();
                return new KeyboardShortcut(parts);
            }
            catch (SystemException ex)
            {
                SimpleLogger.LogError("Failed to read keybind from settings: " + ex.Message);
            }
            return Empty;
        }

        /// <summary>
        /// Serialize the key combination into a user readable string.
        /// </summary>
        public string Serialize()
        {
            return _allKeys != null
                ? string.Join(" + ", _allKeys.Select(x => x.ToString()).ToArray())
                : string.Empty;
        }

        /// <summary>
        /// Check if the main key was just pressed (Input.GetKeyDown), and specified modifier keys are all pressed.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(
            Justification = "Requires Unity Input runtime"
        )]
        public bool IsDown()
        {
            var mainKey = MainKey;
            return mainKey != KeyCode.None
                && UnityEngine.Input.GetKeyDown(mainKey)
                && ModifierKeyTest();
        }

        /// <summary>
        /// Check if the main key is currently held down (Input.GetKey), and specified modifier keys are all pressed.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(
            Justification = "Requires Unity Input runtime"
        )]
        public bool IsPressed()
        {
            var mainKey = MainKey;
            return mainKey != KeyCode.None
                && UnityEngine.Input.GetKey(mainKey)
                && ModifierKeyTest();
        }

        /// <summary>
        /// Check if the main key was just lifted (Input.GetKeyUp), and specified modifier keys are all pressed.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(
            Justification = "Requires Unity Input runtime"
        )]
        public bool IsUp()
        {
            var mainKey = MainKey;
            return mainKey != KeyCode.None
                && UnityEngine.Input.GetKeyUp(mainKey)
                && ModifierKeyTest();
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(
            Justification = "Requires Unity Input runtime"
        )]
        private bool ModifierKeyTest()
        {
            var mainKey = MainKey;
            return _allKeys.All(key => key == mainKey || UnityEngine.Input.GetKey(key))
                && ModifierBlockKeyCodes
                    .Except(_allKeys)
                    .All(key => !UnityEngine.Input.GetKey(key));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return MainKey != KeyCode.None
                ? string.Join(" + ", _allKeys.Select(key => key.ToString()).ToArray())
                : "Not set";
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is KeyboardShortcut shortcut
                && MainKey == shortcut.MainKey
                && Modifiers.SequenceEqual(shortcut.Modifiers);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return MainKey != KeyCode.None
                ? _allKeys.Aggregate(
                    _allKeys.Length,
                    (current, item) => unchecked((current * 31) + (int)item)
                )
                : 0;
        }
    }
}

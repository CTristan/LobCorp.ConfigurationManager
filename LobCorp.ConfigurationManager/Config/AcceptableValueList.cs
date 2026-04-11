// SPDX-License-Identifier: LGPL-3.0-or-later

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ConfigurationManager.Config
{
    /// <summary>
    /// Specifies the list of acceptable values for a setting.
    /// </summary>
    /// <typeparam name="T">The equatable type of the setting value (e.g. string, enum).</typeparam>
    public sealed class AcceptableValueList<T> : IAcceptableValueList
        where T : IEquatable<T>
    {
        /// <inheritdoc />
        IEnumerable IAcceptableValueList.BoxedAcceptableValues => AcceptableValues;

        /// <summary>
        /// Array of valid values for this setting
        /// </summary>
        public IList<T> AcceptableValues { get; }

        /// <summary>
        /// Creates a new constraint limiting values to the specified list.
        /// </summary>
        /// <param name="acceptableValues">One or more values that the setting may hold</param>
        public AcceptableValueList(params T[] acceptableValues)
        {
            if (acceptableValues == null || acceptableValues.Length == 0)
            {
                throw new ArgumentException("At least one acceptable value is needed");
            }

            AcceptableValues = Array.AsReadOnly((T[])acceptableValues.Clone());
        }

        /// <inheritdoc />
        public Type ValueType => typeof(T);

        /// <summary>
        /// Checks whether a value is contained in the acceptable list.
        /// </summary>
        /// <param name="value">The value to test</param>
        public bool IsValid(T value)
        {
            return AcceptableValues.Any(v => v.Equals(value));
        }
    }
}

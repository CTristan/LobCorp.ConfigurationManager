// SPDX-License-Identifier: LGPL-3.0-or-later

using System;

namespace ConfigurationManager
{
    /// <summary>
    /// Arguments representing a change in value
    /// </summary>
    public sealed class ValueChangedEventArgs<TValue> : EventArgs
    {
        /// <inheritdoc />
        public ValueChangedEventArgs(TValue newValue)
        {
            NewValue = newValue;
        }

        /// <summary>
        /// Newly assigned value
        /// </summary>
        public TValue NewValue { get; private set; }
    }
}

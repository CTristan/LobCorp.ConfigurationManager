// SPDX-License-Identifier: LGPL-3.0-or-later

using System;
using System.Collections;

namespace ConfigurationManager.Config
{
    /// <summary>
    /// Base type of all acceptable value constraints.
    /// </summary>
    public interface IAcceptableValue
    {
        /// <summary>
        /// The CLR type of values this constraint applies to
        /// </summary>
        Type ValueType { get; }
    }

    /// <summary>
    /// Acceptable value constraint that enumerates a list of valid values as untyped objects.
    /// </summary>
    public interface IAcceptableValueList : IAcceptableValue
    {
        /// <summary>
        /// The valid values, boxed as objects so callers can iterate without knowing the element type.
        /// </summary>
        IEnumerable BoxedAcceptableValues { get; }
    }

    /// <summary>
    /// Acceptable value constraint that defines an inclusive range as untyped objects.
    /// </summary>
    public interface IAcceptableValueRange : IAcceptableValue
    {
        /// <summary>
        /// Inclusive lower bound, boxed as an object.
        /// </summary>
        object BoxedMinValue { get; }

        /// <summary>
        /// Inclusive upper bound, boxed as an object.
        /// </summary>
        object BoxedMaxValue { get; }
    }
}

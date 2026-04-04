// SPDX-License-Identifier: LGPL-3.0-or-later

using System;

namespace ConfigurationManager.Config
{
    /// <summary>
    /// Base type of all acceptable value constraints.
    /// </summary>
    public abstract class AcceptableValueBase
    {
        /// <summary>
        /// The CLR type of values this constraint applies to
        /// </summary>
        public abstract Type ValueType { get; }
    }
}

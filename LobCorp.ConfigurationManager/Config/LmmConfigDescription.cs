// SPDX-License-Identifier: LGPL-3.0-or-later

using System;
using System.Collections.Generic;

namespace ConfigurationManager.Config
{
    /// <summary>
    /// Metadata for a config entry, including description, acceptable values, and tags.
    /// </summary>
    public class LmmConfigDescription
    {
        /// <summary>
        /// Human-readable description shown in the config UI
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Optional constraint on valid values (range or list)
        /// </summary>
        public IAcceptableValue AcceptableValues { get; }

        /// <summary>
        /// Attribute objects applied to the entry (e.g. <see cref="ConfigurationManagerAttributes"/>)
        /// </summary>
        public IList<object> Tags { get; }

        /// <summary>
        /// Creates a new config description with optional value constraints and tags.
        /// </summary>
        /// <param name="description">Human-readable description text</param>
        /// <param name="acceptableValues">Optional constraint on valid values</param>
        /// <param name="tags">Attribute objects that control UI display behavior</param>
        public LmmConfigDescription(
            string description,
            IAcceptableValue acceptableValues = null,
            params object[] tags
        )
        {
            Description = description;
            AcceptableValues = acceptableValues;
            Tags = Array.AsReadOnly((object[])(tags ?? new object[0]).Clone());
        }
    }
}

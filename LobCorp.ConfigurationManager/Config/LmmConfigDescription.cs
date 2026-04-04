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
        public string Description { get; private set; }

        /// <summary>
        /// Optional constraint on valid values (range or list)
        /// </summary>
        public AcceptableValueBase AcceptableValues { get; private set; }

        /// <summary>
        /// Attribute objects applied to the entry (e.g. <see cref="ConfigurationManagerAttributes"/>)
        /// </summary>
        public object[] Tags { get; private set; }

        /// <summary>
        /// Creates a new config description with optional value constraints and tags.
        /// </summary>
        /// <param name="description">Human-readable description text</param>
        /// <param name="acceptableValues">Optional constraint on valid values</param>
        /// <param name="tags">Attribute objects that control UI display behavior</param>
        public LmmConfigDescription(
            string description,
            AcceptableValueBase acceptableValues = null,
            params object[] tags
        )
        {
            Description = description;
            AcceptableValues = acceptableValues;
            Tags = tags ?? new object[0];
        }
    }
}

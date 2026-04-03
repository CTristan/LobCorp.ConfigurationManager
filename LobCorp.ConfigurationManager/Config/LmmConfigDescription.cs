namespace ConfigurationManager.Config
{
    /// <summary>
    /// Metadata for a config entry, including description, acceptable values, and tags.
    /// </summary>
    public class LmmConfigDescription
    {
        public string Description { get; private set; }
        public AcceptableValueBase AcceptableValues { get; private set; }
        public object[] Tags { get; private set; }

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

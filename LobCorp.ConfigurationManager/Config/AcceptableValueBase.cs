using System;

namespace ConfigurationManager.Config
{
    /// <summary>
    /// Base type of all acceptable value constraints.
    /// </summary>
    public abstract class AcceptableValueBase
    {
        public abstract Type ValueType { get; }
    }
}

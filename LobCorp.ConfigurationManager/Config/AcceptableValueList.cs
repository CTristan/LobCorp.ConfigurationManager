using System;
using System.Linq;

namespace ConfigurationManager.Config
{
    /// <summary>
    /// Specifies the list of acceptable values for a setting.
    /// </summary>
    public class AcceptableValueList<T> : AcceptableValueBase
        where T : IEquatable<T>
    {
        public T[] AcceptableValues { get; private set; }

        public AcceptableValueList(params T[] acceptableValues)
        {
            if (acceptableValues == null || acceptableValues.Length == 0)
                throw new ArgumentException("At least one acceptable value is needed");
            AcceptableValues = acceptableValues;
        }

        public override Type ValueType
        {
            get { return typeof(T); }
        }

        public bool IsValid(T value)
        {
            return AcceptableValues.Any(v => v.Equals(value));
        }
    }
}

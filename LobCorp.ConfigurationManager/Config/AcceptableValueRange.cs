using System;

namespace ConfigurationManager.Config
{
    /// <summary>
    /// Specifies the valid range for a setting value.
    /// </summary>
    public class AcceptableValueRange<T> : AcceptableValueBase
        where T : IComparable
    {
        public T MinValue { get; private set; }
        public T MaxValue { get; private set; }

        public AcceptableValueRange(T minValue, T maxValue)
        {
            if (maxValue.CompareTo(minValue) < 0)
                throw new ArgumentException("MaxValue must be greater than or equal to MinValue");
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public override Type ValueType
        {
            get { return typeof(T); }
        }

        public T Clamp(T value)
        {
            if (value.CompareTo(MinValue) < 0)
                return MinValue;
            if (value.CompareTo(MaxValue) > 0)
                return MaxValue;
            return value;
        }
    }
}

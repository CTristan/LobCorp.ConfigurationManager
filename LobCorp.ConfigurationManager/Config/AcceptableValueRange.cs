using System;

namespace ConfigurationManager.Config
{
    /// <summary>
    /// Specifies the valid range for a setting value.
    /// </summary>
    public class AcceptableValueRange<T> : AcceptableValueBase
        where T : IComparable
    {
        /// <summary>
        /// Inclusive lower bound of the valid range
        /// </summary>
        public T MinValue { get; private set; }

        /// <summary>
        /// Inclusive upper bound of the valid range
        /// </summary>
        public T MaxValue { get; private set; }

        /// <summary>
        /// Creates a new range constraint with the given bounds.
        /// </summary>
        /// <param name="minValue">Inclusive lower bound</param>
        /// <param name="maxValue">Inclusive upper bound</param>
        public AcceptableValueRange(T minValue, T maxValue)
        {
            if (maxValue.CompareTo(minValue) < 0)
            {
                throw new ArgumentException("MaxValue must be greater than or equal to MinValue");
            }

            MinValue = minValue;
            MaxValue = maxValue;
        }

        /// <inheritdoc />
        public override Type ValueType
        {
            get { return typeof(T); }
        }

        /// <summary>
        /// Clamps a value to within the valid range.
        /// </summary>
        /// <param name="value">The value to clamp</param>
        public T Clamp(T value)
        {
            if (value.CompareTo(MinValue) < 0)
            {
                return MinValue;
            }

            if (value.CompareTo(MaxValue) > 0)
            {
                return MaxValue;
            }

            return value;
        }
    }
}

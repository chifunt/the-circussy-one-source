using System;

namespace TheCircussyOne.Config
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class NumericSliderAttribute : Attribute
    {
        public NumericSliderAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public float Min { get; }
        public float Max { get; }
    }
}

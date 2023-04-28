using System;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex
{
    public abstract class InputSlider<T> : Field<T>
    {
        protected override string[] DefaultClasses => new string[] { "field", "input-slider" };

        public readonly Input<T> input;
        public readonly Slider<T> slider;

        protected abstract Input<T> InputFactory { get; }
        protected abstract Slider<T> SliderFactory { get; }

        public T Min
        {
            get => slider.Min;
            set => slider.Min = value;
        }
        public T Max
        {
            get => slider.Max;
            set => slider.Max = value;
        }
        public bool Delayed { get; set; }

        public override T CurrentValue
        {
            set
            {
                base.CurrentValue = value;
                input.CurrentValue = value;
                slider.CurrentValue = value;
            }
        }


        public InputSlider()
        {
            DelegateValue<T> iValue = new DelegateValue<T>(() => CurrentValue, value =>
            {
                if (Delayed) CurrentValue = Clamp(value);
                else BindedValue = Clamp(value);
            });

            input = InputFactory.Bind(iValue);
            slider = SliderFactory.Bind(iValue);
        }

        protected abstract T Clamp(T value);
    }


    #region Byte Input Slider
    public class ByteInputSlider : InputSlider<byte>
    {
        protected override Input<byte> InputFactory => this.Create<ByteInput>();
        protected override Slider<byte> SliderFactory => this.Create<ByteSlider>("flexible");


        public ByteInputSlider() => Modify();
        public ByteInputSlider Modify(byte min = byte.MinValue, byte max = byte.MaxValue, bool delayed = false)
        {
            Min = min;
            Max = max;
            Delayed = delayed;

            return this;
        }

        protected override byte Clamp(byte value)
        {
            if (value <= Min) return Min;
            if (value >= Max) return Max;
            return value;
        }
    }
    #endregion Byte Input Slider

    #region Int Input Slider
    public class IntInputSlider : InputSlider<int>
    {
        protected override Input<int> InputFactory => this.Create<IntInput>();
        protected override Slider<int> SliderFactory => this.Create<IntSlider>("flexible");


        public IntInputSlider() => Modify();
        public IntInputSlider Modify(int min = int.MinValue, int max = int.MaxValue, bool delayed = false)
        {
            Min = min;
            Max = max;
            Delayed = delayed;

            return this;
        }

        protected override int Clamp(int value)
        {
            if (value <= Min) return Min;
            if (value >= Max) return Max;
            return value;
        }
    }
    #endregion Int Input Slider

    #region Long Input Slider
    public class LongInputSlider : InputSlider<long>
    {
        protected override Input<long> InputFactory => this.Create<LongInput>();
        protected override Slider<long> SliderFactory => this.Create<LongSlider>("flexible");


        public LongInputSlider() => Modify();
        public LongInputSlider Modify(long min = long.MinValue, long max = long.MaxValue, bool delayed = false)
        {
            Min = min;
            Max = max;
            Delayed = delayed;

            return this;
        }

        protected override long Clamp(long value)
        {
            if (value <= Min) return Min;
            if (value >= Max) return Max;
            return value;
        }
    }
    #endregion Long Input Slider

    #region Float Input Slider
    public class FloatInputSlider : InputSlider<float>
    {
        protected override Input<float> InputFactory => this.Create<FloatInput>();
        protected override Slider<float> SliderFactory => this.Create<FloatSlider>("flexible");

        public int Decimals { get => ((FloatSlider)slider).Decimals; set => ((FloatSlider)slider).Decimals = value; }


        public FloatInputSlider() => Modify();
        public FloatInputSlider Modify(float min = float.MinValue, float max = float.MaxValue, int decimals = 1, bool delayed = false)
        {
            Min = min;
            Max = max;
            Decimals = decimals;
            Delayed = delayed;

            return this;
        }

        protected override float Clamp(float value)
        {
            if (value <= Min) return Min;
            if (value >= Max) return Max;
            return value;
        }
    }
    #endregion Float Input Slider

    #region Double Input Slider
    public class DoubleInputSlider : InputSlider<double>
    {
        protected override Input<double> InputFactory => this.Create<DoubleInput>();
        protected override Slider<double> SliderFactory => this.Create<DoubleSlider>("flexible");

        public int Decimals { get => ((DoubleSlider)slider).Decimals; set => ((DoubleSlider)slider).Decimals = value; }


        public DoubleInputSlider() => Modify();
        public DoubleInputSlider Modify(double min = double.MinValue, double max = double.MaxValue, int decimals = 1, bool delayed = false)
        {
            Min = min;
            Max = max;
            Decimals = decimals;
            Delayed = delayed;

            return this;
        }

        protected override double Clamp(double value)
        {
            if (value <= Min) return Min;
            if (value >= Max) return Max;
            return value;
        }
    }
    #endregion Double Input Slider

    #region Decimal Input Slider
    public class DecimalInputSlider : InputSlider<decimal>
    {
        protected override Input<decimal> InputFactory => this.Create<DecimalInput>();
        protected override Slider<decimal> SliderFactory => this.Create<DecimalSlider>("flexible");

        public int Decimals { get => ((DecimalSlider)slider).Decimals; set => ((DecimalSlider)slider).Decimals = value; }


        public DecimalInputSlider() => Modify();
        public DecimalInputSlider Modify(decimal min = decimal.MinValue, decimal max = decimal.MaxValue, int decimals = 1, bool delayed = false)
        {
            Min = min;
            Max = max;
            Decimals = decimals;
            Delayed = delayed;

            return this;
        }

        protected override decimal Clamp(decimal value)
        {
            if (value <= Min) return Min;
            if (value >= Max) return Max;
            return value;
        }
    }
    #endregion Decimal Input Slider
}
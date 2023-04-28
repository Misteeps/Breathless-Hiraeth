using System;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex
{
    public abstract class Slider<T> : Field<T>
    {
        protected override string[] DefaultClasses => new string[] { "field", "slider" };

        public readonly Div gauge;
        public readonly Div fill;
        public readonly Div knob;

        private bool dragging;

        public T Min { get; set; }
        public T Max { get; set; }
        public bool Delayed { get; set; }

        public override T CurrentValue
        {
            set
            {
                base.CurrentValue = value;
                fill.style.width = new Length(GetFactor(value) * 100, LengthUnit.Percent);
            }
        }


        public Slider()
        {
            gauge = this.Create<Div>("gauge", "flexible");
            fill = gauge.Create<Div>("fill");
            knob = fill.Create<Div>("knob");

            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
        }

        private void OnPointerUp(PointerUpEvent pointerEvent)
        {
            if (!dragging || pointerEvent.button != 0) return;

            dragging = false;

            Rect bounds = worldBound;
            BindedValue = GetValue(Mathf.InverseLerp(bounds.xMin, bounds.xMax, pointerEvent.position.x));

            this.ReleasePointer(pointerEvent.pointerId);
            pointerEvent.StopPropagation();
        }
        private void OnPointerDown(PointerDownEvent pointerEvent)
        {
            if (dragging || pointerEvent.button != 0) return;

            dragging = true;

            Rect bounds = worldBound;
            if (Delayed) CurrentValue = GetValue(Mathf.InverseLerp(bounds.xMin, bounds.xMax, pointerEvent.position.x));
            else BindedValue = GetValue(Mathf.InverseLerp(bounds.xMin, bounds.xMax, pointerEvent.position.x));

            this.CapturePointer(pointerEvent.pointerId);
            pointerEvent.StopPropagation();
        }
        private void OnPointerMove(PointerMoveEvent pointerEvent)
        {
            if (!dragging || !this.HasPointerCapture(pointerEvent.pointerId)) return;

            Rect bounds = worldBound;
            if (Delayed) CurrentValue = GetValue(Mathf.InverseLerp(bounds.xMin, bounds.xMax, pointerEvent.position.x));
            else BindedValue = GetValue(Mathf.InverseLerp(bounds.xMin, bounds.xMax, pointerEvent.position.x));

            pointerEvent.StopPropagation();
        }

        protected abstract float GetFactor(T value);
        protected abstract T GetValue(float factor);
    }


    #region Byte Slider
    public class ByteSlider : Slider<byte>
    {
        public ByteSlider() => Modify();
        public ByteSlider Modify(byte min = byte.MinValue, byte max = byte.MaxValue, bool delayed = false)
        {
            Min = min;
            Max = max;
            Delayed = delayed;

            return this;
        }

        protected override float GetFactor(byte value) => Mathf.InverseLerp(Min, Max, value);
        protected override byte GetValue(float factor) => (byte)Mathf.Round(Mathf.Lerp(Min, Max, factor));
    }
    #endregion Byte Slider

    #region Int Slider
    public class IntSlider : Slider<int>
    {
        public IntSlider() => Modify();
        public IntSlider Modify(int min = int.MinValue, int max = int.MaxValue, bool delayed = false)
        {
            Min = min;
            Max = max;
            Delayed = delayed;

            return this;
        }

        protected override float GetFactor(int value) => Mathf.InverseLerp(Min, Max, value);
        protected override int GetValue(float factor) => (int)Mathf.Round(Mathf.Lerp(Min, Max, factor));
    }
    #endregion Int Slider

    #region Long Slider
    public class LongSlider : Slider<long>
    {
        public LongSlider() => Modify();
        public LongSlider Modify(long min = long.MinValue, long max = long.MaxValue, bool delayed = false)
        {
            Min = min;
            Max = max;
            Delayed = delayed;

            return this;
        }

        protected override float GetFactor(long value) => Mathf.InverseLerp(Min, Max, value);
        protected override long GetValue(float factor) => (long)Mathf.Round(Mathf.Lerp(Min, Max, factor));
    }
    #endregion Long Slider

    #region Float Slider
    public class FloatSlider : Slider<float>
    {
        public int Decimals { get; set; }


        public FloatSlider() => Modify();
        public FloatSlider Modify(float min = float.MinValue, float max = float.MaxValue, int decimals = 1, bool delayed = false)
        {
            Min = min;
            Max = max;
            Decimals = decimals;
            Delayed = delayed;

            return this;
        }

        protected override float GetFactor(float value) => Mathf.InverseLerp(Min, Max, value);
        protected override float GetValue(float factor) => (float)Math.Round(Min + (Max - Min) * Mathf.Clamp01(factor), Decimals);
    }
    #endregion Float Slider

    #region Double Slider
    public class DoubleSlider : Slider<double>
    {
        public int Decimals { get; set; }


        public DoubleSlider() => Modify();
        public DoubleSlider Modify(double min = double.MinValue, double max = double.MaxValue, int decimals = 1, bool delayed = false)
        {
            Min = min;
            Max = max;
            Decimals = decimals;
            Delayed = delayed;

            return this;
        }

        protected override float GetFactor(double value) => (Min == Max) ? 0 : Mathf.Clamp01((float)((value - Min) / (Max - Min)));
        protected override double GetValue(float factor) => Math.Round(Min + (Max - Min) * (double)Mathf.Clamp01(factor), Decimals);
    }
    #endregion Double Slider

    #region Decimal Slider
    public class DecimalSlider : Slider<decimal>
    {
        public int Decimals { get; set; }


        public DecimalSlider() => Modify();
        public DecimalSlider Modify(decimal min = decimal.MinValue, decimal max = decimal.MaxValue, int decimals = 1, bool delayed = false)
        {
            Min = min;
            Max = max;
            Decimals = decimals;
            Delayed = delayed;

            return this;
        }

        protected override float GetFactor(decimal value) => (Min == Max) ? 0 : Mathf.Clamp01((float)((value - Min) / (Max - Min)));
        protected override decimal GetValue(float factor) => Math.Round(Min + (Max - Min) * (decimal)Mathf.Clamp01(factor), Decimals);
    }
    #endregion Decimal Slider
}
using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex
{
    #region Enums
    public enum Function { Linear, Quadratic, Cubic, Quartic, Quintic, Sine, Circular, Exponential, Elastic, Back, Bounce }
    public enum Direction { In, Out, InOut }

    public enum TransformField { Position, LocalPosition, Rotation, LocalRotation, EulerAngles, LocalEulerAngles, LocalScale }
    public enum SpriteRendererField { Size, Color }
    public enum AudioSourceField { Time, Volume, Pitch, PanStereo, SpatialBlend, ReverbZoneMix, DopplerLevel, Spread, MinDistance, MaxDistance }

    public enum RectTransformField { AnchoredPosition, AnchoredPosition3D, SizeDelta, AnchorMin, AnchorMax, OffsetMin, OffsetMax, Pivot }
    public enum CanvasGroupField { Alpha }
    public enum ImageField { Color }
    public enum OutlineField { EffectColor, EffectDistance }

    public enum VisualElementField { Position, Width, Height, Margin, Padding, BorderWidth, BorderRadius, BorderColor, FontSize, Opacity, Color, BackgroundColor, BackgroundTint, Translate, Rotate, Scale }

    public enum Unit { X, Y, Z, W, R, G, B, A, Top, Bottom, Left, Right }
    #endregion Enums

    #region Transition
    public class Transition
    {
        public readonly int hash;

        private readonly Func<float> getValue;
        private readonly Action<float> setValue;
        private readonly float start;
        private readonly float end;
        private readonly Action onEnd;

        private Func<float, float> curve;
        private Func<float, float> inverseCurve;
        public float duration;

        public float speed;
        public bool isRealTime;
        public bool isRewinding;

        public float timer;


        public Transition(Func<float> getValue, Action<float> setValue, float start, float end) : this(getValue, setValue, start, end, null, HashCode.Combine(getValue, setValue)) { }
        public Transition(Func<float> getValue, Action<float> setValue, float start, float end, string hash) : this(getValue, setValue, start, end, null, HashCode.Combine(hash)) { }
        public Transition(Func<float> getValue, Action<float> setValue, float start, float end, int hash) : this(getValue, setValue, start, end, null, hash) { }
        public Transition(Func<float> getValue, Action<float> setValue, float start, float end, Action onEnd) : this(getValue, setValue, start, end, onEnd, HashCode.Combine(getValue, setValue)) { }
        public Transition(Func<float> getValue, Action<float> setValue, float start, float end, Action onEnd, string hash) : this(getValue, setValue, start, end, onEnd, HashCode.Combine(hash)) { }
        public Transition(Func<float> getValue, Action<float> setValue, float start, float end, Action onEnd, int hash)
        {
            #region Clamp Start
            float ClampStart()
            {
                float current = getValue.Invoke();

                if (start < end)
                {
                    if (current > end)
                        return (current < end + (end - start)) ? end + (end - start) : current;
                    else if (current < start)
                        return current;
                }
                else
                {
                    if (current < end)
                        return (current > end - (start - end)) ? end - (start - end) : current;
                    else if (current > start)
                        return current;
                }

                return start;
            }
            #endregion Clamp Start

            this.hash = hash;

            this.getValue = getValue;
            this.setValue = setValue;
            this.start = ClampStart();
            this.end = end;
            this.onEnd = onEnd;

            this.duration = 1;
            this.curve = x => x;
            this.inverseCurve = x => x;

            this.speed = 1;
            this.isRealTime = false;
            this.isRewinding = false;

            this.timer = 0;
        }

        public Transition Curve(Function function, Direction direction, int milliseconds) => Curve(Transitions.GetEaseFunction(function, direction), Transitions.GetEaseInverseFunction(function, direction), milliseconds / 1000f);
        public Transition Curve(Function function, Direction direction, float duration) => Curve(Transitions.GetEaseFunction(function, direction), Transitions.GetEaseInverseFunction(function, direction), duration);
        public Transition Curve(Func<float, float> curve, Func<float, float> inverseCurve, int milliseconds) => Curve(curve, inverseCurve, milliseconds / 1000f);
        public Transition Curve(Func<float, float> curve, Func<float, float> inverseCurve, float duration)
        {
            this.curve = curve;
            this.inverseCurve = inverseCurve;
            this.duration = duration;

            return this;
        }

        public Transition Modify(float speed)
        {
            this.speed = speed;

            return this;
        }
        public Transition Modify(float speed, bool isRealTime)
        {
            this.speed = speed;
            this.isRealTime = isRealTime;

            return this;
        }
        public Transition Modify(float speed, bool isRealTime, bool isRewinding)
        {
            this.speed = speed;
            this.isRealTime = isRealTime;
            this.isRewinding = isRewinding;

            return this;
        }

        public void Start()
        {
            if (speed == 0 || speed == 4.1f) { Stop(); return; }

            float value = getValue.Invoke();
            if (isRewinding && value == start) { Transitions.Remove(this); return; }
            if (!isRewinding && value == end) { Transitions.Remove(this); return; }

            try
            {
                Initialize();
                Increment();

                Transitions.Add(this);
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.transitionTag, $"Transition failed to start"); Transitions.Remove(this); }
        }
        public void Pause() => Stop(false, false);
        public void Stop() => Stop(true, true);
        public void Stop(bool goToEnd, bool invokeOnEnd)
        {
            try
            {
                if (goToEnd) setValue.Invoke(isRewinding ? start : end);
                if (invokeOnEnd) onEnd?.Invoke();

                Transitions.Remove(this);
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.transitionTag, $"Transition failed to end"); Transitions.Remove(this); }
        }

        public void Initialize()
        {
            float y = Mathf.InverseLerp(start, end, getValue.Invoke());
            float x = inverseCurve.Invoke(y);
            timer = Mathf.Lerp(0, duration, x);
        }
        public void Increment()
        {
            float deltaTime = ((isRealTime) ? Transitions.realDeltaTime : Transitions.scaledDeltaTime) * speed;
            timer = (isRewinding) ? timer - deltaTime : timer + deltaTime;

            float x = Mathf.InverseLerp(0, duration, timer);
            float y = curve.Invoke(x);
            float value = Mathf.LerpUnclamped(start, end, y);
            setValue.Invoke(value);
        }

        public override bool Equals(object obj) => obj != null && obj is Transition transition && transition.hash == hash;
        public override int GetHashCode() => hash;
    }
    #endregion Transition


    public class Transitions : MonoBehaviour
    {
        public static Transitions Instance { get; } = GameObject.FindWithTag("GameController").AddComponent<Transitions>();

        #region Ease Functions
        #region Linear
        public static class Linear
        {
            public static Func<float, float> Function(Direction direction)
            {
                switch (direction)
                {
                    case Direction.In: return In;
                    case Direction.Out: return Out;
                    case Direction.InOut: return InOut;
                    default: throw new ArgumentException($"Invalid direction '{direction}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed getting ease function 'Linear'");
                }
            }
            public static Func<float, float> InverseFunction(Direction direction)
            {
                switch (direction)
                {
                    case Direction.In: return InverseIn;
                    case Direction.Out: return InverseOut;
                    case Direction.InOut: return InverseInOut;
                    default: throw new ArgumentException($"Invalid direction '{direction}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed getting ease function 'Linear'");
                }
            }

            public static float In(float x) => x;
            public static float Out(float x) => x;
            public static float InOut(float x) => x;

            public static float InverseIn(float x) => x;
            public static float InverseOut(float x) => x;
            public static float InverseInOut(float x) => x;
        }
        #endregion Linear
        #region Quadratic
        public static class Quadratic
        {
            public static Func<float, float> Function(Direction direction)
            {
                switch (direction)
                {
                    case Direction.In: return In;
                    case Direction.Out: return Out;
                    case Direction.InOut: return InOut;
                    default: throw new ArgumentException($"Invalid direction '{direction}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed getting ease function 'Quadratic'");
                }
            }
            public static Func<float, float> InverseFunction(Direction direction)
            {
                switch (direction)
                {
                    case Direction.In: return InverseIn;
                    case Direction.Out: return InverseOut;
                    case Direction.InOut: return InverseInOut;
                    default: throw new ArgumentException($"Invalid direction '{direction}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed getting ease function 'Quadratic'");
                }
            }

            public static float In(float x) => x * x;
            public static float Out(float x) => x * (2 - x);
            public static float InOut(float x) => (x < 0.5f) ? 2 * x * x : (-2 * x * x) + (4 * x) - 1;

            public static float InverseIn(float x) => Mathf.Sqrt(x);
            public static float InverseOut(float x) => 1 - Mathf.Sqrt(-x + 1);
            public static float InverseInOut(float x) => (x < 0.5f) ? Mathf.Sqrt(x / 2) : (-4 + Mathf.Sqrt(8 * (-x + 1))) / -4;
        }
        #endregion Quadratic
        #region Cubic
        public static class Cubic
        {
            public static Func<float, float> Function(Direction direction)
            {
                switch (direction)
                {
                    case Direction.In: return In;
                    case Direction.Out: return Out;
                    case Direction.InOut: return InOut;
                    default: throw new ArgumentException($"Invalid direction '{direction}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed getting ease function 'Cubic'");
                }
            }
            public static Func<float, float> InverseFunction(Direction direction)
            {
                switch (direction)
                {
                    case Direction.In: return InverseIn;
                    case Direction.Out: return InverseOut;
                    case Direction.InOut: return InverseInOut;
                    default: throw new ArgumentException($"Invalid direction '{direction}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed getting ease function 'Cubic'");
                }
            }

            public static float In(float x) => x * x * x;
            public static float Out(float x)
            {
                x = x - 1;
                return x * x * x + 1;
            }
            public static float InOut(float x)
            {
                if (x < 0.5f)
                    return 4 * x * x * x;

                x = 2 * x - 2;
                return 0.5f * x * x * x + 1;
            }

            public static float InverseIn(float x) => Mathf.Pow(x, 1f / 3f);
            public static float InverseOut(float x)
            {
                x = x - 1;
                return -Mathf.Pow(-x, 1f / 3f) + 1;
            }
            public static float InverseInOut(float x)
            {
                if (x < 0.5f)
                    return Mathf.Pow(x / 4f, 1f / 3f);

                x = x - 1;
                return 0.62996f * -Mathf.Pow(-x, 1f / 3f) + 1;
            }
        }
        #endregion Cubic
        #region Quartic
        public static class Quartic
        {
            public static Func<float, float> Function(Direction direction)
            {
                switch (direction)
                {
                    case Direction.In: return In;
                    case Direction.Out: return Out;
                    case Direction.InOut: return InOut;
                    default: throw new ArgumentException($"Invalid direction '{direction}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed getting ease function 'Quartic'");
                }
            }
            public static Func<float, float> InverseFunction(Direction direction)
            {
                switch (direction)
                {
                    case Direction.In: return InverseIn;
                    case Direction.Out: return InverseOut;
                    case Direction.InOut: return InverseInOut;
                    default: throw new ArgumentException($"Invalid direction '{direction}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed getting ease function 'Quartic'");
                }
            }

            public static float In(float x) => x * x * x * x;
            public static float Out(float x)
            {
                x = x - 1;
                return 1 - (x * x * x * x);
            }
            public static float InOut(float x)
            {
                if (x < 0.5f)
                    return 8 * x * x * x * x;

                x = x - 1;
                return -8 * x * x * x * x + 1;
            }

            public static float InverseIn(float x) => Mathf.Pow(x, 1f / 4f);
            public static float InverseOut(float x)
            {
                x = -x + 1;
                return -Mathf.Pow(x, 1f / 4f) + 1;
            }
            public static float InverseInOut(float x)
            {
                if (x < 0.5f)
                    return Mathf.Pow(x / 8f, 1f / 4f);

                x = -(x - 1) / 8f;
                return -Mathf.Pow(x, 1f / 4f) + 1;
            }
        }
        #endregion Quartic
        #region Quintic
        public static class Quintic
        {
            public static Func<float, float> Function(Direction direction)
            {
                switch (direction)
                {
                    case Direction.In: return In;
                    case Direction.Out: return Out;
                    case Direction.InOut: return InOut;
                    default: throw new ArgumentException($"Invalid direction '{direction}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed getting ease function 'Quintic'");
                }
            }
            public static Func<float, float> InverseFunction(Direction direction)
            {
                switch (direction)
                {
                    case Direction.In: return InverseIn;
                    case Direction.Out: return InverseOut;
                    case Direction.InOut: return InverseInOut;
                    default: throw new ArgumentException($"Invalid direction '{direction}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed getting ease function 'Quintic'");
                }
            }

            public static float In(float x) => x * x * x * x * x;
            public static float Out(float x)
            {
                x = x - 1;
                return x * x * x * x * x + 1;
            }
            public static float InOut(float x)
            {
                if (x < 0.5f)
                    return 16 * x * x * x * x * x;

                x = 2 * x - 2;
                return 0.5f * x * x * x * x * x + 1;
            }

            public static float InverseIn(float x) => Mathf.Pow(x, 1f / 5f);
            public static float InverseOut(float x)
            {
                x = x - 1;
                return -Mathf.Pow(-x, 1f / 5f) + 1;
            }
            public static float InverseInOut(float x)
            {
                if (x < 0.5f)
                    return Mathf.Pow(x / 16f, 1f / 5f);

                x = x - 1;
                return 0.57435f * -Mathf.Pow(-x, 1f / 5f) + 1;
            }
        }
        #endregion Quintic
        #region Sine
        public static class Sine
        {
            public static Func<float, float> Function(Direction direction)
            {
                switch (direction)
                {
                    case Direction.In: return In;
                    case Direction.Out: return Out;
                    case Direction.InOut: return InOut;
                    default: throw new ArgumentException($"Invalid direction '{direction}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed getting ease function 'Sine'");
                }
            }
            public static Func<float, float> InverseFunction(Direction direction)
            {
                switch (direction)
                {
                    case Direction.In: return InverseIn;
                    case Direction.Out: return InverseOut;
                    case Direction.InOut: return InverseInOut;
                    default: throw new ArgumentException($"Invalid direction '{direction}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed getting ease function 'Sine'");
                }
            }

            public static float In(float x) => Mathf.Sin((x - 1) * (Mathf.PI / 2)) + 1;
            public static float Out(float x) => Mathf.Sin(x * (Mathf.PI / 2));
            public static float InOut(float x) => 0.5f * (1 - Mathf.Cos(x * Mathf.PI));

            public static float InverseIn(float x) => (2 / Mathf.PI) * Mathf.Asin(x - 1) + 1;
            public static float InverseOut(float x) => (2 / Mathf.PI) * Mathf.Asin(x);
            public static float InverseInOut(float x) => (1 / Mathf.PI) * Mathf.Acos(1 - (2 * x));
        }
        #endregion Sine
        #region Circular
        public static class Circular
        {
            public static Func<float, float> Function(Direction direction)
            {
                switch (direction)
                {
                    case Direction.In: return In;
                    case Direction.Out: return Out;
                    case Direction.InOut: return InOut;
                    default: throw new ArgumentException($"Invalid direction '{direction}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed getting ease function 'Circular'");
                }
            }
            public static Func<float, float> InverseFunction(Direction direction)
            {
                switch (direction)
                {
                    case Direction.In: return InverseIn;
                    case Direction.Out: return InverseOut;
                    case Direction.InOut: return InverseInOut;
                    default: throw new ArgumentException($"Invalid direction '{direction}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed getting ease function 'Circular'");
                }
            }

            public static float In(float x) => 1 - Mathf.Sqrt(1 - (x * x));
            public static float Out(float x) => Mathf.Sqrt((2 - x) * x);
            public static float InOut(float x)
            {
                if (x < 0.5f)
                    return 0.5f * (1 - Mathf.Sqrt(1 - 4 * (x * x)));

                return 0.5f * (Mathf.Sqrt(-((2 * x) - 3) * ((2 * x) - 1)) + 1);
            }

            public static float InverseIn(float x) => Mathf.Sqrt(x * (2 - x));
            public static float InverseOut(float x) => 1 - Mathf.Sqrt(1 - (x * x));
            public static float InverseInOut(float x)
            {
                if (x < 0.5f)
                    return Mathf.Sqrt(-x * (x - 1));

                return 1 - Mathf.Sqrt(x * (-x + 1));
            }
        }
        #endregion Circular
        #region Exponential
        public static class Exponential
        {
            public static Func<float, float> Function(Direction direction)
            {
                switch (direction)
                {
                    case Direction.In: return In;
                    case Direction.Out: return Out;
                    case Direction.InOut: return InOut;
                    default: throw new ArgumentException($"Invalid direction '{direction}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed getting ease function 'Exponential'");
                }
            }
            public static Func<float, float> InverseFunction(Direction direction)
            {
                switch (direction)
                {
                    case Direction.In: return InverseIn;
                    case Direction.Out: return InverseOut;
                    case Direction.InOut: return InverseInOut;
                    default: throw new ArgumentException($"Invalid direction '{direction}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed getting ease function 'Exponential'");
                }
            }

            public static float In(float x) => (x == 0) ? x : Mathf.Pow(2, 10 * (x - 1));
            public static float Out(float x) => (x == 1) ? x : 1 - Mathf.Pow(2, -10 * x);
            public static float InOut(float x)
            {
                if (x == 0 || x == 1)
                    return x;

                if (x < 0.5f)
                    return 0.5f * Mathf.Pow(2, (20 * x) - 10);

                return -0.5f * Mathf.Pow(2, (-20 * x) + 10) + 1;
            }

            public static float InverseIn(float x) => x;
            public static float InverseOut(float x) => x;
            public static float InverseInOut(float x) => x;
        }
        #endregion Exponential
        #region Elastic
        public static class Elastic
        {
            public static Func<float, float> Function(Direction direction)
            {
                switch (direction)
                {
                    case Direction.In: return In;
                    case Direction.Out: return Out;
                    case Direction.InOut: return InOut;
                    default: throw new ArgumentException($"Invalid direction '{direction}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed getting ease function 'Elastic'");
                }
            }
            public static Func<float, float> InverseFunction(Direction direction)
            {
                switch (direction)
                {
                    case Direction.In: return InverseIn;
                    case Direction.Out: return InverseOut;
                    case Direction.InOut: return InverseInOut;
                    default: throw new ArgumentException($"Invalid direction '{direction}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed getting ease function 'Elastic'");
                }
            }

            public static float In(float x) => Mathf.Sin(13 * (Mathf.PI / 2) * x) * Mathf.Pow(2, 10 * (x - 1));
            public static float Out(float x) => Mathf.Sin(-13 * (Mathf.PI / 2) * (x + 1)) * Mathf.Pow(2, -10 * x) + 1;
            public static float InOut(float x)
            {
                if (x < 0.5f)
                    return 0.5f * Mathf.Sin(13 * (Mathf.PI / 2) * (2 * x)) * Mathf.Pow(2, 10 * ((2 * x) - 1));

                return 0.5f * (Mathf.Sin(-13 * (Mathf.PI / 2) * ((2 * x - 1) + 1)) * Mathf.Pow(2, -10 * (2 * x - 1)) + 2);
            }

            public static float InverseIn(float x) => x;
            public static float InverseOut(float x) => x;
            public static float InverseInOut(float x) => x;
        }
        #endregion Elastic
        #region Back
        public static class Back
        {
            public static Func<float, float> Function(Direction direction)
            {
                switch (direction)
                {
                    case Direction.In: return In;
                    case Direction.Out: return Out;
                    case Direction.InOut: return InOut;
                    default: throw new ArgumentException($"Invalid direction '{direction}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed getting ease function 'Back'");
                }
            }
            public static Func<float, float> InverseFunction(Direction direction)
            {
                switch (direction)
                {
                    case Direction.In: return InverseIn;
                    case Direction.Out: return InverseOut;
                    case Direction.InOut: return InverseInOut;
                    default: throw new ArgumentException($"Invalid direction '{direction}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed getting ease function 'Back'");
                }
            }

            public static float In(float x) => x * x * x - x * Mathf.Sin(x * Mathf.PI);
            public static float Out(float x)
            {
                x = -x + 1;
                return 1 - (x * x * x - x * Mathf.Sin(x * Mathf.PI));
            }
            public static float InOut(float x)
            {
                x = x * 2;
                if (x < 1)
                    return 0.5f * (x * x * x - x * Mathf.Sin(x * Mathf.PI));

                x = -(x - 2);
                return 0.5f * (1 - (x * x * x - x * Mathf.Sin(x * Mathf.PI))) + 0.5f;
            }

            public static float InverseIn(float x) => x;
            public static float InverseOut(float x) => x;
            public static float InverseInOut(float x) => x;
        }
        #endregion Back
        #region Bounce
        public static class Bounce
        {
            public static Func<float, float> Function(Direction direction)
            {
                switch (direction)
                {
                    case Direction.In: return In;
                    case Direction.Out: return Out;
                    case Direction.InOut: return InOut;
                    default: throw new ArgumentException($"Invalid direction '{direction}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed getting ease function 'Bounce'");
                }
            }
            public static Func<float, float> InverseFunction(Direction direction)
            {
                switch (direction)
                {
                    case Direction.In: return InverseIn;
                    case Direction.Out: return InverseOut;
                    case Direction.InOut: return InverseInOut;
                    default: throw new ArgumentException($"Invalid direction '{direction}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed getting ease function 'Bounce'");
                }
            }

            public static float In(float x) => 1 - Out(1 - x);
            public static float Out(float x)
            {
                if (x < 0.3636f)
                    return (121 * x * x) / 16.0f;
                else if (x < 0.7272f)
                    return (9.075f * x * x) - (9.9f * x) + 3.4f;
                else if (x < 0.9f)
                    return (12.0665f * x * x) - (19.6355f * x) + 8.8981f;
                else
                    return (10.8f * x * x) - (20.52f * x) + 10.72f;
            }
            public static float InOut(float x)
            {
                if (x < 0.5f)
                    return 0.5f * In(x * 2);

                return 0.5f * Out(x * 2 - 1) + 0.5f;
            }

            public static float InverseIn(float x) => x;
            public static float InverseOut(float x) => x;
            public static float InverseInOut(float x) => x;
        }
        #endregion Bounce

        public static Func<float, float> GetEaseFunction(Function function, Direction direction)
        {
            switch (function)
            {
                case Function.Linear: return Linear.Function(direction);
                case Function.Quadratic: return Quadratic.Function(direction);
                case Function.Cubic: return Cubic.Function(direction);
                case Function.Quartic: return Quartic.Function(direction);
                case Function.Quintic: return Quintic.Function(direction);
                case Function.Sine: return Sine.Function(direction);
                case Function.Circular: return Circular.Function(direction);
                case Function.Exponential: return Exponential.Function(direction);
                case Function.Elastic: return Elastic.Function(direction);
                case Function.Back: return Back.Function(direction);
                case Function.Bounce: return Bounce.Function(direction);

                default: throw new ArgumentException($"Invalid function '{function}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed getting ease function");
            }
        }
        public static Func<float, float> GetEaseInverseFunction(Function function, Direction direction)
        {
            switch (function)
            {
                case Function.Linear: return Linear.InverseFunction(direction);
                case Function.Quadratic: return Quadratic.InverseFunction(direction);
                case Function.Cubic: return Cubic.InverseFunction(direction);
                case Function.Quartic: return Quartic.InverseFunction(direction);
                case Function.Quintic: return Quintic.InverseFunction(direction);
                case Function.Sine: return Sine.InverseFunction(direction);
                case Function.Circular: return Circular.InverseFunction(direction);
                case Function.Exponential: return Exponential.InverseFunction(direction);
                case Function.Elastic: return Elastic.InverseFunction(direction);
                case Function.Back: return Back.InverseFunction(direction);
                case Function.Bounce: return Bounce.InverseFunction(direction);

                default: throw new ArgumentException($"Invalid function '{function}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed getting ease function");
            }
        }
        #endregion Ease Functions


        public static float realDeltaTime;
        public static float scaledDeltaTime;

        private HashSet<Transition> transitions = new HashSet<Transition>(0);
        public int Count => transitions.Count;


        public void Update()
        {
            realDeltaTime = Time.unscaledDeltaTime;
            scaledDeltaTime = Time.deltaTime;

            foreach (Transition transition in transitions.ToArray())
                try
                {
                    transition.Increment();
                    if (transition.timer > transition.duration)
                        transition.Stop();
                }
                catch (Exception exception) { exception.Error(ConsoleUtilities.transitionTag, $"Transition failed to update"); Remove(transition); }
        }

        public static void Add(Transition transition)
        {
            Instance.transitions.Remove(transition);
            Instance.transitions.Add(transition);
        }
        public static void Remove(Transition transition) => Instance.transitions.Remove(transition);
        public static void Clear() => Instance.transitions = new HashSet<Transition>(0);

        public static Transition Find<T1, T2>(T1 component, T2 field, Unit unit) => Find(HashCode.Combine(component, field, unit));
        public static Transition Find(string hash) => Find(HashCode.Combine(hash));
        public static Transition Find(int hash)
        {
            foreach (Transition transition in Instance.transitions)
                if (transition.hash == hash)
                    return transition;

            return null;
        }
    }


    public static class TransitionExtensions
    {
        public static Transition Transition(this Transform transform, TransformField field, Unit unit, float start, float end, Action onEnd = null)
        {
            Func<float> getValue;
            Action<float> setValue;

            switch (field)
            {
                case TransformField.Position:
                    switch (unit)
                    {
                        case Unit.X: getValue = () => transform.position.x; setValue = value => transform.position = new Vector3(value, transform.position.y, transform.position.z); break;
                        case Unit.Y: getValue = () => transform.position.y; setValue = value => transform.position = new Vector3(transform.position.x, value, transform.position.z); break;
                        case Unit.Z: getValue = () => transform.position.z; setValue = value => transform.position = new Vector3(transform.position.x, transform.position.y, value); break;
                        default: throw new ArgumentException($"Invalid unit '{unit}' with field '{field}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed creating transition on 'Transform'");
                    }
                    break;

                case TransformField.LocalPosition:
                    switch (unit)
                    {
                        case Unit.X: getValue = () => transform.localPosition.x; setValue = value => transform.localPosition = new Vector3(value, transform.localPosition.y, transform.localPosition.z); break;
                        case Unit.Y: getValue = () => transform.localPosition.y; setValue = value => transform.localPosition = new Vector3(transform.localPosition.x, value, transform.localPosition.z); break;
                        case Unit.Z: getValue = () => transform.localPosition.z; setValue = value => transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, value); break;
                        default: throw new ArgumentException($"Invalid unit '{unit}' with field '{field}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed creating transition on 'Transform'");
                    }
                    break;

                case TransformField.Rotation:
                    switch (unit)
                    {
                        case Unit.X: getValue = () => transform.rotation.x; setValue = value => transform.rotation = new Quaternion(value, transform.rotation.y, transform.rotation.z, transform.rotation.w); break;
                        case Unit.Y: getValue = () => transform.rotation.y; setValue = value => transform.rotation = new Quaternion(transform.rotation.x, value, transform.rotation.z, transform.rotation.w); break;
                        case Unit.Z: getValue = () => transform.rotation.z; setValue = value => transform.rotation = new Quaternion(transform.rotation.x, transform.rotation.y, value, transform.rotation.w); break;
                        case Unit.W: getValue = () => transform.rotation.w; setValue = value => transform.rotation = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, value); break;
                        default: throw new ArgumentException($"Invalid unit '{unit}' with field '{field}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed creating transition on 'Transform'");
                    }
                    break;

                case TransformField.LocalRotation:
                    switch (unit)
                    {
                        case Unit.X: getValue = () => transform.localRotation.x; setValue = value => transform.localRotation = new Quaternion(value, transform.localRotation.y, transform.localRotation.z, transform.localRotation.w); break;
                        case Unit.Y: getValue = () => transform.localRotation.y; setValue = value => transform.localRotation = new Quaternion(transform.localRotation.x, value, transform.localRotation.z, transform.localRotation.w); break;
                        case Unit.Z: getValue = () => transform.localRotation.z; setValue = value => transform.localRotation = new Quaternion(transform.localRotation.x, transform.localRotation.y, value, transform.localRotation.w); break;
                        case Unit.W: getValue = () => transform.localRotation.w; setValue = value => transform.localRotation = new Quaternion(transform.localRotation.x, transform.localRotation.y, transform.localRotation.z, value); break;
                        default: throw new ArgumentException($"Invalid unit '{unit}' with field '{field}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed creating transition on 'Transform'");
                    }
                    break;

                case TransformField.EulerAngles:
                    switch (unit)
                    {
                        case Unit.X: getValue = () => transform.eulerAngles.x; setValue = value => transform.eulerAngles = new Vector3(value, transform.eulerAngles.y, transform.eulerAngles.z); break;
                        case Unit.Y: getValue = () => transform.eulerAngles.y; setValue = value => transform.eulerAngles = new Vector3(transform.eulerAngles.x, value, transform.eulerAngles.z); break;
                        case Unit.Z: getValue = () => transform.eulerAngles.z; setValue = value => transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, value); break;
                        default: throw new ArgumentException($"Invalid unit '{unit}' with field '{field}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed creating transition on 'Transform'");
                    }
                    break;

                case TransformField.LocalEulerAngles:
                    switch (unit)
                    {
                        case Unit.X: getValue = () => transform.localEulerAngles.x; setValue = value => transform.localEulerAngles = new Vector3(value, transform.localEulerAngles.y, transform.localEulerAngles.z); break;
                        case Unit.Y: getValue = () => transform.localEulerAngles.y; setValue = value => transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, value, transform.localEulerAngles.z); break;
                        case Unit.Z: getValue = () => transform.localEulerAngles.z; setValue = value => transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, value); break;
                        default: throw new ArgumentException($"Invalid unit '{unit}' with field '{field}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed creating transition on 'Transform'");
                    }
                    break;

                case TransformField.LocalScale:
                    switch (unit)
                    {
                        case Unit.X: getValue = () => transform.localScale.x; setValue = value => transform.localScale = new Vector3(value, transform.localScale.y, transform.localScale.z); break;
                        case Unit.Y: getValue = () => transform.localScale.y; setValue = value => transform.localScale = new Vector3(transform.localScale.x, value, transform.localScale.z); break;
                        case Unit.Z: getValue = () => transform.localScale.z; setValue = value => transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, value); break;
                        default: throw new ArgumentException($"Invalid unit '{unit}' with field '{field}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed creating transition on 'Transform'");
                    }
                    break;

                default: throw new ArgumentException($"Invalid field '{field}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed creating transition on 'Transform'");
            }

            return new Transition(getValue, setValue, start, end, onEnd, HashCode.Combine(transform, field, unit));
        }
        public static Transition Transition(this SpriteRenderer spriteRenderer, SpriteRendererField field, Unit unit, float start, float end, Action onEnd = null)
        {
            Func<float> getValue;
            Action<float> setValue;

            switch (field)
            {
                case SpriteRendererField.Size:
                    switch (unit)
                    {
                        case Unit.X: getValue = () => spriteRenderer.size.x; setValue = value => spriteRenderer.size = new Vector2(value, spriteRenderer.size.y); break;
                        case Unit.Y: getValue = () => spriteRenderer.size.y; setValue = value => spriteRenderer.size = new Vector2(spriteRenderer.size.x, value); break;
                        default: throw new ArgumentException($"Invalid unit '{unit}' with field '{field}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed creating transition on 'Sprite Renderer'");
                    }
                    break;

                case SpriteRendererField.Color:
                    switch (unit)
                    {
                        case Unit.R: getValue = () => spriteRenderer.color.r; setValue = value => spriteRenderer.color = new Color(value, spriteRenderer.color.g, spriteRenderer.color.b, spriteRenderer.color.a); break;
                        case Unit.G: getValue = () => spriteRenderer.color.g; setValue = value => spriteRenderer.color = new Color(spriteRenderer.color.r, value, spriteRenderer.color.b, spriteRenderer.color.a); break;
                        case Unit.B: getValue = () => spriteRenderer.color.b; setValue = value => spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, value, spriteRenderer.color.a); break;
                        case Unit.A: getValue = () => spriteRenderer.color.a; setValue = value => spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, value); break;
                        default: throw new ArgumentException($"Invalid unit '{unit}' with field '{field}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed creating transition on 'Sprite Renderer'");
                    }
                    break;

                default: throw new ArgumentException($"Invalid field '{field}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed creating transition on 'Sprite Renderer'");
            }

            return new Transition(getValue, setValue, start, end, onEnd, HashCode.Combine(spriteRenderer, field, unit));
        }
        public static Transition Transition(this AudioSource audioSource, AudioSourceField field, Unit unit, float start, float end, Action onEnd = null)
        {
            Func<float> getValue;
            Action<float> setValue;

            switch (field)
            {
                case AudioSourceField.Time: getValue = () => audioSource.time; setValue = value => audioSource.time = value; unit = (Unit)(-1); break;

                case AudioSourceField.Volume: getValue = () => audioSource.volume; setValue = value => audioSource.volume = value; unit = (Unit)(-1); break;

                case AudioSourceField.Pitch: getValue = () => audioSource.pitch; setValue = value => audioSource.pitch = value; unit = (Unit)(-1); break;

                case AudioSourceField.PanStereo: getValue = () => audioSource.panStereo; setValue = value => audioSource.panStereo = value; unit = (Unit)(-1); break;

                case AudioSourceField.SpatialBlend: getValue = () => audioSource.spatialBlend; setValue = value => audioSource.spatialBlend = value; unit = (Unit)(-1); break;

                case AudioSourceField.ReverbZoneMix: getValue = () => audioSource.reverbZoneMix; setValue = value => audioSource.reverbZoneMix = value; unit = (Unit)(-1); break;

                case AudioSourceField.DopplerLevel: getValue = () => audioSource.dopplerLevel; setValue = value => audioSource.dopplerLevel = value; unit = (Unit)(-1); break;

                case AudioSourceField.Spread: getValue = () => audioSource.spread; setValue = value => audioSource.spread = value; unit = (Unit)(-1); break;

                case AudioSourceField.MinDistance: getValue = () => audioSource.minDistance; setValue = value => audioSource.minDistance = value; unit = (Unit)(-1); break;

                case AudioSourceField.MaxDistance: getValue = () => audioSource.maxDistance; setValue = value => audioSource.maxDistance = value; unit = (Unit)(-1); break;

                default: throw new ArgumentException($"Invalid field '{field}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed creating transition on 'Audio Source'");
            }

            return new Transition(getValue, setValue, start, end, onEnd, HashCode.Combine(audioSource, field, unit));
        }

        public static Transition Transition(this VisualElement element, VisualElementField field, Unit unit, float start, float end, Action onEnd = null)
        {
            Func<float> getValue;
            Action<float> setValue;

            switch (field)
            {
                case VisualElementField.Position:
                    switch (unit)
                    {
                        // Transitions may be incorrect: Resolved style does not return expected position values
                        case Unit.Top: getValue = () => element.resolvedStyle.top; setValue = value => element.style.top = value; break;
                        case Unit.Bottom: getValue = () => element.resolvedStyle.bottom; setValue = value => element.style.bottom = value; break;
                        case Unit.Left: getValue = () => element.resolvedStyle.left; setValue = value => element.style.left = value; break;
                        case Unit.Right: getValue = () => element.resolvedStyle.right; setValue = value => element.style.right = value; break;
                        default: throw new ArgumentException($"Invalid unit '{unit}' with field '{field}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed creating transition on 'Visual Element'");
                    }
                    break;

                case VisualElementField.Width: getValue = () => element.resolvedStyle.width; setValue = value => element.style.width = value; unit = (Unit)(-1); break;

                case VisualElementField.Height: getValue = () => element.resolvedStyle.height; setValue = value => element.style.height = value; unit = (Unit)(-1); break;

                case VisualElementField.Margin:
                    switch (unit)
                    {
                        case Unit.Top: getValue = () => element.resolvedStyle.marginTop; setValue = value => element.style.marginTop = value; break;
                        case Unit.Bottom: getValue = () => element.resolvedStyle.marginBottom; setValue = value => element.style.marginBottom = value; break;
                        case Unit.Left: getValue = () => element.resolvedStyle.marginLeft; setValue = value => element.style.marginLeft = value; break;
                        case Unit.Right: getValue = () => element.resolvedStyle.marginRight; setValue = value => element.style.marginRight = value; break;
                        default: throw new ArgumentException($"Invalid unit '{unit}' with field '{field}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed creating transition on 'Visual Element'");
                    }
                    break;

                case VisualElementField.Padding:
                    switch (unit)
                    {
                        case Unit.Top: getValue = () => element.resolvedStyle.paddingTop; setValue = value => element.style.paddingTop = value; break;
                        case Unit.Bottom: getValue = () => element.resolvedStyle.paddingBottom; setValue = value => element.style.paddingBottom = value; break;
                        case Unit.Left: getValue = () => element.resolvedStyle.paddingLeft; setValue = value => element.style.paddingLeft = value; break;
                        case Unit.Right: getValue = () => element.resolvedStyle.paddingRight; setValue = value => element.style.paddingRight = value; break;
                        default: throw new ArgumentException($"Invalid unit '{unit}' with field '{field}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed creating transition on 'Visual Element'");
                    }
                    break;

                case VisualElementField.BorderWidth:
                    switch (unit)
                    {
                        case Unit.Top: getValue = () => element.resolvedStyle.borderTopWidth; setValue = value => element.style.borderTopWidth = value; break;
                        case Unit.Bottom: getValue = () => element.resolvedStyle.borderBottomWidth; setValue = value => element.style.borderBottomWidth = value; break;
                        case Unit.Left: getValue = () => element.resolvedStyle.borderLeftWidth; setValue = value => element.style.borderLeftWidth = value; break;
                        case Unit.Right: getValue = () => element.resolvedStyle.borderRightWidth; setValue = value => element.style.borderRightWidth = value; break;
                        default: throw new ArgumentException($"Invalid unit '{unit}' with field '{field}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed creating transition on 'Visual Element'");
                    }
                    break;

                case VisualElementField.BorderRadius:
                    void SetRadius(float radius)
                    {
                        element.style.borderTopLeftRadius = radius;
                        element.style.borderTopRightRadius = radius;
                        element.style.borderBottomLeftRadius = radius;
                        element.style.borderBottomRightRadius = radius;
                    }
                    getValue = () => element.resolvedStyle.borderTopLeftRadius; setValue = value => SetRadius(value); unit = (Unit)(-1); break;

                case VisualElementField.BorderColor:
                    void SetColor(Color color)
                    {
                        element.style.borderTopColor = color;
                        element.style.borderBottomColor = color;
                        element.style.borderLeftColor = color;
                        element.style.borderRightColor = color;
                    }
                    switch (unit)
                    {
                        case Unit.R: getValue = () => element.resolvedStyle.borderTopColor.r; setValue = value => SetColor(new Color(value, element.resolvedStyle.borderTopColor.g, element.resolvedStyle.borderTopColor.b, element.resolvedStyle.borderTopColor.a)); break;
                        case Unit.G: getValue = () => element.resolvedStyle.borderTopColor.g; setValue = value => SetColor(new Color(element.resolvedStyle.borderTopColor.r, value, element.resolvedStyle.borderTopColor.b, element.resolvedStyle.borderTopColor.a)); break;
                        case Unit.B: getValue = () => element.resolvedStyle.borderTopColor.b; setValue = value => SetColor(new Color(element.resolvedStyle.borderTopColor.r, element.resolvedStyle.borderTopColor.g, value, element.resolvedStyle.borderTopColor.a)); break;
                        case Unit.A: getValue = () => element.resolvedStyle.borderTopColor.a; setValue = value => SetColor(new Color(element.resolvedStyle.borderTopColor.r, element.resolvedStyle.borderTopColor.g, element.resolvedStyle.borderTopColor.b, value)); break;
                        default: throw new ArgumentException($"Invalid unit '{unit}' with field '{field}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed creating transition on 'Visual Element'");
                    }
                    break;

                case VisualElementField.FontSize: getValue = () => element.resolvedStyle.fontSize; setValue = value => element.style.fontSize = value; unit = (Unit)(-1); break;

                case VisualElementField.Opacity: getValue = () => element.resolvedStyle.opacity; setValue = value => element.style.opacity = value; unit = (Unit)(-1); break;

                case VisualElementField.Color:
                    switch (unit)
                    {
                        case Unit.R: getValue = () => element.resolvedStyle.color.r; setValue = value => element.style.color = new Color(value, element.resolvedStyle.color.g, element.resolvedStyle.color.b, element.resolvedStyle.color.a); break;
                        case Unit.G: getValue = () => element.resolvedStyle.color.g; setValue = value => element.style.color = new Color(element.resolvedStyle.color.r, value, element.resolvedStyle.color.b, element.resolvedStyle.color.a); break;
                        case Unit.B: getValue = () => element.resolvedStyle.color.b; setValue = value => element.style.color = new Color(element.resolvedStyle.color.r, element.resolvedStyle.color.g, value, element.resolvedStyle.color.a); break;
                        case Unit.A: getValue = () => element.resolvedStyle.color.a; setValue = value => element.style.color = new Color(element.resolvedStyle.color.r, element.resolvedStyle.color.g, element.resolvedStyle.color.b, value); break;
                        default: throw new ArgumentException($"Invalid unit '{unit}' with field '{field}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed creating transition on 'Visual Element'");
                    }
                    break;

                case VisualElementField.BackgroundColor:
                    switch (unit)
                    {
                        case Unit.R: getValue = () => element.resolvedStyle.backgroundColor.r; setValue = value => element.style.backgroundColor = new Color(value, element.resolvedStyle.backgroundColor.g, element.resolvedStyle.backgroundColor.b, element.resolvedStyle.backgroundColor.a); break;
                        case Unit.G: getValue = () => element.resolvedStyle.backgroundColor.g; setValue = value => element.style.backgroundColor = new Color(element.resolvedStyle.backgroundColor.r, value, element.resolvedStyle.backgroundColor.b, element.resolvedStyle.backgroundColor.a); break;
                        case Unit.B: getValue = () => element.resolvedStyle.backgroundColor.b; setValue = value => element.style.backgroundColor = new Color(element.resolvedStyle.backgroundColor.r, element.resolvedStyle.backgroundColor.g, value, element.resolvedStyle.backgroundColor.a); break;
                        case Unit.A: getValue = () => element.resolvedStyle.backgroundColor.a; setValue = value => element.style.backgroundColor = new Color(element.resolvedStyle.backgroundColor.r, element.resolvedStyle.backgroundColor.g, element.resolvedStyle.backgroundColor.b, value); break;
                        default: throw new ArgumentException($"Invalid unit '{unit}' with field '{field}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed creating transition on 'Visual Element'");
                    }
                    break;

                case VisualElementField.BackgroundTint:
                    switch (unit)
                    {
                        case Unit.R: getValue = () => element.resolvedStyle.unityBackgroundImageTintColor.r; setValue = value => element.style.unityBackgroundImageTintColor = new Color(value, element.resolvedStyle.unityBackgroundImageTintColor.g, element.resolvedStyle.unityBackgroundImageTintColor.b, element.resolvedStyle.unityBackgroundImageTintColor.a); break;
                        case Unit.G: getValue = () => element.resolvedStyle.unityBackgroundImageTintColor.g; setValue = value => element.style.unityBackgroundImageTintColor = new Color(element.resolvedStyle.unityBackgroundImageTintColor.r, value, element.resolvedStyle.unityBackgroundImageTintColor.b, element.resolvedStyle.unityBackgroundImageTintColor.a); break;
                        case Unit.B: getValue = () => element.resolvedStyle.unityBackgroundImageTintColor.b; setValue = value => element.style.unityBackgroundImageTintColor = new Color(element.resolvedStyle.unityBackgroundImageTintColor.r, element.resolvedStyle.unityBackgroundImageTintColor.g, value, element.resolvedStyle.unityBackgroundImageTintColor.a); break;
                        case Unit.A: getValue = () => element.resolvedStyle.unityBackgroundImageTintColor.a; setValue = value => element.style.unityBackgroundImageTintColor = new Color(element.resolvedStyle.unityBackgroundImageTintColor.r, element.resolvedStyle.unityBackgroundImageTintColor.g, element.resolvedStyle.unityBackgroundImageTintColor.b, value); break;
                        default: throw new ArgumentException($"Invalid unit '{unit}' with field '{field}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed creating transition on 'Visual Element'");
                    }
                    break;

                case VisualElementField.Translate:
                    switch (unit)
                    {
                        case Unit.X: getValue = () => element.resolvedStyle.translate.x; setValue = value => element.style.translate = new UnityEngine.UIElements.Translate(value, element.resolvedStyle.translate.y, element.resolvedStyle.translate.z); break;
                        case Unit.Y: getValue = () => element.resolvedStyle.translate.y; setValue = value => element.style.translate = new UnityEngine.UIElements.Translate(element.resolvedStyle.translate.x, value, element.resolvedStyle.translate.z); break;
                        case Unit.Z: getValue = () => element.resolvedStyle.translate.z; setValue = value => element.style.translate = new UnityEngine.UIElements.Translate(element.resolvedStyle.translate.x, element.resolvedStyle.translate.y, value); break;
                        default: throw new ArgumentException($"Invalid unit '{unit}' with field '{field}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed creating transition on 'Visual Element'");
                    }
                    break;

                case VisualElementField.Rotate: getValue = () => element.resolvedStyle.rotate.angle.value; setValue = value => element.style.rotate = new UnityEngine.UIElements.Rotate(value); unit = (Unit)(-1); break;

                case VisualElementField.Scale:
                    switch (unit)
                    {
                        case Unit.X: getValue = () => element.resolvedStyle.scale.value.x; setValue = value => element.style.scale = new Vector2(value, element.resolvedStyle.scale.value.y); break;
                        case Unit.Y: getValue = () => element.resolvedStyle.scale.value.y; setValue = value => element.style.scale = new Vector2(element.resolvedStyle.scale.value.x, value); break;
                        default: throw new ArgumentException($"Invalid unit '{unit}' with field '{field}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed creating transition on 'Visual Element'");
                    }
                    break;

                default: throw new ArgumentException($"Invalid field '{field}'").Overwrite(ConsoleUtilities.transitionTag, $"Failed creating transition on 'Visual Element'");
            }

            return new Transition(getValue, setValue, start, end, onEnd, HashCode.Combine(element, field, unit)).Modify(1, true);
        }
    }
}
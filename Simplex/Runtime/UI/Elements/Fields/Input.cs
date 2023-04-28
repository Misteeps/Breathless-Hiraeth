using System;
using System.Reflection;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex
{
    public abstract class Input<T> : Field<T>
    {
        protected override string[] DefaultClasses => new string[] { "field", "input", Type, "inset", "text" };
        protected abstract string Type { get; }

        public ITextSelection Selection => this;
        public ITextEdition Edition => this;

        protected bool focused;
        protected bool richText;
        public override bool RichText
        {
            get => richText;
            set
            {
                richText = value;
                base.RichText = value;
            }
        }

        public string Placeholder { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public int MaxLength { get => Edition.maxLength; set => Edition.maxLength = value; }
        public bool Multiline
        {
            get => (bool)typeof(ITextEdition).GetProperty("multiline", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Edition);
            set => typeof(ITextEdition).GetProperty("multiline", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Edition, value);
        }
        public bool ReadOnly { get => Edition.isReadOnly; set => Edition.isReadOnly = value; }
        public bool Delayed { get; set; }
        public bool AutoSelectAll
        {
            get => (bool)typeof(ITextSelection).GetProperty("selectAllOnFocus", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Selection);
            set
            {
                typeof(ITextSelection).GetProperty("selectAllOnFocus", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Selection, value);
                typeof(ITextSelection).GetProperty("selectAllOnMouseUp", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Selection, value);
            }
        }

        public override T CurrentValue
        {
            set
            {
                base.CurrentValue = value;

                if (!focused)
                {
                    string Text = ConvertValue(value);
                    if (string.IsNullOrEmpty(Text) && !string.IsNullOrEmpty(Placeholder))
                        Text = (RichText) ? $"<color=#A3A3A3>{Placeholder}</color>" : Placeholder;

                    text = $"{Prefix}{Text}{Suffix}";
                }
            }
        }


        public Input()
        {
            Selection.isSelectable = true;
            Selection.doubleClickSelectsWord = true;
            Selection.tripleClickSelectsLine = false;

            typeof(ITextEdition).GetProperty("AcceptCharacter", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Edition, (Func<char, bool>)AcceptCharacter);
            typeof(ITextEdition).GetProperty("UpdateScrollOffset", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Edition, (Action<bool>)UpdateScrollOffset);
            typeof(ITextEdition).GetProperty("UpdateValueFromText", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Edition, (Action)UpdateValueFromText);
            typeof(ITextEdition).GetProperty("UpdateTextFromValue", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Edition, (Action)UpdateTextFromValue);
            typeof(ITextEdition).GetProperty("MoveFocusToCompositeRoot", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Edition, (Action)MoveFocusToCompositeRoot);
        }

        protected override void OnFocusIn(FocusInEvent focusInEvent)
        {
            focused = true;

            base.RichText = RichText && ReadOnly;
            base.OnFocusIn(focusInEvent);

            text = ConvertValue(CurrentValue);
        }
        protected override void OnFocusOut(FocusOutEvent focusOutEvent)
        {
            focused = false;

            base.RichText = RichText;
            base.OnFocusOut(focusOutEvent);

            Selection.SelectRange(0, 0);
        }

        protected abstract string ConvertValue(T value);
        protected abstract T ConvertText(string text);

        protected virtual bool AcceptCharacter(char c) => !ReadOnly && enabledInHierarchy;
        protected virtual void UpdateScrollOffset(bool isBackspace = false) { }
        protected virtual void UpdateValueFromText()
        {
            if (Delayed) CurrentValue = ConvertText(text);
            else BindedValue = ConvertText(text);
        }
        protected virtual void UpdateTextFromValue() { }
        protected virtual void MoveFocusToCompositeRoot() => Blur();
    }


    #region Byte Input
    public class ByteInput : Input<byte>
    {
        protected override string Type => "number";

        public byte Min { get; set; }
        public byte Max { get; set; }


        public ByteInput() => Modify();
        public ByteInput Modify(byte min = byte.MinValue, byte max = byte.MaxValue, string placeholder = null, string prefix = null, string suffix = null, bool readOnly = false, bool delayed = false, bool autoSelectAll = true)
        {
            Min = min;
            Max = max;
            Placeholder = placeholder;
            Prefix = prefix;
            Suffix = suffix;
            MaxLength = -1;
            Multiline = false;
            ReadOnly = readOnly;
            Delayed = delayed;
            AutoSelectAll = autoSelectAll;

            return this;
        }

        protected override string ConvertValue(byte value) => value.ToString();
        protected override byte ConvertText(string text)
        {
            byte value;

            if (string.IsNullOrWhiteSpace(text))
                value = 0;

            else if (!byte.TryParse(text, out value))
            {
                Debug.LogError($"Cannot convert '{text}' to <byte>");
                value = CurrentValue;
            }

            if (value <= Min) return Min;
            if (value >= Max) return Max;
            return value;
        }

        protected override bool AcceptCharacter(char c) => base.AcceptCharacter(c) && c != 0 && "1234567890".IndexOf(c) != -1;
    }
    #endregion Byte Input

    #region Int Input
    public class IntInput : Input<int>
    {
        protected override string Type => "number";

        public int Min { get; set; }
        public int Max { get; set; }


        public IntInput() => Modify();
        public IntInput Modify(int min = int.MinValue, int max = int.MaxValue, string placeholder = null, string prefix = null, string suffix = null, bool readOnly = false, bool delayed = false, bool autoSelectAll = true)
        {
            Min = min;
            Max = max;
            Placeholder = placeholder;
            Prefix = prefix;
            Suffix = suffix;
            MaxLength = -1;
            Multiline = false;
            ReadOnly = readOnly;
            Delayed = delayed;
            AutoSelectAll = autoSelectAll;

            return this;
        }

        protected override string ConvertValue(int value) => value.ToString();
        protected override int ConvertText(string text)
        {
            int value;

            if (string.IsNullOrWhiteSpace(text))
                value = 0;

            else if (!int.TryParse(text, out value))
            {
                Debug.LogError($"Cannot convert '{text}' to <int>");
                value = CurrentValue;
            }

            if (value <= Min) return Min;
            if (value >= Max) return Max;
            return value;
        }

        protected override bool AcceptCharacter(char c) => base.AcceptCharacter(c) && c != 0 && "1234567890-".IndexOf(c) != -1;
    }
    #endregion Int Input

    #region Long Input
    public class LongInput : Input<long>
    {
        protected override string Type => "number";

        public long Min { get; set; }
        public long Max { get; set; }


        public LongInput() => Modify();
        public LongInput Modify(long min = long.MinValue, long max = long.MaxValue, string placeholder = null, string prefix = null, string suffix = null, bool readOnly = false, bool delayed = false, bool autoSelectAll = true)
        {
            Min = min;
            Max = max;
            Placeholder = placeholder;
            Prefix = prefix;
            Suffix = suffix;
            MaxLength = -1;
            Multiline = false;
            ReadOnly = readOnly;
            Delayed = delayed;
            AutoSelectAll = autoSelectAll;

            return this;
        }

        protected override string ConvertValue(long value) => value.ToString();
        protected override long ConvertText(string text)
        {
            long value;

            if (string.IsNullOrWhiteSpace(text))
                value = 0;

            else if (!long.TryParse(text, out value))
            {
                Debug.LogError($"Cannot convert '{text}' to <long>");
                value = CurrentValue;
            }

            if (value <= Min) return Min;
            if (value >= Max) return Max;
            return value;
        }

        protected override bool AcceptCharacter(char c) => base.AcceptCharacter(c) && c != 0 && "1234567890-".IndexOf(c) != -1;
    }
    #endregion Long Input

    #region Float Input
    public class FloatInput : Input<float>
    {
        protected override string Type => "number";

        public float Min { get; set; }
        public float Max { get; set; }


        public FloatInput() => Modify();
        public FloatInput Modify(float min = float.MinValue, float max = float.MaxValue, string placeholder = null, string prefix = null, string suffix = null, bool readOnly = false, bool delayed = false, bool autoSelectAll = true)
        {
            Min = min;
            Max = max;
            Placeholder = placeholder;
            Prefix = prefix;
            Suffix = suffix;
            MaxLength = -1;
            Multiline = false;
            ReadOnly = readOnly;
            Delayed = delayed;
            AutoSelectAll = autoSelectAll;

            return this;
        }

        protected override string ConvertValue(float value) => value.ToString();
        protected override float ConvertText(string text)
        {
            float value;

            if (string.IsNullOrWhiteSpace(text))
                value = 0;

            else if (!float.TryParse(text, out value))
            {
                Debug.LogError($"Cannot convert '{text}' to <float>");
                value = CurrentValue;
            }

            if (value <= Min) return Min;
            if (value >= Max) return Max;
            return value;
        }

        protected override bool AcceptCharacter(char c) => base.AcceptCharacter(c) && c != 0 && "1234567890-.,".IndexOf(c) != -1;
    }
    #endregion Float Input

    #region Double Input
    public class DoubleInput : Input<double>
    {
        protected override string Type => "number";

        public double Min { get; set; }
        public double Max { get; set; }


        public DoubleInput() => Modify();
        public DoubleInput Modify(double min = double.MinValue, double max = double.MaxValue, string placeholder = null, string prefix = null, string suffix = null, bool readOnly = false, bool delayed = false, bool autoSelectAll = true)
        {
            Min = min;
            Max = max;
            Placeholder = placeholder;
            Prefix = prefix;
            Suffix = suffix;
            MaxLength = -1;
            Multiline = false;
            ReadOnly = readOnly;
            Delayed = delayed;
            AutoSelectAll = autoSelectAll;

            return this;
        }

        protected override string ConvertValue(double value) => value.ToString();
        protected override double ConvertText(string text)
        {
            double value;

            if (string.IsNullOrWhiteSpace(text))
                value = 0;

            else if (!double.TryParse(text, out value))
            {
                Debug.LogError($"Cannot convert '{text}' to <double>");
                value = CurrentValue;
            }

            if (value <= Min) return Min;
            if (value >= Max) return Max;
            return value;
        }

        protected override bool AcceptCharacter(char c) => base.AcceptCharacter(c) && c != 0 && "1234567890-.,".IndexOf(c) != -1;
    }
    #endregion Double Input

    #region Decimal Input
    public class DecimalInput : Input<decimal>
    {
        protected override string Type => "number";

        public decimal Min { get; set; }
        public decimal Max { get; set; }


        public DecimalInput() => Modify();
        public DecimalInput Modify(decimal min = decimal.MinValue, decimal max = decimal.MaxValue, string placeholder = null, string prefix = null, string suffix = null, bool readOnly = false, bool delayed = false, bool autoSelectAll = true)
        {
            Min = min;
            Max = max;
            Placeholder = placeholder;
            Prefix = prefix;
            Suffix = suffix;
            MaxLength = -1;
            Multiline = false;
            ReadOnly = readOnly;
            Delayed = delayed;
            AutoSelectAll = autoSelectAll;

            return this;
        }

        protected override string ConvertValue(decimal value) => value.ToString();
        protected override decimal ConvertText(string text)
        {
            decimal value;

            if (string.IsNullOrWhiteSpace(text))
                value = 0;

            else if (!decimal.TryParse(text, out value))
            {
                Debug.LogError($"Cannot convert '{text}' to <decimal>");
                value = CurrentValue;
            }

            if (value <= Min) return Min;
            if (value >= Max) return Max;
            return value;
        }

        protected override bool AcceptCharacter(char c) => base.AcceptCharacter(c) && c != 0 && "1234567890-.,".IndexOf(c) != -1;
    }
    #endregion Decimal Input

    #region String Input
    public class StringInput : Input<string>
    {
        protected override string Type => "string";

        public StringInput() => Modify();
        public StringInput Modify(string placeholder = null, string prefix = null, string suffix = null, int maxLength = -1, bool multiline = false, bool readOnly = false, bool delayed = false, bool autoSelectAll = false)
        {
            Placeholder = placeholder;
            Prefix = prefix;
            Suffix = suffix;
            MaxLength = maxLength;
            Multiline = multiline;
            ReadOnly = readOnly;
            Delayed = delayed;
            AutoSelectAll = autoSelectAll;

            return this;
        }

        protected override string ConvertValue(string text) => text;
        protected override string ConvertText(string text) => text;
    }
    #endregion String Input

    #region Label Input
    public class LabelInput : Input<string>
    {
        protected override string[] DefaultClasses => new string[] { "field", "input", Type, "text" };
        protected override string Type => "label";

        public LabelInput() => Modify();
        public LabelInput Modify(string placeholder = null, string prefix = null, string suffix = null, int maxLength = -1, bool multiline = false, bool readOnly = true, bool delayed = false, bool autoSelectAll = false)
        {
            Placeholder = placeholder;
            Prefix = prefix;
            Suffix = suffix;
            MaxLength = maxLength;
            Multiline = multiline;
            ReadOnly = readOnly;
            Delayed = delayed;
            AutoSelectAll = autoSelectAll;

            return this;
        }

        protected override string ConvertValue(string text) => text;
        protected override string ConvertText(string text) => text;
    }
    #endregion Label Input
}
using System;

using UnityEngine;


namespace Simplex
{
    [Serializable]
    public struct SEnum<T> where T : Enum
    {
#if UNITY_EDITOR
        [SerializeField] private string stringValue;
#endif

        [SerializeField] private T value;

        public T Value
        {
#if UNITY_EDITOR
            get
            {
                try { return (T)Enum.Parse<T>(stringValue); }
                catch (Exception exception) { exception.Error($"Failed parsing {stringValue:info} to Enum {typeof(T):type}"); return value; }
            }
            set
            {
                stringValue = value.ToString();
                this.value = value;
            }
#else
            get => value;
            set => this.value = value;
#endif
        }


        public override string ToString() => Value.ToString();
        public override bool Equals(object obj) => obj is SEnum<T> other && other.Value.Equals(Value);
        public override int GetHashCode() => HashCode.Combine(Value);

        public static implicit operator T(SEnum<T> enumeration) => enumeration.Value;
        public static implicit operator SEnum<T>(T enumeration) => new SEnum<T>() { Value = enumeration };
        public static explicit operator Enum(SEnum<T> enumeration) => enumeration.Value;
    }
}
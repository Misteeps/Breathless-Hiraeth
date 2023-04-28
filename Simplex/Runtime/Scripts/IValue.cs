using System;
using System.Reflection;

using UnityEngine;


namespace Simplex
{
    #region IValue
    public interface IValue
    {
        public string Name { get; }
        public object Value { get; set; }
    }
    #endregion IValue
    #region IValue <T>
    public interface IValue<T> : IValue
    {
        public new T Value { get; set; }
    }
    #endregion IValue <T>

    #region Delegate Value
    public class DelegateValue : IValue
    {
        public Func<object> getValue;
        public Action<object> setValue;
        public string Name { get; private set; }
        public object Value
        {
            get => getValue?.Invoke() ?? default;
            set => setValue?.Invoke(value);
        }


        public DelegateValue(Func<object> getValue) : this(getValue, null, null) { }
        public DelegateValue(Func<object> getValue, Action<object> setValue) : this(getValue, setValue, null) { }
        public DelegateValue(Func<object> getValue, Action<object> setValue, string name)
        {
            this.getValue = getValue;
            this.setValue = setValue;
            Name = name;
        }
    }
    #endregion Delegate Value
    #region Delegate Value <T>
    public class DelegateValue<T> : IValue<T>
    {
        public Func<T> getValue;
        public Action<T> setValue;
        public string Name { get; private set; }
        object IValue.Value { get => Value; set => Value = (T)value; }
        public T Value
        {
            get => (getValue == null) ? default : getValue.Invoke();
            set => setValue?.Invoke(value);
        }


        public DelegateValue(Func<T> getValue) : this(getValue, null, null) { }
        public DelegateValue(Func<T> getValue, Action<T> setValue) : this(getValue, setValue, null) { }
        public DelegateValue(Func<T> getValue, Action<T> setValue, string name)
        {
            this.getValue = getValue;
            this.setValue = setValue;
            Name = name;
        }
    }
    #endregion Delegate Value <T>

    #region Field Value
    public class FieldValue : IValue
    {
        public object container;
        public FieldInfo field;
        public string Name => field.Name;
        public object Value
        {
            get => field?.GetValue(container) ?? default;
            set => field?.SetValue(container, value);
        }


        public FieldValue(FieldInfo field) : this(null, field) { }
        public FieldValue(object container, string field, BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) : this(container, container.GetType().GetField(field, flags)) { }
        public FieldValue(object container, FieldInfo field)
        {
            this.container = container;
            this.field = field;
        }
    }
    #endregion Field Value
    #region Field Value <T>
    public class FieldValue<T> : FieldValue, IValue<T>
    {
        public new T Value { get => (T)base.Value; set => base.Value = value; }


        public FieldValue(FieldInfo field) : base(field) { }
        public FieldValue(object container, string field, BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) : base(container, field, flags) { }
        public FieldValue(object container, FieldInfo field) : base(container, field) { }
    }
    #endregion Field Value

    #region Property Value
    public class PropertyValue : IValue
    {
        public object container;
        public PropertyInfo property;
        public string Name => property.Name;
        public object Value
        {
            get => property?.GetValue(container) ?? default;
            set => property?.SetValue(container, value);
        }


        public PropertyValue(PropertyInfo property) : this(null, property) { }
        public PropertyValue(object container, string property, BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) : this(container, container.GetType().GetProperty(property, flags)) { }
        public PropertyValue(object container, PropertyInfo property)
        {
            this.container = container;
            this.property = property;
        }
    }
    #endregion Property Value
    #region Property Value <T>
    public class PropertyValue<T> : PropertyValue, IValue<T>
    {
        public new T Value { get => (T)base.Value; set => base.Value = value; }


        public PropertyValue(PropertyInfo property) : base(property) { }
        public PropertyValue(object container, string property, BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) : base(container, property, flags) { }
        public PropertyValue(object container, PropertyInfo property) : base(container, property) { }
    }
    #endregion Property Value
}
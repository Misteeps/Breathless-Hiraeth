using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex
{
    public abstract class Field : Label
    {
        protected override string[] DefaultClasses => new string[] { "field" };
        protected override Size DefaultSize => Size.Medium;
        protected override bool DefaultFocusable => true;
        protected override PickingMode DefaultPickingMode => PickingMode.Position;


        public Field()
        {
            RegisterCallback<RefreshEvent>(OnRefresh);
            RegisterCallback<FocusInEvent>(OnFocusIn);
            RegisterCallback<FocusOutEvent>(OnFocusOut);
        }

        protected abstract void OnRefresh(RefreshEvent refreshEvent);
        protected virtual void OnFocusIn(FocusInEvent focusInEvent) => this.Refresh();
        protected virtual void OnFocusOut(FocusOutEvent focusOutEvent) => this.Refresh();
    }


    public abstract class Field<T> : Field, IBindable
    {
        private IValue iValue;
        public T BindedValue
        {
            get => (iValue == null) ? CurrentValue : (T)iValue.Value;
            set
            {
                if (iValue == null) CurrentValue = value;
                else if (!EqualityComparer<T>.Default.Equals(value, BindedValue))
                    iValue.Value = value;

                this.Refresh();
            }
        }

        private T currentValue;
        public virtual T CurrentValue
        {
            get => currentValue;
            set
            {
                if (EqualityComparer<T>.Default.Equals(value, currentValue)) return;

                using ChangeEvent<T> changeEvent = ChangeEvent<T>.GetPooled(currentValue, value);
                changeEvent.target = this;
                SendEvent(changeEvent);

                currentValue = value;
            }
        }

        IValue IBindable.IValue { get => iValue; set => iValue = value; }


        protected override void OnRefresh(RefreshEvent refreshEvent) => CurrentValue = BindedValue;
        protected override void OnFocusOut(FocusOutEvent focusOutEvent) => BindedValue = CurrentValue;
    }
}
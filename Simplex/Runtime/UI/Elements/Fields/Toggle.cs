using System;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex
{
    public abstract class Toggle : Field<bool>
    {
        public override bool CurrentValue
        {
            set
            {
                base.CurrentValue = value;
                this.ClassToggle("on", "off", CurrentValue);
            }
        }


        public Toggle()
        {
            RegisterCallback<ClickEvent>(OnClick);
        }

        protected virtual void OnClick(ClickEvent clickEvent)
        {
            if (clickEvent.button == 0)
                BindedValue = !BindedValue;

            clickEvent.StopPropagation();
        }
    }


    #region Toggle Slide
    public class ToggleSlide : Toggle
    {
        protected override string[] DefaultClasses => new string[] { "field", "toggle", "slide", "inset" };

        public readonly Div fill;
        public readonly Div knob;


        public ToggleSlide()
        {
            fill = this.Create<Div>("fill");
            knob = fill.Create<Div>("knob");
        }
    }
    #endregion Toggle Slide

    #region Toggle Check
    public class ToggleCheck : Toggle
    {
        protected override string[] DefaultClasses => new string[] { "field", "toggle", "check", "inset", "icon" };
    }
    #endregion Toggle Check

    #region Toggle Button
    public class ToggleButton : Toggle
    {
        protected override string[] DefaultClasses => new string[] { "field", "toggle", "button", "outset", "text" };

        public string EnabledText { get; set; }
        public string DisabledText { get; set; }

        public override bool CurrentValue
        {
            set
            {
                base.CurrentValue = value;
                text = (CurrentValue) ? EnabledText : DisabledText;
            }
        }


        public ToggleButton() => Modify();
        public ToggleButton Modify(string enabledText = "Enabled", string disabledText = "Disabled")
        {
            EnabledText = enabledText;
            DisabledText = disabledText;

            return this;
        }
    }
    #endregion Toggle Button
}
using System;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex
{
    public class Div : VisualElement
    {
        protected virtual string[] DefaultClasses => null;
        protected virtual Size DefaultSize => (Size)(-1);
        protected virtual bool DefaultFocusable => false;
        protected virtual PickingMode DefaultPickingMode => PickingMode.Ignore;
        protected virtual UsageHints DefaultUsageHints => UsageHints.None;


        public Div()
        {
            ClearClassList();

            if (!DefaultClasses.IsEmpty())
                for (int i = 0; i < DefaultClasses.Length; i++)
                    AddToClassList(DefaultClasses[i]);

            if (DefaultSize != (Size)(-1))
                this.Size(DefaultSize);

            focusable = DefaultFocusable;
            pickingMode = DefaultPickingMode;
            usageHints = DefaultUsageHints;
        }
    }
}
using System;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex
{
    public class Label : TextElement
    {
        protected virtual string[] DefaultClasses => new string[] { "label", "text" };
        protected virtual Size DefaultSize => Size.Medium;
        protected virtual bool DefaultFocusable => false;
        protected virtual PickingMode DefaultPickingMode => PickingMode.Ignore;
        protected virtual UsageHints DefaultUsageHints => UsageHints.None;

        public virtual bool RichText { get => enableRichText; set => enableRichText = value; }


        public Label()
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

            RichText = true;
        }
    }


    #region Labeled
    public class Labeled : Div
    {
        protected override string[] DefaultClasses => new string[] { "labeled" };
        protected override Size DefaultSize => Size.Medium;

        public readonly Label label;

        public string Text { get => label.text; set => label.text = value; }
        public bool RichText { get => label.enableRichText; set => label.enableRichText = value; }
        public bool Highlight
        {
            get => ClassListContains("highlight");
            set
            {
                EnableInClassList("highlight", value);
                pickingMode = (value) ? PickingMode.Position : PickingMode.Ignore;
            }
        }


        public Labeled()
        {
            label = this.Create<Label>();

            Modify();
        }
        public Labeled Modify(string text = null, Size size = Size.Medium, bool richText = true, bool highlight = true)
        {
            Text = text;
            RichText = richText;
            Highlight = highlight;

            this.Size(size);
            if (string.IsNullOrEmpty(name))
                this.Name(text);

            return this;
        }

        public virtual Labeled Bind(IValue iValue)
        {
            foreach (VisualElement child in Children())
                if (child is IBindable bindable) bindable.IValue = iValue;

            if (string.IsNullOrEmpty(Text))
                Text = iValue.Name.TitleCase();

            return this.Refresh();
        }
    }
    #endregion Labeled

    #region Labeled <T1>
    public class Labeled<T1> : Labeled where T1 : VisualElement, new()
    {
        public readonly T1 element1;


        public Labeled()
        {
            element1 = this.Create<T1>("flexible");
        }
        public new Labeled<T1> Modify(string text = null, Size size = Size.Medium, bool richText = true, bool highlight = true) => base.Modify(text, size, richText, highlight) as Labeled<T1>;

        public new Labeled<T1> Bind(IValue iValue) => base.Bind(iValue) as Labeled<T1>;

        public Labeled<T1> Elements(out T1 element1)
        {
            element1 = this.element1;

            return this;
        }
        public Labeled<T1> Elements(Action<T1> element1)
        {
            element1.Invoke(this.element1);

            return this;
        }
    }
    #endregion Labeled <T1>

    #region Labeled <T1, T2>
    public class Labeled<T1, T2> : Labeled where T1 : VisualElement, new() where T2 : VisualElement, new()
    {
        public readonly T1 element1;
        public readonly T2 element2;


        public Labeled()
        {
            element1 = this.Create<T1>("flexible");
            element2 = this.Create<T2>("flexible");
        }
        public new Labeled<T1, T2> Modify(string text = null, Size size = Size.Medium, bool richText = true, bool highlight = true) => base.Modify(text, size, richText, highlight) as Labeled<T1, T2>;

        public new Labeled<T1, T2> Bind(IValue iValue) => base.Bind(iValue) as Labeled<T1, T2>;

        public Labeled<T1, T2> Elements(out T1 element1, out T2 element2)
        {
            element1 = this.element1;
            element2 = this.element2;

            return this;
        }
        public Labeled<T1, T2> Elements(Action<T1> element1, Action<T2> element2)
        {
            element1.Invoke(this.element1);
            element2.Invoke(this.element2);

            return this;
        }
    }
    #endregion Labeled <T1, T2>

    #region Labeled <T1, T2, T3>
    public class Labeled<T1, T2, T3> : Labeled where T1 : VisualElement, new() where T2 : VisualElement, new() where T3 : VisualElement, new()
    {
        public readonly T1 element1;
        public readonly T2 element2;
        public readonly T3 element3;


        public Labeled()
        {
            element1 = this.Create<T1>("flexible");
            element2 = this.Create<T2>("flexible");
            element3 = this.Create<T3>("flexible");
        }
        public new Labeled<T1, T2, T3> Modify(string text = null, Size size = Size.Medium, bool richText = true, bool highlight = true) => base.Modify(text, size, richText, highlight) as Labeled<T1, T2, T3>;

        public new Labeled<T1, T2, T3> Bind(IValue iValue) => base.Bind(iValue) as Labeled<T1, T2, T3>;

        public Labeled<T1, T2, T3> Elements(out T1 element1, out T2 element2, out T3 element3)
        {
            element1 = this.element1;
            element2 = this.element2;
            element3 = this.element3;

            return this;
        }
        public Labeled<T1, T2, T3> Elements(Action<T1> element1, Action<T2> element2, Action<T3> element3)
        {
            element1.Invoke(this.element1);
            element2.Invoke(this.element2);
            element3.Invoke(this.element3);

            return this;
        }
    }
    #endregion Labeled <T1, T2, T3>
}
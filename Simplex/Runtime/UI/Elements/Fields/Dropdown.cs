using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex
{
    public class Dropdown : Field
    {
        protected override string[] DefaultClasses => new string[] { "field", "dropdown", "outset", "text", "icon" };

        public string Text
        {
            get => text;
            set
            {
                text = value;
                EnableInClassList("text", !string.IsNullOrEmpty(value));
            }
        }
        public int MaxHeight { get; set; }
        public bool InheritStyleSheets { get; set; }

        public Func<VisualElement> contentsFactory;


        public Dropdown()
        {
            RegisterCallback<ClickEvent>(OnClick);

            Modify();
        }
        public Dropdown Modify(string text = null, int maxHeight = 400, bool inheritStyleSheets = true)
        {
            Text = text;
            MaxHeight = maxHeight;
            InheritStyleSheets = inheritStyleSheets;

            if (string.IsNullOrEmpty(name))
                this.Name(text);

            return this;
        }

        public Dropdown Bind(VisualElement contents) => Bind(() => contents);
        public Dropdown Bind(Func<VisualElement> contentsFactory)
        {
            this.contentsFactory = contentsFactory;

            return this.Refresh();
        }

        protected virtual void OnClick(ClickEvent clickEvent)
        {
            if (contentsFactory == null) { ConsoleUtilities.Warn(ConsoleUtilities.uiTag, $"Empty contents in {this:ref}. Dropdown popup will not be shown"); return; }

            Popup popup = new Popup();
            popup.MaxHeight = MaxHeight;
            popup.Add(contentsFactory.Invoke());
            popup.Open(this, InheritStyleSheets);

            clickEvent.StopPropagation();
        }
        protected override void OnRefresh(RefreshEvent refreshEvent) { }
    }


    public class Dropdown<T> : Field<T>
    {
        protected override string[] DefaultClasses => new string[] { "field", "dropdown", "outset", "text", "icon" };

        public string Title { get; set; }
        public int MaxHeight { get; set; }
        public bool InheritStyleSheets { get; set; }
        public bool Searchable { get; set; }

        public Func<T[]> getValues;
        public Func<T, string> stringify;

        public override T CurrentValue
        {
            set
            {
                base.CurrentValue = value;
                text = stringify.Invoke(value);
            }
        }


        public Dropdown()
        {
            RegisterCallback<ClickEvent>(OnClick);

            Modify();
        }
        public Dropdown<T> Modify(string title = null, int maxHeight = 400, bool inheritStyleSheets = true, bool searchable = false)
        {
            Title = title;
            MaxHeight = maxHeight;
            InheritStyleSheets = inheritStyleSheets;
            Searchable = searchable;

            return this;
        }

        public Dropdown<T> Bind<TEnum>(IValue iValue) where TEnum : Enum, T => Bind(iValue, (T[])Enum.GetValues(typeof(TEnum)));
        public Dropdown<T> Bind(IValue iValue, IEnumerable<(string name, T value)> choices)
        {
            if (choices == null || choices.Count() == 0) return Bind(iValue);

            string[] names = choices.Select(choice => choice.name).ToArray();
            T[] values = choices.Select(choice => choice.value).ToArray();

            string Stringify(T value)
            {
                int index = Array.IndexOf(values, value);
                return (index == -1) ? null : names[index];
            }

            return Bind(iValue, values, Stringify);
        }
        public Dropdown<T> Bind(IValue iValue, T[] values = null, Func<T, string> stringify = null) => Bind(iValue, () => values, stringify);
        public Dropdown<T> Bind(IValue iValue, Func<T[]> getValues, Func<T, string> stringify = null)
        {
            this.getValues = getValues;
            this.stringify = stringify ?? (value => value?.ToString());

            return UIUtilities.Bind(this, iValue);
        }

        protected virtual void OnClick(ClickEvent clickEvent)
        {
            T[] values = getValues?.Invoke();
            if (values.IsEmpty()) { ConsoleUtilities.Warn(ConsoleUtilities.uiTag, $"Empty values in {this:ref}. Dropdown popup will not be shown"); return; }

            Popup popup = new Popup();
            popup.MaxHeight = MaxHeight;

            Action<T, bool> onSelect = null;
            Action<T, DirectoryElement> onBind = null;
            if (typeof(T).IsEnum && Attribute.IsDefined(typeof(T), typeof(FlagsAttribute)))
            {
                onSelect = (value, selected) => BindedValue = (T)Enum.ToObject(typeof(T), (selected) ? Convert.ToInt32(CurrentValue) + Convert.ToInt32(value) : Convert.ToInt32(CurrentValue) - Convert.ToInt32(value));
                onBind = (value, element) => element.Modify(stringify.Invoke(value), null, (Convert.ToInt32(CurrentValue) & Convert.ToInt32(value)) == Convert.ToInt32(value)).Bind(value);
            }
            else
            {
                onSelect = (value, selected) => { BindedValue = value; popup.Close(); };
                onBind = (value, element) => element.Modify(stringify.Invoke(value), null, value.Equals(CurrentValue)).Bind(value);
            }

            popup.Create<DirectoryView<T>>("flexible").Modify(Title, Searchable).Bind(values, onSelect, onBind);
            popup.Open(this, InheritStyleSheets);

            clickEvent.StopPropagation();
        }
    }
}
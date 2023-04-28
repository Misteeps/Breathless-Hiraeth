using System;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex
{
    public class Button : Field
    {
        protected override string[] DefaultClasses => new string[] { "field", "button", "outset", "text", "icon" };

        public string Text
        {
            get => text;
            set
            {
                text = value;
                EnableInClassList("text", !string.IsNullOrEmpty(value));
            }
        }
        public Texture2D Icon
        {
            get => resolvedStyle.backgroundImage.texture;
            set
            {
                if (value == null)
                {
                    style.backgroundImage = StyleKeyword.Null;
                    RemoveFromClassList("icon");
                }
                else
                {
                    style.backgroundImage = value;
                    AddToClassList("icon");
                }
            }
        }

        public EventCallback<ClickEvent> onClick;


        public Button()
        {
            RegisterCallback<ClickEvent>(OnClick);

            Modify();
        }
        public Button Modify(string text = null, Texture2D icon = null)
        {
            Text = text;
            Icon = icon;

            if (string.IsNullOrEmpty(name))
                this.Name(text ?? icon?.name);

            return this;
        }

        public Button Bind(EventCallback<ClickEvent> onClick)
        {
            this.onClick = onClick;

            return this.Refresh();
        }

        protected virtual void OnClick(ClickEvent clickEvent)
        {
            onClick.Invoke(clickEvent);

            clickEvent.StopPropagation();
        }
        protected override void OnRefresh(RefreshEvent refreshEvent) { }
    }
}
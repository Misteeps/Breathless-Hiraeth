using System;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex
{
    public class DirectoryElement : Div
    {
        protected override string[] DefaultClasses => new string[] { "directory-element" };
        protected override PickingMode DefaultPickingMode => PickingMode.Position;
        public override VisualElement contentContainer => body;

        public readonly Div header;
        public readonly Div body;

        public readonly Div arrow;
        public readonly Div check;
        public readonly Div icon;
        public readonly Label title;

        public virtual string Title
        {
            get => title.text;
            set => title.text = value;
        }
        public virtual Texture2D Icon
        {
            get => icon.resolvedStyle.backgroundImage.texture;
            set
            {
                if (value == null)
                {
                    icon.style.backgroundImage = StyleKeyword.Null;
                    icon.Enable(false);
                }
                else
                {
                    icon.style.backgroundImage = value;
                    icon.Enable(true);
                }
            }
        }
        public virtual bool Selected
        {
            get => ClassListContains("selected");
            set
            {
                EnableInClassList("selected", value);
                check.Enable(value);
            }
        }
        public virtual bool Collapsed
        {
            get => ClassListContains("collapsed");
            set => EnableInClassList("collapsed", value);
        }

        public Action onClick;


        public DirectoryElement()
        {
            header = hierarchy.Create<Div>("header").PickingMode(PickingMode.Position);
            body = hierarchy.Create<Div>("body");

            arrow = header.Create<Div>("icon").Name("arrow").Enable(false).PickingMode(PickingMode.Position);
            check = header.Create<Div>("icon").Name("check").Enable(false);
            icon = header.Create<Div>("icon").Name("icon").Enable(false);
            title = header.Create<Label>("flexible").Size(Size.Medium).Name("title").PickingMode(PickingMode.Position);

            header.RegisterCallback<ClickEvent>(OnClick);
            arrow.RegisterCallback<ClickEvent>(OnCollapse);
            RegisterCallback<RefreshEvent>(OnRefresh);

            Modify();
        }
        public DirectoryElement Modify(string title = null, Texture2D icon = null, bool selected = false, bool collapsed = false)
        {
            Title = title;
            Icon = icon;
            Selected = selected;
            Collapsed = collapsed;

            return this;
        }

        public virtual DirectoryElement Bind(object item)
        {
            onClick = null;

            body.Clear();

            return this.Refresh();
        }

        protected virtual void OnClick(ClickEvent clickEvent)
        {
            if (onClick == null) OnCollapse(clickEvent);
            else
            {
                Selected = !Selected;
                onClick.Invoke();
            }

            clickEvent.StopPropagation();
        }
        protected virtual void OnCollapse(ClickEvent clickEvent)
        {
            if (!arrow.enabledSelf) return;

            bool collapsed = !Collapsed;
            Collapsed = collapsed;

            if (clickEvent.altKey)
                body.Query<DirectoryElement>().ForEach(element => element.Collapsed = collapsed);

            clickEvent.StopPropagation();
        }
        protected virtual void OnRefresh(RefreshEvent refreshEvent) => arrow.Enable(childCount != 0);
    }
}
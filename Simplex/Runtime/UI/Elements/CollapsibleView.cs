using System;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex
{
    public class CollapsibleView : Div
    {
        protected override string[] DefaultClasses => new string[] { "collapsible-view" };
        public override VisualElement contentContainer => body;

        public readonly Div header;
        public readonly VerticalScrollView body;

        public readonly Button arrow;
        public readonly Label title;

        public virtual string Title
        {
            get => title.text;
            set => title.text = value;
        }
        public virtual bool Collapsed
        {
            get => ClassListContains("collapsed");
            set => EnableInClassList("collapsed", value);
        }


        public CollapsibleView()
        {
            header = hierarchy.Create<Div>("header", "toolbar");
            body = hierarchy.Create<VerticalScrollView>("body");

            arrow = header.Create<Button>("icon").Name("arrow").Bind(_ => Collapsed = !Collapsed);
            header.Create<HorizontalSpace>().Size(Size.Mini);
            title = header.Create<Label>("flexible").Name("title");

            Modify();
        }
        public virtual CollapsibleView Modify(string title = null, bool collapsed = false)
        {
            Title = title;
            Collapsed = collapsed;

            return this;
        }
    }
}
using System;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex
{
    public abstract class ScrollView : Div
    {
        protected override PickingMode DefaultPickingMode => PickingMode.Position;
        public override VisualElement contentContainer => contents;

        public readonly Div container;
        public readonly Div contents;


        public ScrollView()
        {
            container = hierarchy.Create<Div>("container", "flexible");
            contents = container.Create<Div>("contents", "static");

            RegisterCallback<WheelEvent>(OnScroll);
        }

        protected abstract void OnScroll(WheelEvent wheelEvent);
    }


    #region Horizontal
    public class HorizontalScrollView : ScrollView
    {
        protected override string[] DefaultClasses => new string[] { "scroll-view", "horizontal" };

        public readonly ScrollBar scrollbar;


        public HorizontalScrollView()
        {
            scrollbar = hierarchy.Create<ScrollBar>().Bind(contents, false);
        }

        protected override void OnScroll(WheelEvent wheelEvent)
        {
            if (!scrollbar.Active) return;

            if ((wheelEvent.delta.y < 0 && scrollbar.Factor != 0) || (wheelEvent.delta.y > 0 && scrollbar.Factor != 1))
                wheelEvent.StopPropagation();

            scrollbar.Position += (int)(40 * wheelEvent.delta.y);
        }
    }
    #endregion Horizontal

    #region Vertical
    public class VerticalScrollView : ScrollView
    {
        protected override string[] DefaultClasses => new string[] { "scroll-view", "vertical" };

        public readonly ScrollBar scrollbar;


        public VerticalScrollView()
        {
            scrollbar = hierarchy.Create<ScrollBar>().Bind(contents, true);
        }

        protected override void OnScroll(WheelEvent wheelEvent)
        {
            if (!scrollbar.Active) return;

            if ((wheelEvent.delta.y < 0 && scrollbar.Factor != 0) || (wheelEvent.delta.y > 0 && scrollbar.Factor != 1))
                wheelEvent.StopPropagation();

            scrollbar.Position += (int)(40 * wheelEvent.delta.y);
        }
    }
    #endregion Vertical

    #region Dynamic
    public class DynamicScrollView : ScrollView
    {
        protected override string[] DefaultClasses => new string[] { "scroll-view", "dynamic" };

        public readonly ScrollBar vertical;
        public readonly ScrollBar horizontal;


        public DynamicScrollView()
        {
            vertical = hierarchy.Create<ScrollBar>().Bind(contents, true);
            horizontal = hierarchy.Create<ScrollBar>().Bind(contents, false);

            throw new NotImplementedException();
        }

        protected override void OnScroll(WheelEvent wheelEvent) { }
    }
    #endregion Dynamic
}
using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex
{
    public class Popup : Div
    {
        protected override string[] DefaultClasses => new string[] { "popup-background" };
        protected override PickingMode DefaultPickingMode => PickingMode.Position;
        public override VisualElement contentContainer => window;

        public readonly Div window;

        public StyleLength Width
        {
            get => window.style.width;
            set => window.style.width = value;
        }
        public StyleLength MinWidth
        {
            get => window.style.minWidth;
            set => window.style.minWidth = value;
        }
        public StyleLength MaxWidth
        {
            get => window.style.maxWidth;
            set => window.style.maxWidth = value;
        }
        public StyleLength Height
        {
            get => window.style.height;
            set => window.style.height = value;
        }
        public StyleLength MinHeight
        {
            get => window.style.minHeight;
            set => window.style.minHeight = value;
        }
        public StyleLength MaxHeight
        {
            get => window.style.maxHeight;
            set => window.style.maxHeight = value;
        }

        private VisualElement sourceElement;
        public virtual VisualElement SourceElement
        {
            get => sourceElement;
            set
            {
                sourceElement?.UnregisterCallback<DetachFromPanelEvent>(Close);
                sourceElement = value;
                sourceElement?.RegisterCallback<DetachFromPanelEvent>(Close);
            }
        }


        public Popup()
        {
            window = hierarchy.Create<Div>("popup-window").PickingMode(PickingMode.Position);
            window.RegisterCallback<ClickEvent>(e => e.StopPropagation());
            window.RegisterCallback<KeyDownEvent>(e => { if (e.keyCode is KeyCode.Escape) Close(); });

            RegisterCallback<ClickEvent>(e => Close());
        }
        public virtual Popup Modify(int top = -1, int bottom = -1, int left = -1, int right = -1, int width = -1, int height = -1, bool fit = true)
        {
            window.style.top = (top == -1) ? StyleKeyword.Auto : top;
            window.style.bottom = (bottom == -1) ? StyleKeyword.Auto : bottom;
            window.style.left = (left == -1) ? StyleKeyword.Auto : left;
            window.style.right = (right == -1) ? StyleKeyword.Auto : right;
            window.style.width = (width == -1) ? StyleKeyword.Null : width;
            window.style.height = (height == -1) ? StyleKeyword.Null : height;

            if (fit)
                Fit();

            return this;
        }

        public virtual Popup Open(IPanel panel, int top = -1, int bottom = -1, int left = -1, int right = -1, int width = -1, int height = -1, bool fit = true)
        {
            if (SourceElement != null) Close();

            panel.visualTree.Add(this);
            Modify(top, bottom, left, right, width, height, fit);

            return this.Refresh();
        }
        public virtual Popup Open(VisualElement source, bool inheritStyleSheets = true)
        {
            if (SourceElement != null) Close();
            SourceElement = source ?? throw new NullReferenceException($"Null element").Overwrite(ConsoleUtilities.uiTag, $"Failed displaying popup under {source:ref}");

            if (SourceElement.panel == null) throw new Exception("Element not attached to panel").Overwrite(ConsoleUtilities.uiTag, $"Failed displaying popup under {source:ref}");
            SourceElement.panel.visualTree.Add(this);

            if (inheritStyleSheets)
            {
                VisualElement parent = SourceElement;
                List<StyleSheet> styleSheets = new List<StyleSheet>();
                for (int i = 0; i < 100; i++)
                {
                    parent = parent.parent;
                    if (parent == null) break;
                    for (int j = parent.styleSheets.count - 1; j >= 0; j--)
                        styleSheets.Insert(0, parent.styleSheets[j]);
                }

                foreach (StyleSheet styleSheet in styleSheets)
                    this.styleSheets.Add(styleSheet);
            }

            Rect bounds = SourceElement.worldBound;
            Modify(top: (int)bounds.yMax + 1, left: (int)bounds.xMin, width: (int)bounds.width);

            return this.Refresh();
        }

        protected virtual void Close(DetachFromPanelEvent panelEvent) => Close();
        public virtual void Close()
        {
            SourceElement = null;
            RemoveFromHierarchy();
        }

        public async void Fit()
        {
            for (int i = 0; i < 10; i++)
                if (!float.IsNaN(worldBound.width) && !float.IsNaN(worldBound.height)) break;
                else await GeneralUtilities.DelayFrame(1);

            Rect parentBounds = this.worldBound;
            Rect windowBounds = window.worldBound;

            if (parentBounds.Contains(windowBounds.min) && parentBounds.Contains(windowBounds.max))
                return;

            window.style.top = Mathf.Clamp(windowBounds.yMin, parentBounds.yMin, parentBounds.yMax - windowBounds.height);
            window.style.left = Mathf.Clamp(windowBounds.xMin, parentBounds.xMin, parentBounds.xMax - windowBounds.width);
        }
    }
}
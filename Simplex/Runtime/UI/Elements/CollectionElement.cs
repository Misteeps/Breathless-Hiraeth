using System;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex
{
    public class CollectionElement : Div
    {
        protected override string[] DefaultClasses => new string[] { "collection-element" };
        protected override PickingMode DefaultPickingMode => PickingMode.Position;
        public override VisualElement contentContainer => contents;

        public readonly Label index;
        public readonly Div contents;
        private Div[] rows;

        public EventCallback<PointerUpEvent> onPointerUp;
        public EventCallback<PointerDownEvent> onPointerDown;
        public EventCallback<PointerMoveEvent> onPointerMove;

        private ContextualMenuManipulator contextMenu;
        public ContextualMenuManipulator ContextMenu
        {
            get => contextMenu;
            set
            {
                index.RemoveManipulator(contextMenu);
                contextMenu = value;
                index.AddManipulator(contextMenu);
            }
        }


        public CollectionElement()
        {
            index = hierarchy.Create<Label>("index", "text", "icon").Text("0").PickingMode(PickingMode.Position);
            contents = hierarchy.Create<Div>("contents", "flexible");

            index.RegisterCallback<PointerUpEvent>(OnPointerUp);
            index.RegisterCallback<PointerDownEvent>(OnPointerDown);
            index.RegisterCallback<PointerMoveEvent>(OnPointerMove);

            Modify(0, false);

            focusable = false;
        }
        public CollectionElement Modify(int index, bool draggable, EventCallback<PointerUpEvent> onPointerUp = null, EventCallback<PointerDownEvent> onPointerDown = null, EventCallback<PointerMoveEvent> onPointerMove = null, ContextualMenuManipulator contextMenu = null)
        {
            this.index.text = index.ToString();
            this.index.EnableInClassList("draggable", draggable);
            this.ClassToggle("even", "odd", index % 2 == 0);

            this.onPointerUp = onPointerUp;
            this.onPointerDown = onPointerDown;
            this.onPointerMove = onPointerMove;
            ContextMenu = contextMenu;

            return this;
        }

        public virtual CollectionElement Bind(object item)
        {
            contents.Clear();

            contents.Create<Label>().Text(item?.ToString());

            return this.Refresh();
        }

        protected virtual void OnPointerUp(PointerUpEvent pointerEvent) => onPointerUp?.Invoke(pointerEvent);
        protected virtual void OnPointerDown(PointerDownEvent pointerEvent) => onPointerDown?.Invoke(pointerEvent);
        protected virtual void OnPointerMove(PointerMoveEvent pointerEvent) => onPointerMove?.Invoke(pointerEvent);

        public virtual Div Row(int index)
        {
            if (rows.IsEmpty())
            {
                rows = new Div[index + 1];
                for (int i = 0; i < rows.Length; i++)
                    rows[i] = this.Create<Div>("row").Name(i.ToString());
            }

            if (rows.OutOfRange(index))
            {
                Array.Resize(ref rows, index + 1);
                for (int i = 0; i < rows.Length; i++)
                    if (rows[i] == null)
                        rows[i] = this.Create<Div>("row").Name(i.ToString());
            }

            return rows[index];
        }
    }
}
using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;


namespace Simplex
{
    public class CollectionView<TItem> : CollectionView<TItem, CollectionElement> { }
    public class CollectionView<TItem, TElement> : CollapsibleView where TElement : CollectionElement, new()
    {
        #region Element Pool
        public static class ElementPool
        {
            private readonly static ObjectPool<TElement> pool = new ObjectPool<TElement>(() => new TElement(), defaultCapacity: 0, maxSize: 256);

            public static int CountAll => pool.CountAll;
            public static int CountActive => pool.CountActive;
            public static int CountInactive => pool.CountInactive;

            public static TElement Get() => pool.Get();
            public static PooledObject<TElement> Get(out TElement element) => pool.Get(out element);
            public static void Release(TElement element) => pool.Release(element);
        }
        #endregion Element Pool


        protected override string[] DefaultClasses => new string[] { "collapsible-view", "collection-view" };

        public readonly Button add;
        public readonly Button delete;
        public readonly IntInput size;

        private TElement[] dragTargets;
        private Div dragline;
        private int draglineIndex;
        private int dragStart;
        private bool dragging;

        private IValue iValue;
        public virtual IEnumerable<TItem> Collection { get; protected set; }
        public virtual int CollectionSize { get; protected set; }

        public virtual Func<TItem> ItemFactory { get; set; }
        public virtual Action<TItem, TElement, int> OnBindElement { get; set; }
        public virtual Action<TItem> OnAddItem { get; set; }
        public virtual Action<int> OnDeleteItem { get; set; }
        public virtual Action<int, int> OnMoveItem { get; set; }
        public virtual Action<int> OnResize { get; set; }

        public int MaxSize
        {
            get => size.Max;
            set => size.Max = Mathf.Clamp(value, 0, int.MaxValue);
        }
        public bool LockSize
        {
            get => size.ReadOnly;
            set
            {
                add.Enable(!value);
                delete.Enable(!value);
                size.Enable(!value);
            }
        }
        public bool LockSort
        {
            get;
            set;
        }


        public CollectionView()
        {
            arrow.Create<Div>("icon").Name("list").Size(Size.Medium);
            arrow.Create<Div>("icon").Name("arrow").Size(Size.Medium);

            header.Create<HorizontalSpace>().Size(Size.Mini);
            add = header.Create<Button>("icon").Name("add").Bind(_ => AddItem((ItemFactory == null) ? default : ItemFactory.Invoke()));
            header.Create<HorizontalSpace>().Size(Size.Mini);
            delete = header.Create<Button>("icon").Name("delete").Bind(_ => DeleteItem(CollectionSize - 1));
            header.Create<HorizontalSpace>().Size(Size.Mini);
            size = header.Create<IntInput>().Name("size").Size(Size.Small).Modify(delayed: true).Bind(() => CollectionSize, Resize);

            dragline = UIUtilities.Create<Div>("dragline");
            MoveDragLine(-1);

            RegisterCallback<DetachFromPanelEvent>(OnDetach);
            RegisterCallback<RefreshEvent>(OnRefresh);

            Modify();
        }
        public CollectionView<TItem, TElement> Modify(string title = null, int maxSize = 9999, bool lockSize = false, bool lockSort = false, bool collapsed = false)
        {
            Title = title;
            MaxSize = maxSize;
            LockSize = lockSize;
            LockSort = lockSort;
            Collapsed = collapsed;

            return this;
        }

        public CollectionView<TItem, TElement> Bind(Func<IEnumerable<TItem>> getCollection, Action<IEnumerable<TItem>> setCollection, Func<TItem> itemFactory = null, Action<TItem, TElement, int> onBindElement = null, Action<TItem> onAddItem = null, Action<int> onDeleteItem = null, Action<int, int> onMoveItem = null, Action<int> onResize = null) => Bind(new DelegateValue<IEnumerable<TItem>>(getCollection, setCollection, $"{typeof(TItem).Name}s"), itemFactory, onBindElement, onAddItem, onDeleteItem, onMoveItem, onResize);
        public CollectionView<TItem, TElement> Bind(IEnumerable<TItem> collection, Func<TItem> itemFactory = null, Action<TItem, TElement, int> onBindElement = null, Action<TItem> onAddItem = null, Action<int> onDeleteItem = null, Action<int, int> onMoveItem = null, Action<int> onResize = null) => Bind(new DelegateValue<IEnumerable<TItem>>(() => collection, null, $"{typeof(TItem).Name}s"), itemFactory, onBindElement, onAddItem, onDeleteItem, onMoveItem, onResize);
        public CollectionView<TItem, TElement> Bind(IValue iValue, Func<TItem> itemFactory = null, Action<TItem, TElement, int> onBindElement = null, Action<TItem> onAddItem = null, Action<int> onDeleteItem = null, Action<int, int> onMoveItem = null, Action<int> onResize = null)
        {
            this.iValue = iValue;
            if (string.IsNullOrEmpty(Title))
                Title = iValue.Name.TitleCase();

            ItemFactory = itemFactory ?? DefaultItemFactory;
            OnBindElement = onBindElement ?? DefaultOnBindElement;
            OnAddItem = onAddItem ?? DefaultOnAddItem;
            OnDeleteItem = onDeleteItem ?? DefaultOnDeleteItem;
            OnMoveItem = onMoveItem ?? DefaultOnMoveItem;
            OnResize = onResize ?? DefaultOnResize;

            return this.Refresh();
        }

        protected virtual void OnDetach(DetachFromPanelEvent panelEvent) => DetachElements();
        protected virtual void OnRefresh(RefreshEvent refreshEvent)
        {
            Collection = (IEnumerable<TItem>)iValue?.Value;
            CollectionSize = Collection?.Count() ?? 0;

            DetachElements();
            AttachElements();
        }

        protected virtual void AttachElements()
        {
            if (Collection == null) return;

            int iterator = 0;
            foreach (TItem item in Collection)
            {
                TElement element = ElementPool.Get();
                int index = iterator++;
                try
                {
                    element.Modify(index, !LockSort, e => OnElementPointerUp(element, index, e), e => OnElementPointerDown(element, index, e), e => OnElementPointerMove(element, e), ElementContextMenu(element, index));
                    OnBindElement.Invoke(item, element, index);

                    body.Add(element);
                }
                catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed binding and attaching {typeof(TElement):type} to {typeof(TItem):type} {Title:info} collection view"); }
            }
        }
        protected virtual void DetachElements()
        {
            body.Query<TElement>().ForEach(element =>
            {
                try
                {
                    ElementPool.Release(element);
                    element.Modify(-1, false);
                    element.RemoveFromHierarchy();
                }
                catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed detaching collection element {typeof(TElement):type}"); }
            });
        }

        protected virtual void OnElementPointerUp(TElement element, int index, PointerUpEvent pointerEvent)
        {
            if (LockSort || !dragging || pointerEvent.button != 0) return;

            int newIndex = CalculateDrag(element);
            if (newIndex < 0 || newIndex >= CollectionSize)
                newIndex = dragStart;

            element.RemoveFromClassList("drag");
            dragTargets = null;
            dragStart = -1;
            dragging = false;

            MoveDragLine(-1);
            element.style.top = StyleKeyword.Null;
            element.style.left = StyleKeyword.Null;

            element.index.ReleasePointer(pointerEvent.pointerId);
            pointerEvent.StopPropagation();

            element.RemoveFromHierarchy();
            ElementPool.Release(element);

            MoveItem(index, newIndex);
        }
        protected virtual void OnElementPointerDown(TElement element, int index, PointerDownEvent pointerEvent)
        {
            if (LockSort || dragging || pointerEvent.button != 0) return;

            element.AddToClassList("drag");
            dragTargets = body.Query<TElement>().Where(e => e != element).Build().ToArray();
            dragStart = index;
            dragging = true;

            for (int i = 0; i < dragTargets.Length; i++)
                dragTargets[i].Modify(i, false);

            MoveDragLine(body.IndexOf(element));
            MoveElement(element, pointerEvent.localPosition);
            body.Add(element);

            element.index.CapturePointer(pointerEvent.pointerId);
            pointerEvent.StopPropagation();
        }
        protected virtual void OnElementPointerMove(TElement element, PointerMoveEvent pointerEvent)
        {
            if (LockSort || !dragging || !element.index.HasMouseCapture()) return;

            MoveElement(element, pointerEvent.localPosition);
            MoveDragLine(CalculateDrag(element));

            pointerEvent.StopPropagation();
        }
        protected virtual ContextualMenuManipulator ElementContextMenu(TElement element, int index) => new ContextualMenuManipulator(e =>
        {
            DropdownMenuAction.Status moveUpStatus = (LockSort || index == 0) ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal;
            DropdownMenuAction.Status moveDownStatus = (LockSort || index == CollectionSize - 1) ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal;
            DropdownMenuAction.Status deleteStatus = (LockSize) ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal;

            e.menu.AppendSeparator();
            e.menu.AppendAction("Move Up", e => MoveItem(index, index - 1), moveUpStatus);
            e.menu.AppendAction("Move Down", e => MoveItem(index, index + 1), moveDownStatus);
            e.menu.AppendSeparator();
            e.menu.AppendAction("Move to Top", e => MoveItem(index, 0), moveUpStatus);
            e.menu.AppendAction("Move to Bottom", e => MoveItem(index, CollectionSize - 1), moveDownStatus);
            e.menu.AppendSeparator();
            e.menu.AppendAction("Delete", e => DeleteItem(index), deleteStatus);
        });

        private void MoveDragLine(int index)
        {
            if (index == draglineIndex) return;

            if (index == -1)
                dragline.RemoveFromHierarchy();
            else
                body.Insert(index, dragline);

            draglineIndex = index;
        }
        private void MoveElement(TElement element, Vector2 pointerPosition)
        {
            element.style.top = element.layout.y - (element.layout.height / 2) + pointerPosition.y;
            element.style.left = element.layout.x - (element.index.layout.width / 2) + pointerPosition.x;
        }
        private int CalculateDrag(TElement element)
        {
            if (dragTargets.IsEmpty()) return -1;

            TElement first = dragTargets[0];
            float left = first.layout.xMin;
            float right = first.layout.xMax;

            for (int i = -1; i < dragTargets.Length; i++)
            {
                float top = (i == -1) ? first.layout.yMin - 40 : dragTargets[i].layout.center.y;
                float bottom = (i + 1 == dragTargets.Length) ? dragTargets[i].layout.yMax + 40 : dragTargets[i + 1].layout.center.y;

                if (Rect.MinMaxRect(left, top, right, bottom).Contains(element.layout.center))
                    return i + 1;
            }

            return -1;
        }

        public void AddItem(TItem item)
        {
            try
            {
                if (Collection == null) throw new NullReferenceException("Null collection");
                if (OnAddItem == null) throw new NullReferenceException("Null action");
                if (LockSize) throw new Exception("Collection size is locked");

                if (CollectionSize < MaxSize)
                {
                    OnAddItem.Invoke(item);
                    iValue.Value = Collection;
                }
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed adding {item:ref} to {this:ref}"); }

            this.Refresh();
        }
        public void DeleteItem(int index)
        {
            try
            {
                if (Collection == null) throw new NullReferenceException("Null collection");
                if (OnDeleteItem == null) throw new NullReferenceException("Null action");
                if (LockSize) throw new Exception("Collection size is locked");

                if (CollectionSize > 0)
                {
                    OnDeleteItem.Invoke(index);
                    iValue.Value = Collection;
                }
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed removing [{index:info}] from {this:ref}"); }

            this.Refresh();
        }
        public void MoveItem(int index, int newIndex)
        {
            try
            {
                if (Collection == null) throw new NullReferenceException("Null collection");
                if (OnMoveItem == null) throw new NullReferenceException("Null action");
                if (LockSort) throw new Exception("Collection sorting is locked");

                if (index != newIndex)
                {
                    OnMoveItem.Invoke(index, newIndex);
                    iValue.Value = Collection;
                }
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed moving [{index:info}] to [{newIndex:info}] in {this:ref}"); }

            this.Refresh();
        }
        public void Resize(int size)
        {
            try
            {
                if (Collection == null) throw new NullReferenceException("Null collection");
                if (OnResize == null) throw new NullReferenceException("Null action");
                if (LockSize) throw new Exception("Collection size is locked");

                size = Mathf.Clamp(size, 0, MaxSize);
                if (size != CollectionSize)
                {
                    OnResize.Invoke(size);
                    iValue.Value = Collection;
                }
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed resizing {this:ref} to size {size:info}"); }

            this.Refresh();
        }

        protected virtual TItem DefaultItemFactory() => default;
        protected virtual void DefaultOnBindElement(TItem item, TElement element, int index) => element.Bind(item);
        protected virtual void DefaultOnAddItem(TItem item)
        {
            if (Collection == null) throw new NullReferenceException("Null collection");

            else if (Collection is TItem[] array)
                Collection = array.Append(item);

            else if (Collection is IList<TItem> iList && !iList.IsReadOnly)
                iList.Add(item);

            //else if (Collection is ICollection<TItem> iCollection && !iCollection.IsReadOnly)
            //{
            //}

            else throw new NotImplementedException($"Add Item action required for <{Collection.GetType().Name}>");
        }
        protected virtual void DefaultOnDeleteItem(int index)
        {
            if (Collection == null) throw new NullReferenceException("Null collection");

            else if (Collection is TItem[] array)
                Collection = array.Remove(index);

            else if (Collection is IList<TItem> iList && !iList.IsReadOnly)
                iList.RemoveAt(index);

            //else if (Collection is ICollection<TItem> iCollection && !iCollection.IsReadOnly)
            //{
            //}

            else throw new NotImplementedException($"Remove Item action required for <{Collection.GetType().Name}>");
        }
        protected virtual void DefaultOnMoveItem(int index, int newIndex)
        {
            if (Collection == null) throw new NullReferenceException("Null collection");

            else if (Collection is TItem[] array)
            {
                TItem item = array[index];
                if (index < newIndex)
                    for (int i = index; i < newIndex; i++)
                        array[i] = array[i + 1];
                else
                    for (int i = index; i > newIndex; i--)
                        array[i] = array[i - 1];
                array[newIndex] = item;
                Collection = array;
            }

            else if (Collection is IList<TItem> iList && !iList.IsReadOnly)
            {
                TItem item = iList[index];
                iList.RemoveAt(index);
                iList.Insert(newIndex, item);
            }

            //else if (Collection is ICollection<TItem> iCollection && !iCollection.IsReadOnly)
            //{
            //}

            else throw new NotImplementedException($"Move Item action required for <{Collection.GetType().Name}>");
        }
        protected virtual void DefaultOnResize(int size)
        {
            if (Collection == null) throw new NullReferenceException("Null collection");

            else if (Collection is TItem[] array)
            {
                TItem[] newArray = new TItem[size];
                for (int i = 0; i < size; i++)
                    newArray[i] = (array.OutOfRange(i)) ? ItemFactory.Invoke() : array[i];
                Collection = newArray;
            }

            else if (Collection is List<TItem> list)
            {
                list.Capacity = Mathf.Max(list.Count, size);
                while (list.Count < size) list.Add(ItemFactory.Invoke());
                while (list.Count > size) list.RemoveAt(list.Count - 1);
                list.TrimExcess();
            }

            else if (Collection is IList<TItem> iList && !iList.IsReadOnly)
            {
                while (iList.Count < size) iList.Add(ItemFactory.Invoke());
                while (iList.Count > size) iList.RemoveAt(iList.Count - 1);
            }

            //else if (Collection is ICollection<TItem> iCollection && !iCollection.IsReadOnly)
            //{
            //}

            else throw new NotImplementedException($"Resize action required for <{Collection.GetType().Name}>");
        }
    }
}
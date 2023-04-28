using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;


namespace Simplex
{
    public class DirectoryView<TItem> : DirectoryView<TItem, DirectoryElement> { }
    public class DirectoryView<TItem, TElement> : CollapsibleView where TElement : DirectoryElement, new()
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


        protected override string[] DefaultClasses => new string[] { "collapsible-view", "directory-view" };

        public readonly StringInput searchbar;

        private IValue iValue;
        public Dictionary<string, TElement> Directories { get; protected set; }
        public Dictionary<TItem, TElement> Items { get; protected set; }

        public virtual Action<TItem, bool> OnSelect { get; set; }
        public virtual Action<TItem, TElement> OnBindElement { get; set; }
        public virtual Func<TItem, string> ItemDirectory { get; set; }

        public bool Searchable
        {
            get => searchbar.resolvedStyle.display == DisplayStyle.Flex;
            set
            {
                searchbar.Display(value);
                if (!value) Search = null;
            }
        }
        public string Search
        {
            get => searchbar.CurrentValue;
            set
            {
                if (value == searchbar.CurrentValue) return;
                searchbar.CurrentValue = value;
            }
        }


        public DirectoryView()
        {
            header.Create<HorizontalSpace>().Size(Size.Mini);
            searchbar = header.Create<StringInput>("icon").Name("searchbar").Size(Size.Small).Bind(() => Search, value => Search = value);

            RegisterCallback<DetachFromPanelEvent>(OnDetach);
            RegisterCallback<RefreshEvent>(OnRefresh);

            Modify();
        }
        public DirectoryView<TItem, TElement> Modify(string title = null, bool searchable = false, bool collapsed = false)
        {
            Title = title;
            Searchable = searchable;
            Collapsed = collapsed;

            return this;
        }

        public DirectoryView<TItem, TElement> Bind<T>(Action<TItem, bool> onSelect, params TItem[] selected) where T : Enum, TItem
        {
            TItem[] enums = (TItem[])Enum.GetValues(typeof(T));
            void OnBindElement(TItem item, TElement element) => element.Modify(item.ToString().TitleCase(), null, selected.Contains(item), false).Bind(item);
            string ItemDirectory(TItem item) => null;

            return Bind(enums, onSelect, OnBindElement, ItemDirectory);
        }
        public DirectoryView<TItem, TElement> Bind<T>(IEnumerable<TItem> objects, Action<TItem, bool> onSelect, params TItem[] selected) where T : UnityEngine.Object, TItem
        {
#if UNITY_EDITOR
            void OnBindElement(TItem item, TElement element) { if (item is UnityEngine.Object obj) element.Modify(obj.name, (Texture2D)UnityEditor.EditorGUIUtility.ObjectContent(obj, obj.GetType()).image, selected.Contains(item)).Bind(item); }
            string ItemDirectory(TItem item) => (item is UnityEngine.Object obj) ? System.IO.Path.GetDirectoryName(UnityEditor.AssetDatabase.GetAssetPath(obj)) : null;
#else
            void OnBindElement(TItem item, TElement element) { if (item is UnityEngine.Object obj) element.Modify(obj.name, null, selected.Contains(item)).Bind(item); }
            string ItemDirectory(TItem item) => null;
#endif

            return Bind(objects, onSelect, OnBindElement, ItemDirectory);
        }
        public DirectoryView<TItem, TElement> Bind(IEnumerable<TItem> collection, Action<TItem, bool> onSelect, Action<TItem, TElement> onBindElement = null, Func<TItem, string> itemDirectory = null) => Bind(new DelegateValue<IEnumerable<TItem>>(() => collection, null, $"{typeof(TItem).Name}s"), onSelect, onBindElement, itemDirectory);
        public DirectoryView<TItem, TElement> Bind(IValue iValue, Action<TItem, bool> onSelect, Action<TItem, TElement> onBindElement = null, Func<TItem, string> itemDirectory = null)
        {
            this.iValue = iValue;
            if (string.IsNullOrEmpty(Title))
                Title = iValue.Name.TitleCase();

            OnSelect = onSelect;
            OnBindElement = onBindElement ?? DefaultOnBindElement;
            ItemDirectory = itemDirectory ?? DefaultItemDirectory;

            return this.Refresh();
        }

        protected virtual void OnDetach(DetachFromPanelEvent panelEvent) => DetachElements();
        protected virtual void OnRefresh(RefreshEvent refreshEvent)
        {
            DetachElements();
            AttachElements();
        }

        protected virtual void AttachElements()
        {
            IEnumerable<TItem> collection = (IEnumerable<TItem>)iValue?.Value;
            if (collection == null) return;

            foreach (TItem item in collection)
            {
                TElement element = ElementPool.Get();
                try
                {
                    OnBindElement.Invoke(item, element);
                    element.onClick = () => OnSelect.Invoke(item, element.Selected);

                    string directory = ItemDirectory.Invoke(item);
                    VisualElement parent = (string.IsNullOrEmpty(directory)) ? body : GetOrCreateDirectory(directory);
                    parent.Add(element);
                    Items.Add(item, element);
                }
                catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed binding and attaching {typeof(TElement):type} to {typeof(TItem):type} {Title:info} directory view"); }
            }
        }
        protected virtual void DetachElements()
        {
            Directories = new Dictionary<string, TElement>();
            Items = new Dictionary<TItem, TElement>();

            body.Query<TElement>().ForEach(element =>
            {
                try
                {
                    ElementPool.Release(element);
                    element.Modify();
                    element.RemoveFromHierarchy();
                }
                catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed detaching directory element {typeof(TElement):type}"); }
            });
        }

        protected virtual TElement GetOrCreateDirectory(string path)
        {
            path = path.Replace(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
            if (Directories.TryGetValue(path, out TElement element)) return element;

            string directory = System.IO.Path.GetDirectoryName(path).Replace(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
            VisualElement parent = (string.IsNullOrEmpty(directory)) ? body : GetOrCreateDirectory(directory);
            element = (TElement)parent.Create<TElement>().Modify(System.IO.Path.GetFileName(path));
            Directories.Add(path, element);
            return element;
        }

        protected virtual void DefaultOnBindElement(TItem item, TElement element) => element.Modify(item.ToString()).Bind(item);
        protected virtual string DefaultItemDirectory(TItem item) => null;
    }
}
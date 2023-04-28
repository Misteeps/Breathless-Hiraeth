using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Buffers.Binary;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex
{
    public enum Size { Mini, Small, Medium, Large, Huge }

    #region General Utilities
    public static class GeneralUtilities
    {
        public static bool IsEmpty<T>(this T[] array) => array == null || array.Length == 0;
        public static bool IsEmpty<T>(this List<T> list) => list == null || list.Count == 0;
        public static bool IsEmpty<T1, T2>(this Dictionary<T1, T2> dictionary) => dictionary == null || dictionary.Count == 0;
        public static bool OutOfRange<T>(this T[] array, int index) => array == null || index < 0 || index >= array.Length;
        public static bool OutOfRange<T>(this List<T> list, int index) => list == null || index < 0 || index >= list.Count;
        public static T[] Append<T>(this T[] array, IEnumerable<T> items, bool checkDuplicate = false)
        {
            if (array.IsEmpty()) return items.ToArray();

            int count = items.Count();
            if (count == 0) return array;

            int i;
            T[] newArray = new T[array.Length + count];
            for (i = 0; i < array.Length; i++)
                newArray[i] = array[i];

            if (checkDuplicate)
                foreach (T item in items)
                {
                    if (Array.IndexOf(newArray, item) != -1)
                    {
                        ConsoleUtilities.Warn($"{typeof(T):type} array already contains {item:ref}. Cancelled append");
                        return array;
                    }
                    newArray[i++] = item;
                }
            else
                foreach (T item in items)
                    newArray[i++] = item;

            return newArray;
        }
        public static T[] Append<T>(this T[] array, T item, bool checkDuplicate = false)
        {
            if (array.IsEmpty()) return new T[1] { item };

            if (checkDuplicate && Array.IndexOf(array, item) != -1)
            {
                ConsoleUtilities.Warn($"{typeof(T):type} array already contains {item:ref}. Cancelled append");
                return array;
            }

            T[] newArray = new T[array.Length + 1];
            for (int i = 0; i < array.Length; i++)
                newArray[i] = array[i];

            newArray[array.Length] = item;
            return newArray;
        }
        public static T[] Remove<T>(this T[] array, T item, bool checkExistance = false)
        {
            if (array.IsEmpty()) return array;

            int index = Array.IndexOf(array, item);

            if (checkExistance && index == -1)
            {
                ConsoleUtilities.Warn($"{typeof(T):type} array does not contain {item:ref}. Cancelled Remove");
                return array;
            }

            return (index == -1) ? array : Remove(array, index);
        }
        public static T[] Remove<T>(this T[] array, int index)
        {
            if (array.IsEmpty()) return array;
            if (array.OutOfRange(index)) throw new IndexOutOfRangeException();

            T[] newArray = new T[array.Length - 1];
            for (int i = 0, j = 0; i < array.Length; i++)
                if (i != index)
                    newArray[j++] = array[i];

            return newArray;
        }

        public static void ScaleImage(this Texture2D image, int width, int height, bool keepAspectRatio = false, bool mipmap = false, FilterMode filter = FilterMode.Bilinear)
        {
            if (keepAspectRatio)
            {
                float ratioW = image.width / (float)width;
                float ratioH = image.height / (float)height;
                float ratio = Mathf.Max(ratioW, ratioH);

                width = Mathf.RoundToInt(image.width / ratio);
                height = Mathf.RoundToInt(image.height / ratio);
            }

            RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            RenderTexture.active = rt;

            Graphics.Blit(image, rt);
            image.Reinitialize(width, height, image.format, mipmap);
            image.filterMode = filter;

            try
            {
                image.ReadPixels(new Rect(0.0f, 0.0f, width, height), 0, 0);
                image.Apply();
            }
            catch (Exception exception) { exception.Error($"Failed scaling image"); }

            RenderTexture.ReleaseTemporary(rt);
        }

        public static async Task DelayMS(int milliseconds)
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            Task<bool> DelayTask(int milliseconds)
            {
                TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();
                Transitions.Instance.StartCoroutine(DelayCoroutine(task, milliseconds));
                return task.Task;
            }
            IEnumerator DelayCoroutine(TaskCompletionSource<bool> task, int milliseconds)
            {
                yield return new WaitForSecondsRealtime(milliseconds / 1000f);
                task.TrySetResult(true);
            }

            await DelayTask(milliseconds);
#else
            await Task.Delay(milliseconds);
#endif
        }
        public static async Task DelayFrame(int frames)
        {
#if !UNITY_EDITOR
            Task<bool> DelayTask(int frames)
            {
                TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();
                Transitions.Instance.StartCoroutine(DelayCoroutine(task, frames));
                return task.Task;
            }
            IEnumerator DelayCoroutine(TaskCompletionSource<bool> task, int frames)
            {
                for (int i = 0; i < frames; i++)
                    yield return null;

                task.TrySetResult(true);
            }

            await DelayTask(frames);
#else
            await Task.Delay(frames);
#endif
        }

        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif !UNITY_WEBGL
            Application.Quit();
#endif
        }
    }
    #endregion General Utilities

    #region UI Utilities
    public static class UIUtilities
    {
        public static T Create<T>(params string[] classes) where T : VisualElement, new() => Create<T>(null, classes);
        public static T Create<T>(this VisualElement.Hierarchy hierarchy, params string[] classes) where T : VisualElement, new()
        {
            try
            {
                T element = new T();
                element.Class(false, classes);
                hierarchy.Add(element);
                return element;
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed creating {typeof(T):type} element"); return null; }
        }
        public static T Create<T>(this VisualElement parent, params string[] classes) where T : VisualElement, new()
        {
            try
            {
                T element = new T();
                element.Class(false, classes);
                parent?.Add(element);
                return element;
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed creating {typeof(T):type} element"); return null; }
        }

        public static T Name<T>(this T element, string name, bool toLower = true, bool replaceSpaces = true) where T : VisualElement
        {
            try
            {
                if (toLower) name = name?.ToLower();
                if (replaceSpaces) name = name?.Replace(' ', '_');
                element.name = name;
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed applying name to {element:ref}"); }
            return element;
        }
        public static T Tooltip<T>(this T element, string tooltip) where T : VisualElement
        {
            try { element.tooltip = tooltip; }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed applying tooltip to {element:ref}"); }
            return element;
        }
        public static T Focusable<T>(this T element, bool focusable) where T : VisualElement
        {
            try { element.focusable = focusable; }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting focusable of {element:ref}"); }
            return element;
        }
        public static T PickingMode<T>(this T element, PickingMode pickingMode) where T : VisualElement
        {
            try { element.pickingMode = pickingMode; }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting picking mode of {element:ref}"); }
            return element;
        }
        public static T UsageHints<T>(this T element, UsageHints usageHints) where T : VisualElement
        {
            try { element.usageHints = usageHints; }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting usage hints of {element:ref}"); }
            return element;
        }

        public static T Style<T>(this T element, params StyleSheet[] styles) where T : VisualElement => Style(element, false, styles);
        public static T Style<T>(this T element, bool clearExisting, params StyleSheet[] styles) where T : VisualElement
        {
            try
            {
                if (clearExisting) element.styleSheets.Clear();
                for (int i = 0; i < styles.Length; i++)
                    element.styleSheets.Add(styles[i]);
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed applying style to {element:ref}"); }
            return element;
        }
        public static T Class<T>(this T element, params string[] classes) where T : VisualElement => Class(element, false, classes);
        public static T Class<T>(this T element, bool clearExisting, params string[] classes) where T : VisualElement
        {
            try
            {
                if (clearExisting) element.ClearClassList();
                for (int i = 0; i < classes.Length; i++)
                    element.AddToClassList(classes[i]);
                if (!element.ClassListContains("static") && !element.ClassListContains("flexible"))
                    element.AddToClassList("static");
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed applying classes to {element:ref}"); }
            return element;
        }
        public static T ClassToggle<T>(this T element, string enabledClass, string disabledClass, bool enable) where T : VisualElement
        {
            try
            {
                if (enable)
                {
                    element.AddToClassList(enabledClass);
                    element.RemoveFromClassList(disabledClass);
                }
                else
                {
                    element.RemoveFromClassList(enabledClass);
                    element.AddToClassList(disabledClass);
                }
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed toggling classes in {element:ref}"); }
            return element;
        }

        public static T Text<T>(this T element, string text) where T : VisualElement
        {
            try
            {
                if (element is TextElement textElement) textElement.text = text;
                else throw new ArgumentException($"Unexpected type {element:type}");
                element.AddToClassList("text");
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed applying text to {element:ref}"); }
            return element;
        }
        public static T Text<T>(this T element, string text, Size size) where T : VisualElement
        {
            try
            {
                element.Text(text);
                element.Size(size);
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed applying text and size to {element:ref}"); }
            return element;
        }
        public static T Icon<T>(this T element, StyleBackground icon) where T : VisualElement
        {
            try
            {
                element.style.backgroundImage = icon;
                element.AddToClassList("icon");
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed applying icon to {element:ref}"); }
            return element;
        }
        public static T Icon<T>(this T element, StyleBackground icon, Size size) where T : VisualElement
        {
            try
            {
                element.Icon(icon);
                element.Size(size);
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed applying icon and size to {element:ref}"); }
            return element;
        }

        public static T Bind<T, TValue>(this T element, Func<TValue> getValue, Action<TValue> setValue) where T : VisualElement, IBindable
        {
            try { element.IValue = new DelegateValue<TValue>(getValue, setValue); }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed binding iValue to {element:ref}"); }
            return element.Refresh();
        }
        public static T Bind<T>(this T element, IValue iValue) where T : VisualElement, IBindable
        {
            try { element.IValue = iValue; }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed binding iValue to {element:ref}"); }
            return element.Refresh();
        }

        public static T OnClick<T>(this T element, EventCallback<ClickEvent> onClick) where T : VisualElement
        {
            try
            {
                element.RegisterCallback(onClick);
                element.pickingMode = UnityEngine.UIElements.PickingMode.Position;
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed registering click event to {element:ref}"); }
            return element;
        }
        public static T OnChange<T, TValue>(this T element, EventCallback<ChangeEvent<TValue>> onChange) where T : VisualElement
        {
            try { element.RegisterCallback(onChange); }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed registering change event to {element:ref}"); }
            return element;
        }
        public static T OnRefresh<T>(this T element, EventCallback<RefreshEvent> onRefresh) where T : VisualElement
        {
            try { element.RegisterCallback(onRefresh); }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed registering refresh event to {element:ref}"); }
            return element;
        }

        public static T Enable<T>(this T element, bool enabled) where T : VisualElement => Enable(element, enabled, (enabled) ? UnityEngine.UIElements.PickingMode.Position : UnityEngine.UIElements.PickingMode.Ignore);
        public static T Enable<T>(this T element, bool enabled, PickingMode pickingMode) where T : VisualElement
        {
            try
            {
                element.SetEnabled(enabled);
                element.pickingMode = pickingMode;
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting enabled of {element:ref}"); }
            return element;
        }
        public static T Refresh<T>(this T element) where T : VisualElement
        {
            async void Execute()
            {
                for (int i = 0; i < 10; i++)
                    if (element.panel != null) break;
                    else await GeneralUtilities.DelayFrame(1);

                using (RefreshEvent refreshEvent = RefreshEvent.GetPooled())
                {
                    refreshEvent.target = element;
                    element.SendEvent(refreshEvent);
                }

                for (int i = 0; i < element.hierarchy.childCount; i++)
                    element.hierarchy[i].Refresh();
            }

            try { Execute(); }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed refreshing {element:ref}"); }
            return element;
        }


        public static T Display<T>(this T element, bool display) where T : VisualElement => Display(element, (display) ? DisplayStyle.Flex : DisplayStyle.None);
        public static T Display<T>(this T element, StyleEnum<DisplayStyle> display) where T : VisualElement
        {
            try { element.style.display = display; }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting display of {element:ref}"); }
            return element;
        }

        public static T Visible<T>(this T element, bool visible) where T : VisualElement => Visible(element, (visible) ? Visibility.Visible : Visibility.Hidden);
        public static T Visible<T>(this T element, StyleEnum<Visibility> visible) where T : VisualElement
        {
            try { element.style.visibility = visible; }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting visibility of {element:ref}"); }
            return element;
        }

        public static T Flex<T>(this T element, bool flexible) where T : VisualElement
        {
            try { element.ClassToggle("flexible", "static", flexible); }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed applying flex class to {element:ref}"); }
            return element;
        }
        public static T Flex<T>(this T element, bool grow, bool shrink) where T : VisualElement
        {
            try
            {
                element.style.flexGrow = (grow) ? 1 : 0;
                element.style.flexShrink = (shrink) ? 1 : 0;
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting flex of {element:ref}"); }
            return element;
        }
        public static T Flex<T>(this T element, bool grow, bool shrink, FlexDirection direction) where T : VisualElement
        {
            try
            {
                element.style.flexGrow = (grow) ? 1 : 0;
                element.style.flexShrink = (shrink) ? 1 : 0;
                element.style.flexDirection = direction;
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting flex of {element:ref}"); }
            return element;
        }
        public static T Flex<T>(this T element, bool grow, bool shrink, FlexDirection direction, Wrap wrap) where T : VisualElement
        {
            try
            {
                element.style.flexGrow = (grow) ? 1 : 0;
                element.style.flexShrink = (shrink) ? 1 : 0;
                element.style.flexDirection = direction;
                element.style.flexWrap = wrap;
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting flex of {element:ref}"); }
            return element;
        }

        public static T Size<T>(this T element, Size size) where T : VisualElement
        {
            try
            {
                foreach (string sizeName in Enum.GetNames(typeof(Size)))
                    element.RemoveFromClassList(sizeName.ToLower());
                element.AddToClassList(size.ToString().ToLower());
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed applying size enum to {element:ref}"); }
            return element;
        }
        public static T Size<T>(this T element, int size) where T : VisualElement => Size(element, size, size);
        public static T Size<T>(this T element, int width, int height) where T : VisualElement
        {
            try
            {
                element.style.width = width;
                element.style.height = height;
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting size of {element:ref}"); }
            return element;
        }
        public static T Size<T>(this T element, int? width = null, int? height = null) where T : VisualElement
        {
            try
            {
                if (width != null) element.style.width = width.Value;
                if (height != null) element.style.height = height.Value;
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting size of {element:ref}"); }
            return element;
        }

        public static T Margin<T>(this T element, int size) where T : VisualElement => Margin(element, size, size, size, size);
        public static T Margin<T>(this T element, int top, int right, int bottom, int left) where T : VisualElement
        {
            try
            {
                element.style.marginTop = top;
                element.style.marginRight = right;
                element.style.marginBottom = bottom;
                element.style.marginLeft = left;
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting margin of {element:ref}"); }
            return element;
        }
        public static T Margin<T>(this T element, int? top = null, int? right = null, int? bottom = null, int? left = null) where T : VisualElement
        {
            try
            {
                if (top != null) element.style.marginTop = top.Value;
                if (right != null) element.style.marginRight = right.Value;
                if (bottom != null) element.style.marginBottom = bottom.Value;
                if (left != null) element.style.marginLeft = left.Value;
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting margin of {element:ref}"); }
            return element;
        }

        public static T Padding<T>(this T element, int size) where T : VisualElement => Padding(element, size, size, size, size);
        public static T Padding<T>(this T element, int top, int right, int bottom, int left) where T : VisualElement
        {
            try
            {
                element.style.paddingTop = top;
                element.style.paddingRight = right;
                element.style.paddingBottom = bottom;
                element.style.paddingLeft = left;
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting padding of {element:ref}"); }
            return element;
        }
        public static T Padding<T>(this T element, int? top = null, int? right = null, int? bottom = null, int? left = null) where T : VisualElement
        {
            try
            {
                if (top != null) element.style.paddingTop = top.Value;
                if (right != null) element.style.paddingRight = right.Value;
                if (bottom != null) element.style.paddingBottom = bottom.Value;
                if (left != null) element.style.paddingLeft = left.Value;
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting padding of {element:ref}"); }
            return element;
        }

        public static T BorderWidth<T>(this T element, int width) where T : VisualElement => BorderWidth(element, width, width, width, width);
        public static T BorderWidth<T>(this T element, int top, int right, int bottom, int left) where T : VisualElement
        {
            try
            {
                element.style.borderTopWidth = top;
                element.style.borderRightWidth = right;
                element.style.borderBottomWidth = bottom;
                element.style.borderLeftWidth = left;
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting border width of {element:ref}"); }
            return element;
        }
        public static T BorderWidth<T>(this T element, int? top = null, int? right = null, int? bottom = null, int? left = null) where T : VisualElement
        {
            try
            {
                if (top != null) element.style.borderTopWidth = top.Value;
                if (right != null) element.style.borderRightWidth = right.Value;
                if (bottom != null) element.style.borderBottomWidth = bottom.Value;
                if (left != null) element.style.borderLeftWidth = left.Value;
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting border width of {element:ref}"); }
            return element;
        }

        public static T BorderColor<T>(this T element, Color color) where T : VisualElement => BorderColor(element, color, color, color, color);
        public static T BorderColor<T>(this T element, Color top, Color right, Color bottom, Color left) where T : VisualElement
        {
            try
            {
                element.style.borderTopColor = top;
                element.style.borderRightColor = right;
                element.style.borderBottomColor = bottom;
                element.style.borderLeftColor = left;
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting border color of {element:ref}"); }
            return element;
        }
        public static T BorderColor<T>(this T element, Color? top = null, Color? right = null, Color? bottom = null, Color? left = null) where T : VisualElement
        {
            try
            {
                if (top != null) element.style.borderTopColor = top.Value;
                if (right != null) element.style.borderRightColor = right.Value;
                if (bottom != null) element.style.borderBottomColor = bottom.Value;
                if (left != null) element.style.borderLeftColor = left.Value;
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting border color of {element:ref}"); }
            return element;
        }

        public static T BorderRadius<T>(this T element, int radius) where T : VisualElement => BorderRadius(element, radius, radius, radius, radius);
        public static T BorderRadius<T>(this T element, int topLeft, int topRight, int bottomRight, int bottomLeft) where T : VisualElement
        {
            try
            {
                element.style.borderTopLeftRadius = topLeft;
                element.style.borderTopRightRadius = topRight;
                element.style.borderBottomRightRadius = bottomRight;
                element.style.borderBottomLeftRadius = bottomLeft;
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting border radius of {element:ref}"); }
            return element;
        }
        public static T BorderRadius<T>(this T element, int? topLeft = null, int? topRight = null, int? bottomRight = null, int? bottomLeft = null) where T : VisualElement
        {
            try
            {
                if (topLeft != null) element.style.borderTopLeftRadius = topLeft.Value;
                if (topRight != null) element.style.borderTopRightRadius = topRight.Value;
                if (bottomRight != null) element.style.borderBottomRightRadius = bottomRight.Value;
                if (bottomLeft != null) element.style.borderBottomLeftRadius = bottomLeft.Value;
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting border radius of {element:ref}"); }
            return element;
        }

        public static T Background<T>(this T element, Color color) where T : VisualElement
        {
            try { element.style.backgroundColor = color; }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting background color of {element:ref}"); }
            return element;
        }
        public static T Background<T>(this T element, Texture2D image) where T : VisualElement
        {
            try { element.style.backgroundImage = image; }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting background image of {element:ref}"); }
            return element;
        }
        public static T Background<T>(this T element, Texture2D image, BackgroundSizeType size) where T : VisualElement
        {
            try
            {
                element.style.backgroundImage = image;
                element.style.backgroundSize = new BackgroundSize(size);
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.uiTag, $"Failed setting background image of {element:ref}"); }
            return element;
        }
    }
    #endregion UI Utilities

    #region File Utilities
    public static class FileUtilities
    {
        #region CRC32
        public static class CRC32
        {
            private const uint seed = 0xFFFFFFFF;
            private static readonly uint[] table = new uint[]
            {
                0x00000000, 0x77073096, 0xEE0E612C, 0x990951BA, 0x076DC419, 0x706AF48F, 0xE963A535, 0x9E6495A3, 0x0EDB8832, 0x79DCB8A4, 0xE0D5E91E, 0x97D2D988, 0x09B64C2B, 0x7EB17CBD, 0xE7B82D07, 0x90BF1D91,
                0x1DB71064, 0x6AB020F2, 0xF3B97148, 0x84BE41DE, 0x1ADAD47D, 0x6DDDE4EB, 0xF4D4B551, 0x83D385C7, 0x136C9856, 0x646BA8C0, 0xFD62F97A, 0x8A65C9EC, 0x14015C4F, 0x63066CD9, 0xFA0F3D63, 0x8D080DF5,
                0x3B6E20C8, 0x4C69105E, 0xD56041E4, 0xA2677172, 0x3C03E4D1, 0x4B04D447, 0xD20D85FD, 0xA50AB56B, 0x35B5A8FA, 0x42B2986C, 0xDBBBC9D6, 0xACBCF940, 0x32D86CE3, 0x45DF5C75, 0xDCD60DCF, 0xABD13D59,
                0x26D930AC, 0x51DE003A, 0xC8D75180, 0xBFD06116, 0x21B4F4B5, 0x56B3C423, 0xCFBA9599, 0xB8BDA50F, 0x2802B89E, 0x5F058808, 0xC60CD9B2, 0xB10BE924, 0x2F6F7C87, 0x58684C11, 0xC1611DAB, 0xB6662D3D,
                0x76DC4190, 0x01DB7106, 0x98D220BC, 0xEFD5102A, 0x71B18589, 0x06B6B51F, 0x9FBFE4A5, 0xE8B8D433, 0x7807C9A2, 0x0F00F934, 0x9609A88E, 0xE10E9818, 0x7F6A0DBB, 0x086D3D2D, 0x91646C97, 0xE6635C01,
                0x6B6B51F4, 0x1C6C6162, 0x856530D8, 0xF262004E, 0x6C0695ED, 0x1B01A57B, 0x8208F4C1, 0xF50FC457, 0x65B0D9C6, 0x12B7E950, 0x8BBEB8EA, 0xFCB9887C, 0x62DD1DDF, 0x15DA2D49, 0x8CD37CF3, 0xFBD44C65,
                0x4DB26158, 0x3AB551CE, 0xA3BC0074, 0xD4BB30E2, 0x4ADFA541, 0x3DD895D7, 0xA4D1C46D, 0xD3D6F4FB, 0x4369E96A, 0x346ED9FC, 0xAD678846, 0xDA60B8D0, 0x44042D73, 0x33031DE5, 0xAA0A4C5F, 0xDD0D7CC9,
                0x5005713C, 0x270241AA, 0xBE0B1010, 0xC90C2086, 0x5768B525, 0x206F85B3, 0xB966D409, 0xCE61E49F, 0x5EDEF90E, 0x29D9C998, 0xB0D09822, 0xC7D7A8B4, 0x59B33D17, 0x2EB40D81, 0xB7BD5C3B, 0xC0BA6CAD,
                0xEDB88320, 0x9ABFB3B6, 0x03B6E20C, 0x74B1D29A, 0xEAD54739, 0x9DD277AF, 0x04DB2615, 0x73DC1683, 0xE3630B12, 0x94643B84, 0x0D6D6A3E, 0x7A6A5AA8, 0xE40ECF0B, 0x9309FF9D, 0x0A00AE27, 0x7D079EB1,
                0xF00F9344, 0x8708A3D2, 0x1E01F268, 0x6906C2FE, 0xF762575D, 0x806567CB, 0x196C3671, 0x6E6B06E7, 0xFED41B76, 0x89D32BE0, 0x10DA7A5A, 0x67DD4ACC, 0xF9B9DF6F, 0x8EBEEFF9, 0x17B7BE43, 0x60B08ED5,
                0xD6D6A3E8, 0xA1D1937E, 0x38D8C2C4, 0x4FDFF252, 0xD1BB67F1, 0xA6BC5767, 0x3FB506DD, 0x48B2364B, 0xD80D2BDA, 0xAF0A1B4C, 0x36034AF6, 0x41047A60, 0xDF60EFC3, 0xA867DF55, 0x316E8EEF, 0x4669BE79,
                0xCB61B38C, 0xBC66831A, 0x256FD2A0, 0x5268E236, 0xCC0C7795, 0xBB0B4703, 0x220216B9, 0x5505262F, 0xC5BA3BBE, 0xB2BD0B28, 0x2BB45A92, 0x5CB36A04, 0xC2D7FFA7, 0xB5D0CF31, 0x2CD99E8B, 0x5BDEAE1D,
                0x9B64C2B0, 0xEC63F226, 0x756AA39C, 0x026D930A, 0x9C0906A9, 0xEB0E363F, 0x72076785, 0x05005713, 0x95BF4A82, 0xE2B87A14, 0x7BB12BAE, 0x0CB61B38, 0x92D28E9B, 0xE5D5BE0D, 0x7CDCEFB7, 0x0BDBDF21,
                0x86D3D2D4, 0xF1D4E242, 0x68DDB3F8, 0x1FDA836E, 0x81BE16CD, 0xF6B9265B, 0x6FB077E1, 0x18B74777, 0x88085AE6, 0xFF0F6A70, 0x66063BCA, 0x11010B5C, 0x8F659EFF, 0xF862AE69, 0x616BFFD3, 0x166CCF45,
                0xA00AE278, 0xD70DD2EE, 0x4E048354, 0x3903B3C2, 0xA7672661, 0xD06016F7, 0x4969474D, 0x3E6E77DB, 0xAED16A4A, 0xD9D65ADC, 0x40DF0B66, 0x37D83BF0, 0xA9BCAE53, 0xDEBB9EC5, 0x47B2CF7F, 0x30B5FFE9,
                0xBDBDF21C, 0xCABAC28A, 0x53B39330, 0x24B4A3A6, 0xBAD03605, 0xCDD70693, 0x54DE5729, 0x23D967BF, 0xB3667A2E, 0xC4614AB8, 0x5D681B02, 0x2A6F2B94, 0xB40BBE37, 0xC30C8EA1, 0x5A05DF1B, 0x2D02EF8D
            };

            public static uint Calculate(byte[] bytes) => Calculate(bytes, 0, bytes.Length);
            public static uint Calculate(byte[] bytes, int offset, int length)
            {
                if (bytes == null) throw new ArgumentNullException("bytes").Overwrite(ConsoleUtilities.fileTag, $"Failed calculating CRC32");
                if (offset < 0 || length < 0 || offset + length > bytes.Length) throw new ArgumentOutOfRangeException().Overwrite(ConsoleUtilities.fileTag, $"Failed calculating CRC32");

                uint crc = 0;
                crc ^= seed;

                while (--length >= 0)
                    crc = table[(crc ^ bytes[offset++]) & 0xFF] ^ (crc >> 8);

                crc ^= seed;
                return crc;
            }
        }
        #endregion CRC32

        #region INI
        public class INI
        {
            public abstract class Line { }

            #region Section
            public class Section : Line
            {
                public string name;

                public Section(string name)
                {
                    this.name = name;
                }
                public override string ToString() => $"\n[{name}]";
            }
            #endregion Section
            #region Comment
            public class Comment : Line
            {
                public string text;

                public Comment(string text)
                {
                    this.text = text;
                }
                public override string ToString() => $"; {text}";
            }
            #endregion Comment
            #region Item
            public class Item : Line
            {
                public string key;
                public object value;

                public Item(string key, object value)
                {
                    this.key = key;
                    this.value = value;
                }
                public override string ToString() => $"{key}: {value}";
            }
            #endregion Item


            public List<Line> lines;


            public INI() => this.lines = new List<Line>();
            public INI(string header) => this.lines = new List<Line>() { new Comment(header) };
            public INI(List<Line> lines) => this.lines = lines;

            public static INI Create(string[] strings)
            {
                INI ini = new INI();

                for (int i = 0; i < strings.Length; i++)
                {
                    string str = strings[i].Trim();

                    if (string.IsNullOrEmpty(str)) continue;
                    else if (str.StartsWith(';')) ini.AddComment(str[1..].Trim());
                    else if (str.StartsWith('[') && str.EndsWith(']')) ini.AddSection(str[1..^1].Trim());
                    else if (str.Contains(':'))
                    {
                        int index = str.IndexOf(':');
                        ini.AddItem(str[..index].Trim(), str[(index + 1)..].Trim());
                    }
                    else Debug.LogWarning($"Unexpected INI line '{str}'");
                }

                return ini;
            }

            public string[] ToStrings()
            {
                List<string> strings = new List<string>(lines.Count);

                foreach (Line line in lines)
                    strings.Add(line.ToString());

                return strings.ToArray();
            }

            public void AddSection(string name) => lines.Add(new Section(name));
            public void AddComment(string text) => lines.Add(new Comment(text));
            public void AddItem(string key, object value) => lines.Add(new Item(key, value));

            public Section FindSection(string name) => (Section)lines.Find(line => line is Section section && section.name == name);
            public Comment FindComment(string text) => (Comment)lines.Find(line => line is Comment comment && comment.text == text);
            public Item FindItem(string key) => (Item)lines.Find(line => line is Item item && item.key == key);
        }
        #endregion INI
        #region PNG
        public class PNG
        {
            #region Chunk
            public readonly struct Chunk
            {
                public readonly byte[] bytes;

                public int LengthFull => bytes.Length;
                public int Length => bytes.Length - 12;
                public string Type => Encoding.ASCII.GetString(new ReadOnlySpan<byte>(bytes, 4, 4));
                public byte[] Data => bytes[8..(LengthFull - 4)];


                public Chunk(byte[] bytes) => this.bytes = bytes;

                public static Chunk Create(string type, byte[] data)
                {
                    List<byte> bytes = new List<byte>(data.Length + 12);
                    Span<byte> span = new Span<byte>(new byte[4]);

                    BinaryPrimitives.WriteUInt32BigEndian(span, (uint)data.Length);
                    bytes.AddRange(span.ToArray());

                    bytes.AddRange(Encoding.ASCII.GetBytes(type));

                    bytes.AddRange(data);

                    BinaryPrimitives.WriteUInt32BigEndian(span, CRC32.Calculate(bytes.ToArray()[4..]));
                    bytes.AddRange(span.ToArray());

                    return new Chunk(bytes.ToArray());
                }
                public static Chunk Create(string type, string data)
                {
                    return Create(type, Encoding.GetBytes(data));
                }
                public static Chunk Create(DateTime dateTime)
                {
                    byte[] data = new byte[7]
                    {
                        (byte)(dateTime.Year >> 8),
                        (byte)(dateTime.Year & 0xff),
                        (byte)dateTime.Month,
                        (byte)dateTime.Day,
                        (byte)dateTime.Hour,
                        (byte)dateTime.Minute,
                        (byte)dateTime.Second,
                    };

                    return Create("tIME", data);
                }
            }
            #endregion Chunk


            public static byte[] header = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            public List<Chunk> chunks;

            public DateTime Time
            {
                get
                {
                    Chunk chunk = Find("tIME");
                    if (chunk.bytes == null || chunk.Length != 7) return default;

                    ReadOnlySpan<byte> data = chunk.Data;
                    int year = BinaryPrimitives.ReadInt16BigEndian(data[0..2]);
                    return new DateTime(year, data[2], data[3], data[4], data[5], data[6], DateTimeKind.Local);
                }
            }


            public PNG() => this.chunks = new List<Chunk>();
            public PNG(List<Chunk> chunks) => this.chunks = chunks;

            public static PNG Create(Texture2D texture, params Chunk[] chunks) => Create(texture.EncodeToPNG(), chunks);
            public static PNG Create(byte[] bytes, params Chunk[] chunks)
            {
                PNG png = new PNG();

                ReadOnlySpan<byte> header = new ReadOnlySpan<byte>(bytes, 0, 8);
                int index = (header.SequenceEqual(PNG.header)) ? 8 : 0;

                while (index < bytes.Length)
                {
                    int length = (int)BinaryPrimitives.ReadUInt32BigEndian(new ReadOnlySpan<byte>(bytes, index, 4));
                    Chunk chunk = new Chunk(bytes[index..(index + length + 12)]);
                    index += chunk.LengthFull;
                    png.Append(chunk);
                }

                foreach (Chunk chunk in chunks)
                    png.Add(chunk);

                return png;
            }

            public Texture2D ToTexture()
            {
                Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                if (!texture.LoadImage(ToBytes())) Debug.LogWarning("Error converting PNG to Texture2D");

                return texture;
            }
            public byte[] ToBytes()
            {
                List<byte> bytes = header.ToList();

                foreach (Chunk chunk in chunks)
                    bytes.AddRange(chunk.bytes);

                return bytes.ToArray();
            }

            public void Append(Chunk chunk) => chunks.Add(chunk);
            public void Add(Chunk chunk)
            {
                int index = 0;
                for (int i = 0; i < chunks.Count; i++)
                    if (chunks[i].Type == "IDAT")
                    {
                        index = i;
                        break;
                    }

                chunks.Insert(index, chunk);
            }

            public Chunk Find(string type) => chunks.Find(chunk => chunk.Type == type);
        }
        #endregion PNG


        public static Encoding Encoding => Encoding.UTF8;
        public static string UserDataRoot => Application.persistentDataPath;
        public static string GameDataRoot => Application.dataPath;


        public static bool Save(string path, byte[] bytes)
        {
            try
            {
                path = ResolvePath(path);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, bytes);
                return true;
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.fileTag, $"Failed saving file to '{path}'"); return false; }
        }
        public static bool Save(string path, IEnumerable<string> lines)
        {
            try
            {
                path = ResolvePath(path);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllLines(path, lines, Encoding);
                return true;
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.fileTag, $"Failed saving file to '{path}'"); return false; }
        }
        public static bool Save(string path, string contents)
        {
            try
            {
                path = ResolvePath(path);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, contents, Encoding);
                return true;
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.fileTag, $"Failed saving file to '{path}'"); return false; }
        }
        public static bool Save(string path, INI ini)
        {
            try
            {
                if (ini == null) throw new ArgumentNullException("ini", "Missing INI reference");
                if (Path.GetExtension(path) != ".ini") throw new ArgumentException("Path missing .ini extension");
                return Save(path, ini.ToStrings());
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.fileTag, $"Failed saving INI file to '{path}'"); return false; }
        }
        public static bool Save(string path, PNG png)
        {
            try
            {
                if (png == null) throw new ArgumentNullException("png", "Missing PNG reference");
                if (Path.GetExtension(path) != ".png") throw new ArgumentException("Path missing .png extension");
                return Save(path, png.ToBytes());
            }
            catch (Exception exception) { exception.Error(ConsoleUtilities.fileTag, $"Failed saving PNG file to '{path}'"); return false; }
        }

        public static bool Load(string path, out byte[] bytes)
        {
            try { bytes = File.ReadAllBytes(ResolvePath(path)); }
            catch (Exception exception) { exception.Error(ConsoleUtilities.fileTag, $"Failed loading file at '{path}'"); bytes = null; }
            return bytes != null;
        }
        public static bool Load(string path, out string[] lines)
        {
            try { lines = File.ReadAllLines(ResolvePath(path), Encoding); }
            catch (Exception exception) { exception.Error(ConsoleUtilities.fileTag, $"Failed loading file at '{path}'"); lines = null; }
            return lines != null;
        }
        public static bool Load(string path, out string contents)
        {
            try { contents = File.ReadAllText(ResolvePath(path), Encoding); }
            catch (Exception exception) { exception.Error(ConsoleUtilities.fileTag, $"Failed loading file at '{path}'"); contents = null; }
            return contents != null;
        }
        public static bool Load(string path, out INI ini)
        {
            try { ini = INI.Create(File.ReadAllLines(ResolvePath(path), Encoding)); }
            catch (Exception exception) { exception.Error(ConsoleUtilities.fileTag, $"Failed loading INI file at '{path}'"); ini = null; }
            return ini != null;
        }
        public static bool Load(string path, out PNG png)
        {
            try { png = PNG.Create(File.ReadAllBytes(ResolvePath(path))); }
            catch (Exception exception) { exception.Error(ConsoleUtilities.fileTag, $"Failed loading PNG file at '{path}'"); png = null; }
            return png != null;
        }

        private static string ResolvePath(string path) => (Path.IsPathRooted(path)) ? path : Path.Combine(UserDataRoot, path);
    }
    #endregion File Utilities

    #region Console Utilities
    public static class ConsoleUtilities
    {
        #region Tag
        public class Tag
        {
            public readonly string name;
            public readonly string colorHex;


            public Tag(string name, Color color) : this(name, ColorUtility.ToHtmlStringRGB(color)) { }
            public Tag(string name, string colorHex)
            {
                this.name = name;
                this.colorHex = colorHex;
            }

            public override string ToString() => $"<b><color=#{colorHex}>{name}</color></b>";
        }
        #endregion Tag

        #region Formatter
        private class Formatter : IFormatProvider, ICustomFormatter
        {
            public object GetFormat(Type formatType) => (formatType == typeof(ICustomFormatter)) ? this : null;
            public string Format(string format, object arg, IFormatProvider formatProvider)
            {
                if (format == "ref") return RefFormat(arg);
                if (format == "info") return InfoFormat(arg);
                if (format == "type") return TypeFormat(arg);

                return null;
            }

            public string RefFormat(object obj)
            {
                if (NullOrEmptyObject(obj, out string str)) return str;

                if (obj is UnityEngine.Object unityObj)
                    str = unityObj.name;

                return $"<color=#A3A3A3><{obj.GetType().Name}> {str}</color>";
            }
            public string InfoFormat(object obj)
            {
                if (NullOrEmptyObject(obj, out string str)) return str;

                if (obj is UnityEngine.Object unityObj)
                    str = unityObj.name;

                return $"<color=#A3A3A3>{str}</color>";
            }
            public string TypeFormat(object obj)
            {
                if (NullOrEmptyObject(obj, out string str)) return str;

                str = (obj is Type type) ? type.Name : obj.GetType().Name;

                return $"<color=#A3A3A3><{str}></color>";
            }

            public bool NullOrEmptyObject(object obj, out string str)
            {
                if (obj == null)
                {
                    str = nullTag.ToString();
                    return true;
                }

                str = obj.ToString();

                if (string.IsNullOrWhiteSpace(str))
                {
                    str = emptyTag.ToString();
                    return true;
                }

                return false;
            }
        }
        #endregion Formatter


        public readonly static Tag uiTag = new Tag("[UI]", new Color(1, 1, 1));
        public readonly static Tag fileTag = new Tag("[File]", new Color(1, 1, 1));
        public readonly static Tag transitionTag = new Tag("[Transition]", new Color(1, 1, 1));
        public readonly static Tag settingsTag = new Tag("[Settings]", new Color(1, 1, 1));
        public readonly static Tag nullTag = new Tag("(null)", new Color(0.7f, 0.2f, 0.2f));
        public readonly static Tag emptyTag = new Tag("(?)", new Color(0.7f, 0.2f, 0.2f));

        private static Formatter formatter = new Formatter();


        public static string Format(this FormattableString str) => str?.ToString(formatter);
        public static string Format(this FormattableString str, Tag tag) => (tag == null) ? str.Format() : $"{tag} {str.Format()}";

        public static string TitleCase(this string str) => (string.IsNullOrEmpty(str)) ? str : System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Regex.Replace(str, "(?<!^)([A-Z][a-z]+)", " $1"));


        public static void Log(FormattableString message) => Debug.Log(message.Format());
        public static void Log(Tag tag, FormattableString message) => Debug.Log(message.Format(tag));

        public static void Warn(FormattableString message) => Debug.LogWarning(message.Format());
        public static void Warn(Tag tag, FormattableString message) => Debug.LogWarning(message.Format(tag));

        public static void Error(FormattableString message) => Debug.LogError(message.Format());
        public static void Error(Tag tag, FormattableString message) => Debug.LogError(message.Format(tag));

        public static void Error(this Exception exception) => Debug.LogException(exception);
        public static void Error(this Exception exception, FormattableString message) => Debug.LogException(exception.Overwrite(null, message));
        public static void Error(this Exception exception, Tag tag, FormattableString message) => Debug.LogException(exception.Overwrite(tag, message));

        public static Exception Overwrite(this Exception exception, FormattableString message) => Overwrite(exception, null, message);
        public static Exception Overwrite(this Exception exception, Tag tag, FormattableString message)
        {
            FieldInfo field = exception.GetType().GetField("_message", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(exception, $"{message.Format(tag)}\n> {exception.Message}");

            return exception;
        }


        public static void LogCollection<T>(this IEnumerable<T> collection) => LogCollection(collection, null, item => $"{item}");
        public static void LogCollection<T>(this IEnumerable<T> collection, FormattableString header) => LogCollection(collection, header, item => $"{item}");
        public static void LogCollection<T>(this IEnumerable<T> collection, FormattableString header, Func<T, FormattableString> stringify)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append((string.IsNullOrEmpty(header.Format())) ? $"{typeof(T).Name} Collection" : header.Format());
            builder.Append(" - ");
            builder.Append((collection == null) ? "null" : $"{collection.Count()} Items");

            if (collection != null)
            {
                int index = 0;
                foreach (T item in collection)
                    try { builder.Append($"\n{index++}: {stringify.Invoke(item).Format()}"); }
                    catch (Exception exception) { exception.Error($"Failed logging collection item [{index}] {item}"); }
            }

            Debug.Log(builder);
        }
    }
    #endregion Console Utilities
}
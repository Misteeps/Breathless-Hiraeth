using System;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

using UnityEditor;
using UnityEditor.UIElements;


namespace Simplex.Editor
{
    public class ObjectPicker : ObjectPicker<UnityEngine.Object> { }
    public class ObjectPicker<T> : Field<T> where T : UnityEngine.Object
    {
        protected override string[] DefaultClasses => new string[] { "field", "object", "inset", "text", "icon" };

        public readonly Div search;

        public string Title { get; set; }
        public string Filter { get; set; }
        public bool AllowSceneObjects { get; set; }

        public override T CurrentValue
        {
            set
            {
                base.CurrentValue = value;
                text = (value) ? ConsoleUtilities.Format($"{value:type} {value.name}") : ConsoleUtilities.Format($"{typeof(T):type} {ConsoleUtilities.nullTag}");
                style.backgroundImage = value.GetIcon();
            }
        }


        public ObjectPicker()
        {
            search = this.Create<Div>("search", "icon", "medium").PickingMode(PickingMode.Position);
            search.RegisterCallback<ClickEvent>(OnSearch);

            RegisterCallback<ClickEvent>(OnClick);
            RegisterCallback<KeyDownEvent>(OnKeyDown);
            RegisterCallback<DragEnterEvent>(OnDragEnter);
            RegisterCallback<DragLeaveEvent>(OnDragLeave);
            RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            RegisterCallback<DragPerformEvent>(OnDragPerform);

            Modify();
        }
        public ObjectPicker<T> Modify(string title = null, string filter = null, bool allowSceneObjects = false)
        {
            Title = title;
            Filter = filter;
            AllowSceneObjects = allowSceneObjects;

            return this;
        }

        protected virtual void OnSearch(ClickEvent clickEvent)
        {
            if (clickEvent.shiftKey)
            {
                Type selectorType = Type.GetType("UnityEditor.ObjectSelector, UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                PropertyInfo getProperty = selectorType.GetProperty("get", BindingFlags.Static | BindingFlags.Public);
                MethodInfo showMethod = selectorType.GetMethod("Show", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(UnityEngine.Object), typeof(Type), typeof(UnityEngine.Object), typeof(bool), typeof(List<int>), typeof(Action<UnityEngine.Object>), typeof(Action<UnityEngine.Object>), typeof(bool) }, null);
                object selector = getProperty.GetValue(null, null);
                Action<UnityEngine.Object> onSet = value => BindedValue = value as T;
                showMethod.Invoke(selector, new object[] { CurrentValue, typeof(T), null, AllowSceneObjects, null, null, onSet, true });
            }
            else
            {
                Popup popup = new Popup();
                popup.MaxHeight = 400;
                popup.Create<DirectoryView<T>>("flexible").Modify(Title, true).Bind<T>(AssetUtilities.Find<T>(Filter), (value, selected) => { BindedValue = value; popup.Close(); }, CurrentValue);
                popup.Open(this, true);
            }

            clickEvent.StopPropagation();
        }
        protected virtual void OnClick(ClickEvent clickEvent)
        {
            if (CurrentValue == null) return;

            if (clickEvent.clickCount == 1)
            {
                if (!clickEvent.shiftKey && !clickEvent.ctrlKey)
                    EditorGUIUtility.PingObject(CurrentValue);
            }
            else if (clickEvent.clickCount == 2)
            {
                AssetDatabase.OpenAsset(CurrentValue);
                GUIUtility.ExitGUI();
            }

            clickEvent.StopPropagation();
        }
        protected virtual void OnKeyDown(KeyDownEvent keyEvent)
        {
            if (keyEvent.keyCode is KeyCode.Space or KeyCode.Return or KeyCode.KeypadEnter)
            {
                keyEvent.StopPropagation();
                return;
            }

            if (keyEvent.keyCode is KeyCode.Delete)
            {
                BindedValue = null;

                keyEvent.StopPropagation();
                return;
            }
        }
        protected virtual void OnDragEnter(DragEnterEvent dragEvent)
        {
            this.ClassToggle("valid", "invalid", ValidateDrag(out _));

            dragEvent.StopPropagation();
        }
        protected virtual void OnDragLeave(DragLeaveEvent dragEvent)
        {
            RemoveFromClassList("valid");
            RemoveFromClassList("invalid");

            dragEvent.StopPropagation();
        }
        protected virtual void OnDragUpdated(DragUpdatedEvent dragEvent)
        {
            DragAndDrop.visualMode = (ValidateDrag(out _)) ? DragAndDropVisualMode.Generic : DragAndDropVisualMode.Rejected;

            dragEvent.StopPropagation();
        }
        protected virtual void OnDragPerform(DragPerformEvent dragEvent)
        {
            RemoveFromClassList("valid");
            RemoveFromClassList("invalid");

            if (ValidateDrag(out T value))
            {
                DragAndDrop.AcceptDrag();
                BindedValue = value;
            }

            dragEvent.StopPropagation();
        }

        protected virtual bool ValidateDrag(out T value)
        {
            UnityEngine.Object[] objectReferences = DragAndDrop.objectReferences;

            if (objectReferences.IsEmpty())
            {
                value = null;
                return false;
            }

            value = objectReferences[0] as T;
            return value != null;
        }
    }
}
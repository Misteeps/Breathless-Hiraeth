using System;
using System.Linq;
using System.Reflection;

using UnityEngine;
using UnityEngine.UIElements;

using UnityEditor;


namespace Simplex.Editor
{
    public class CustomInspector : UnityEditor.Editor
    {
        private EditorWindow editorWindow;
        public EditorWindow EditorWindow
        {
            get
            {
                if (editorWindow == null)
                {
                    EditorWindow[] windows = (EditorWindow[])Resources.FindObjectsOfTypeAll(WindowTypes.Inspector);
                    PropertyInfo trackerProperty = WindowTypes.Inspector.GetProperty("tracker");

                    for (int i = 0; i < windows.Length; i++)
                    {
                        EditorWindow window = windows[i];
                        ActiveEditorTracker tracker = trackerProperty.GetValue(window) as ActiveEditorTracker;
                        if (tracker.activeEditors.Contains(this))
                        {
                            editorWindow = window;
                            break;
                        }
                    }
                }

                return editorWindow;
            }
        }

        protected TemplateContainer TemplateContainer { get; set; }
        protected UnityEngine.UIElements.ScrollView ScrollViewContainer { get; set; }
        public VisualElement DefaultContainer { get; protected set; }
        public VisualElement Root { get; protected set; }

        protected override bool ShouldHideOpenButton() => true;
        public override bool UseDefaultMargins() => false;
        public virtual bool UseDefaultContainer => false;
        public override VisualElement CreateInspectorGUI() => DefaultContainer;


        protected virtual void OnEnable()
        {
            Undo.undoRedoPerformed += Refresh;

            DefaultContainer ??= new VisualElement();
            Root ??= new VisualElement();
            Root.style.flexGrow = 1;

            if (UseDefaultContainer)
                RestoreDefaultContainer();
            else
                DisplayCustomInspector();
        }
        protected virtual void OnDisable()
        {
            Undo.undoRedoPerformed -= Refresh;

            RestoreDefaultContainer();
        }
        private void Refresh() => Root?.Refresh();

        protected virtual void DisplayCustomInspector()
        {
            try
            {
                if (TemplateContainer == null || ScrollViewContainer == null)
                {
                    TemplateContainer = EditorWindow?.rootVisualElement.Q<TemplateContainer>();
                    ScrollViewContainer = TemplateContainer?.Q<UnityEngine.UIElements.ScrollView>();
                }

                ScrollViewContainer.Display(false);
                TemplateContainer.Insert(1, Root);
            }
            catch (Exception exception)
            {
                exception.Error(ConsoleUtilities.uiTag, $"Failed displaying custom inspector for {target:ref}");
                RestoreDefaultContainer();
            }
        }
        protected virtual void RestoreDefaultContainer()
        {
            ScrollViewContainer?.Display(StyleKeyword.Null);
            Root.RemoveFromHierarchy();
            DefaultContainer.Add(Root);
        }
    }
}
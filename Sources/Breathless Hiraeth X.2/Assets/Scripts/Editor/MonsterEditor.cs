using System;
using System.Linq;

using UnityEngine;
using UnityEngine.UIElements;

using UnityEditor;
using UnityEditor.SceneManagement;

using Simplex;
using Simplex.Editor;


namespace Game.Editor
{
    [CustomEditor(typeof(Monster))]
    public class MonsterEditor : UnityEditor.Editor
    {
        public Monster monster;
        public Div root;

        public override bool UseDefaultMargins() => false;


        private void OnEnable() => Undo.undoRedoPerformed += Refresh;
        private void OnDisable() => Undo.undoRedoPerformed -= Refresh;
        private void Refresh() => root?.Refresh();

        public override VisualElement CreateInspectorGUI()
        {
            monster = (Monster)target;
            root = UIUtilities.Create<Div>("body").Style(AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.misteeps.simplex/Editor/UI/Styles/Simplex Inspector Dark.uss"));

            return root.Refresh();
        }

        private void Edit(Action action)
        {
            Undo.RegisterCompleteObjectUndo(monster, "Edit Monster");
            action.Invoke();

            EditorUtility.SetDirty(monster);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}
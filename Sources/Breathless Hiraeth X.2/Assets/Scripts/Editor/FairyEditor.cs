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
    [CustomEditor(typeof(Fairy))]
    public class FairyEditor : UnityEditor.Editor
    {
        public Fairy fairy;
        public Div root;

        public override bool UseDefaultMargins() => false;


        private void OnEnable() => Undo.undoRedoPerformed += Refresh;
        private void OnDisable() => Undo.undoRedoPerformed -= Refresh;
        private void Refresh() => root?.Refresh();

        public override VisualElement CreateInspectorGUI()
        {
            fairy = (Fairy)target;
            root = UIUtilities.Create<Div>("body").Style(AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.misteeps.simplex/Editor/UI/Styles/Simplex Inspector Dark.uss"));

            //root.Create<VerticalSpace>();
            //root.Create<Labeled<ObjectPicker<ParticleSystem>>>().Bind(new DelegateValue<ParticleSystem>(() => fairy.particles, value => Edit(() => fairy.particles = value), "particleSystem"));
            //root.Create<Labeled<IntInput>>().Bind(new DelegateValue<int>(() => fairy.spinSpeed, value => Edit(() => fairy.spinSpeed = value), "spinSpeed"));
            //root.Create<CollectionView<Fairy.Position>>().Bind(() => fairy.positions, value => Edit(() => fairy.positions = value.ToArray()));

            return root.Refresh();
        }

        private void Edit(Action action)
        {
            Undo.RegisterCompleteObjectUndo(fairy, "Edit Fairy");
            action.Invoke();

            EditorUtility.SetDirty(fairy);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}
using System;

using UnityEngine;

using UnityEditor;

using Simplex;
using Simplex.Editor;


namespace Game.Editor
{
    [CustomEditor(typeof(Encounter))]
    public class EncounterEditor : UnityEditor.Editor
    {
        public Encounter encounter;
        public Div root;

        public override bool UseDefaultMargins() => false;


        private void OnEnable() => Undo.undoRedoPerformed += Refresh;
        private void OnDisable() => Undo.undoRedoPerformed -= Refresh;
        private void Refresh() => root?.Refresh();

        public override UnityEngine.UIElements.VisualElement CreateInspectorGUI()
        {
            encounter = (Encounter)target;
            encounter.trigger = encounter.GetComponent<SphereCollider>();

            root = UIUtilities.Create<Div>("body").Style(AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.StyleSheet>("Packages/com.misteeps.simplex/Editor/UI/Styles/Simplex Inspector Dark.uss"));
            root.Create<VerticalSpace>();
            root.Create<Labeled<ToggleCheck>>().Bind(encounter.IValue("patrol"));
            root.Create<Labeled<FloatInputSlider>>().Bind(encounter.IValue("Range")).Elements(e => e.Modify(10, 100, 0));

            return root.Refresh();
        }
    }
}
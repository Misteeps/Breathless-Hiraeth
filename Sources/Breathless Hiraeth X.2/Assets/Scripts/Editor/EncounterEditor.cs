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
        #region Type Element
        public class TypeElement : CollectionElement
        {
            public ObjectPicker<GameObject> prefab;
            public IntInput count;


            public TypeElement()
            {
                count = Row(0).Create<IntInput>("wide2").Tooltip("Count");
                prefab = Row(0).Create<ObjectPicker<GameObject>>("flexible").Modify("Monster Type", "l:monster").Tooltip("Prefab");
            }

            public override CollectionElement Bind(object item)
            {
                if (item is not Encounter.Type type) throw new ArgumentException("Invalid Type").Overwrite(ConsoleUtilities.uiTag, $"Failed binding {item:ref} to {this:ref}");

                prefab.Bind(type.encounter.IValue(type, "prefab"));
                count.Bind(type.encounter.IValue(type, "count"));

                return this.Refresh();
            }
        }
        #endregion Type Element
        #region Wave Element
        public class WaveElement : CollectionElement
        {
            public FloatInputSlider duration;
            public IntInputSlider threshold;
            public CollectionView<Encounter.Type, TypeElement> types;


            public WaveElement()
            {
                contents.Padding(top: 2, bottom: 2);

                Row(0).Create<Labeled<FloatInputSlider>>("flexible").Modify("Duration", Size.Small).Elements(e => e.Modify(1, 60, 0)).Elements(out duration);
                Row(1).Create<Labeled<IntInputSlider>>("flexible").Modify("Threshold", Size.Small).Elements(e => e.Modify(0, 20)).Elements(out threshold);
                types = Row(2).Create<CollectionView<Encounter.Type, TypeElement>>("flexible").Margin(top: 2);
            }

            public override CollectionElement Bind(object item)
            {
                if (item is not Encounter.Wave wave) throw new ArgumentException("Invalid Type").Overwrite(ConsoleUtilities.uiTag, $"Failed binding {item:ref} to {this:ref}");

                duration.Bind(wave.encounter.IValue(wave, "duration"));
                threshold.Bind(wave.encounter.IValue(wave, "threshold"));
                types.Bind(wave.encounter.IValue(wave, "types"), () => new Encounter.Type(wave.encounter));

                return this.Refresh();
            }
        }
        #endregion Wave Element


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
            root.Create<Labeled<Dropdown<Encounter.Status>>>().Modify("Start State").Elements(e => e.Bind<Encounter.Status>(encounter.IValue("startState")));
            root.Create<Labeled<FloatInputSlider>>().Bind(encounter.IValue("aggroTime")).Elements(e => e.Modify(0, 10, 1));
            root.Create<Labeled<FloatInputSlider>>().Bind(encounter.IValue("Range")).Elements(e => e.Modify(10, 100, 0));
            root.Create<Labeled<FloatInputSlider>>().Bind(encounter.IValue("chaseRangeScale")).Elements(e => e.Modify(1, 10));
            root.Create<VerticalSpace>();
            root.Create<CollectionView<Encounter.Wave, WaveElement>>().Bind(encounter.IValue("waves"), () => new Encounter.Wave(encounter));

            return root.Refresh();
        }
    }
}
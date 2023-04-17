using System;
using System.Linq;

using UnityEngine;

using UnityEditor;
using UnityEditor.SceneManagement;

using Simplex;
using Simplex.Editor;


namespace Game.Editor
{
    [CustomEditor(typeof(Fairy))]
    public class FairyEditor : UnityEditor.Editor
    {
        #region Position Element
        public class PositionElement : CollectionElement
        {
            public readonly FloatInput x;
            public readonly FloatInput y;
            public readonly FloatInput z;
            public readonly Button move;
            public readonly Button copy;
            public readonly StringInput dialog;


            public PositionElement()
            {
                x = Row(0).Create<FloatInput>("wide2", "flexible").Modify(delayed: true).Tooltip("Position X");
                y = Row(0).Create<FloatInput>("wide2", "flexible").Modify(delayed: true).Tooltip("Position Y");
                z = Row(0).Create<FloatInput>("wide2", "flexible").Modify(delayed: true).Tooltip("Position Z");
                Row(0).Create<HorizontalSpace>();
                move = Row(0).Create<Button>("wide3").Modify("Move");
                copy = Row(0).Create<Button>("wide3").Modify("Copy");
                dialog = Row(1).Create<StringInput>("flexible").Modify(multiline: true, delayed: true).Tooltip("Dialog");
            }

            public override CollectionElement Bind(object item)
            {
                if (item is not Fairy.Position position) throw new ArgumentException("Invalid Type").Overwrite(ConsoleUtilities.uiTag, $"Failed binding fairy position element to {item:ref}");

                x.Bind(position.fairy.IValue(position, "x"));
                y.Bind(position.fairy.IValue(position, "y"));
                z.Bind(position.fairy.IValue(position, "z"));
                move.Bind(_ => position.fairy.Edit(() => Move(position.fairy, position), "move"));
                copy.Bind(e => position.fairy.Edit(() => Copy(position.fairy, position, !e.shiftKey), "copy"));
                dialog.Bind(new DelegateValue<string>(() => (position.dialog.IsEmpty()) ? "" : string.Join('\n', position.dialog), value => position.fairy.Edit(() => position.dialog = (string.IsNullOrEmpty(value)) ? null : value.Split('\n'), "dialog")));

                return this.Refresh();
            }

            private void Move(Fairy fairy, Fairy.Position position) => fairy.transform.position = new Vector3(position.x, position.y, position.z);
            private void Copy(Fairy fairy, Fairy.Position position, bool calculateHover = true)
            {
                position.x = fairy.transform.position.x;
                position.y = fairy.transform.position.y;
                position.z = fairy.transform.position.z;

                if (calculateHover && Physics.Raycast(fairy.transform.position, Vector3.down, out RaycastHit hit, 10, 1 << LayerMask.NameToLayer("Map Terrain")))
                    position.y = Mathf.Round((hit.point.y + 2.5f) * 100) / 100;

                this.Refresh();
            }
        }
        #endregion Position Element


        public Fairy fairy;
        public Div root;

        public override bool UseDefaultMargins() => false;


        private void OnEnable() => Undo.undoRedoPerformed += Refresh;
        private void OnDisable() => Undo.undoRedoPerformed -= Refresh;
        private void Refresh() => root?.Refresh();

        public override UnityEngine.UIElements.VisualElement CreateInspectorGUI()
        {
            fairy = (Fairy)target;
            root = UIUtilities.Create<Div>("body").Style(AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.StyleSheet>("Packages/com.misteeps.simplex/Editor/UI/Styles/Simplex Inspector Dark.uss"));

            root.Create<VerticalSpace>();
            root.Create<Labeled<ObjectPicker<ParticleSystem>>>().Bind(fairy.IValue("particles"));
            root.Create<Labeled<ObjectPicker<SphereCollider>>>().Bind(fairy.IValue("trigger"));
            root.Create<VerticalSpace>();
            root.Create<Labeled<IntInput>>().Bind(fairy.IValue("spinSpeed"));
            root.Create<VerticalSpace>();
            root.Create<Labeled<StringInput>>().Bind(fairy.IValue("currentScene"));
            root.Create<Labeled<StringInput>>().Bind(fairy.IValue("nextScene"));
            root.Create<VerticalSpace>();
            root.Create<Labeled<IntInput, IntInput>>().Modify("Current").Elements(e1 => e1.Bind(fairy.IValue("currentPosition")).Tooltip("Position"), e2 => e2.Bind(fairy.IValue("currentDialog")).Tooltip("Dialog"));
            root.Create<VerticalSpace>();
            root.Create<CollectionView<Fairy.Position, PositionElement>>().Bind(fairy.IValue("positions"), () => new Fairy.Position(fairy));

            return root.Refresh();
        }
    }
}
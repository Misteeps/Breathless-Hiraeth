using System;

using UnityEngine;

using UnityEditor;

using Simplex;
using Simplex.Editor;


namespace Game.Editor
{
    [CustomEditor(typeof(Monster))]
    public class MonsterEditor : UnityEditor.Editor
    {
        #region Attack Element
        public class AttackElement : CollectionElement
        {
            public new StringInput name;
            public IntInput damage;
            public IntInput angle;


            public AttackElement()
            {
                contents.Padding(top: 2, bottom: 2);

                name = Row(0).Create<StringInput>("flexible");
                damage = Row(0).Create<IntInput>("wide2").Modify(0, 10);
                angle = Row(0).Create<IntInput>("wide2").Modify(0, 360);
            }

            public override CollectionElement Bind(object item)
            {
                if (item is not Monster.AttackData attack) throw new ArgumentException("Invalid Type").Overwrite(ConsoleUtilities.uiTag, $"Failed binding {item:ref} to {this:ref}");

                name.Bind(attack.monster.IValue(attack, "name"));
                damage.Bind(attack.monster.IValue(attack, "damage"));
                angle.Bind(attack.monster.IValue(attack, "angle"));

                return this.Refresh();
            }
        }
        #endregion Wave Element


        public Monster monster;
        public Div root;

        public override bool UseDefaultMargins() => false;


        private void OnEnable() => Undo.undoRedoPerformed += Refresh;
        private void OnDisable() => Undo.undoRedoPerformed -= Refresh;
        private void Refresh() => root?.Refresh();

        public override UnityEngine.UIElements.VisualElement CreateInspectorGUI()
        {
            monster = (Monster)target;
            monster.animator = monster.GetComponent<Animator>();
            monster.agent = monster.GetComponent<UnityEngine.AI.NavMeshAgent>();
            monster.hitbox = monster.GetComponent<CapsuleCollider>();

            monster.speedModifier = 1;
            if (monster.attacks == null) monster.attacks = new Monster.AttackData[0];

            root = UIUtilities.Create<Div>("body").Style(AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.StyleSheet>("Packages/com.misteeps.simplex/Editor/UI/Styles/Simplex Inspector Dark.uss"));
            root.Create<VerticalSpace>();
            root.Create<Labeled<FloatInputSlider>>().Bind(new DelegateValue<float>(() => monster.agent.radius, value => monster.Edit(() => SetHitbox(value, monster.agent.height), "radius"), "Radius")).Elements(e => e.Modify(0, 3));
            root.Create<Labeled<FloatInputSlider>>().Bind(new DelegateValue<float>(() => monster.agent.height, value => monster.Edit(() => SetHitbox(monster.agent.radius, value), "height"), "Height")).Elements(e => e.Modify(0, 3));
            root.Create<Labeled<Dropdown<Size>>>().Modify("Size").Elements(e => e.Bind<Size>(new DelegateValue<Size>(() => monster.size, value => monster.Edit(() => SetSize(value), "size"), "Size")));
            root.Create<VerticalSpace>();
            root.Create<CollectionView<Monster.AttackData, AttackElement>>().Bind(monster.IValue("attacks"), () => new Monster.AttackData(monster));

            return root.Refresh();
        }

        private void SetHitbox(float radius, float height)
        {
            monster.agent.radius = radius;
            monster.agent.height = height;
            monster.agent.stoppingDistance = radius + ((int)monster.size * 0.1f + 0.2f);

            monster.hitbox.radius = radius;
            monster.hitbox.height = height;
            monster.hitbox.center = new Vector3(0, height / 2, 0);
        }
        private void SetSize(Size size)
        {
            monster.size = size;

            monster.agent.speed = 4 - ((int)size * 0.25f);
            monster.agent.angularSpeed = 600 - ((int)size * 50);
            monster.agent.acceleration = 10 - ((int)size * 2);
            monster.agent.stoppingDistance = monster.agent.radius + ((int)size * 0.1f + 0.2f);

            monster.maxSpeed = monster.agent.speed;
        }
    }
}
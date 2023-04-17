using System;

using UnityEngine;

using Simplex;


namespace Game
{
    public class Fairy : MonoBehaviour
    {
        #region Position
        [Serializable]
        public class Position
        {
            public float x;
            public float y;
            public float z;
            public string[] dialog;

#if UNITY_EDITOR
            public Fairy fairy;
            public Position(Fairy fairy) => this.fairy = fairy;
#endif
        }
        #endregion Position


        public ParticleSystem particles;
        public SphereCollider trigger;

        [SerializeField] private int spinSpeed = 200;
        [SerializeField] private string currentScene;
        [SerializeField] private string nextScene;
        [SerializeField] private int currentPosition;
        [SerializeField] private int currentDialog;
        [SerializeField] public Position[] positions;
        public float SpinRadius
        {
            get => particles.shape.position.x;
            set
            {
                ParticleSystem.ShapeModule shape = particles.shape;
                new Transition(() => SpinRadius, value => shape.position = new Vector3(value, 0, 0), SpinRadius, value, "Fairy Spin").Curve(Function.Quadratic, Direction.Out, 600).Start();
            }
        }
        public int SpinSpeed { get => spinSpeed; set => spinSpeed = value; }
        public string CurrentScene => currentScene;
        public string NextScene => nextScene;
        public int CurrentPosition { get => currentPosition; private set => currentPosition = value; }
        public int CurrentDialog { get => currentDialog; private set => currentDialog = value; }


        private void Start()
        {
            if (positions.OutOfRange(CurrentPosition)) return;
            Position position = positions[CurrentPosition];
            transform.position = new Vector3(position.x, position.y, position.z);
            SpinRadius = 1;
        }
        private void Update()
        {
            transform.eulerAngles += new Vector3(0, SpinSpeed * Time.deltaTime, 0);
        }

        public bool DisplayDialog(int index, bool autoNextPosition = true)
        {
            Position position = positions[CurrentPosition];
            if (position.dialog.OutOfRange(index))
            {
                if (autoNextPosition) GoToPosition(CurrentPosition + 1);
                return false;
            }
            else
            {
                string dialog = position.dialog[index];
                Debug.Log(dialog);
                CurrentDialog = index;
                return true;
            }
        }
        public async void GoToPosition(int index)
        {
            trigger.enabled = false;

            if (positions.OutOfRange(index))
            {
                SpinRadius = 2;
                transform.Transition(TransformField.Position, Unit.Y, transform.position.y, transform.position.y - 1.6f).Curve(Function.Sine, Direction.InOut, 800).Start();
                await GeneralUtilities.DelayMS(800);
                transform.Transition(TransformField.Position, Unit.Y, transform.position.y, transform.position.y + 500).Curve(Function.Sine, Direction.In, 10f).Start();
            }
            else
            {
                Position position = positions[index];
                Vector2 start = new Vector2(transform.position.x, transform.position.z);
                Vector2 end = new Vector2(position.x, position.z);
                int duration = (int)(Vector2.Distance(start, end) * 120);

                transform.Transition(TransformField.Position, Unit.X, start.x, end.x).Curve(Function.Sine, Direction.InOut, duration).Start();
                transform.Transition(TransformField.Position, Unit.Z, start.y, end.y).Curve(Function.Sine, Direction.InOut, duration).Start();

                transform.Transition(TransformField.Position, Unit.Y, transform.position.y, transform.position.y - 1.6f).Curve(Function.Sine, Direction.InOut, 800).Start();
                await GeneralUtilities.DelayMS(800);
                transform.Transition(TransformField.Position, Unit.Y, transform.position.y, transform.position.y + 6.4f).Curve(Function.Sine, Direction.InOut, 1600).Start();
                await GeneralUtilities.DelayMS(1600);

                duration = Mathf.Max(1000, duration - 2000);
                transform.Transition(TransformField.Position, Unit.Y, transform.position.y, position.y).Curve(Function.Sine, Direction.InOut, duration).Start();
                await GeneralUtilities.DelayMS(Mathf.Max(0, duration - 400));

                SpinRadius = 1;
                trigger.enabled = true;
            }

            CurrentPosition = index;
            CurrentDialog = 0;
        }

        private static bool AdditiveLeave(Fairy fairy, string scene, int position)
        {
            return false;
        }
        private static bool OverrideLeave(Fairy fairy, string scene, int position)
        {
            return false;
        }
    }
}
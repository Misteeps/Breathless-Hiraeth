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
        [SerializeField] private float pressureScale;
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
        public float PressureScale => pressureScale;
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
            Monolith.Player.audio.PlayOneShot(Monolith.Refs.upgrade2, 0.4f);
            Position position = positions[CurrentPosition];
            if (position.dialog.OutOfRange(index))
            {
                if (autoNextPosition) GoToPosition(CurrentPosition + 1);
                UI.Hud.Instance.FairyDialog(null);
                return false;
            }
            else
            {
                UI.Hud.Instance.FairyDialog(position.dialog[index]);
                CurrentDialog = index;
                return true;
            }
        }
        public async void GoToPosition(int index)
        {
            Progress.position = index;
            Progress.Save();

            CurrentPosition = index;
            CurrentDialog = 0;

            if (OverrideLeave()) return;
            AdditiveLeave();

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

                int shortDuration = Mathf.Min(1200, (int)(duration * 0.2f));
                int longDuration = duration - shortDuration - shortDuration;

                transform.Transition(TransformField.Position, Unit.Y, transform.position.y, transform.position.y - 1.6f).Curve(Function.Sine, Direction.InOut, shortDuration).Start();
                await GeneralUtilities.DelayMS(shortDuration);
                transform.Transition(TransformField.Position, Unit.Y, transform.position.y, position.y + 1.6f).Curve(Function.Sine, Direction.InOut, longDuration).Start();
                await GeneralUtilities.DelayMS(longDuration);
                transform.Transition(TransformField.Position, Unit.Y, transform.position.y, position.y).Curve(Function.Sine, Direction.InOut, shortDuration).Start();
                await GeneralUtilities.DelayMS(shortDuration);

                SpinRadius = 1;
                trigger.enabled = true;
            }
        }

        public bool AdditiveEnter()
        {
            switch (CurrentScene)
            {
                default: return false;
                case "Grove of Beginnings" when CurrentPosition == 0:
                    UI.Hud.Instance.Tip("Forward dialog with [Left Click]");
                    return true;
            }
        }
        public bool AdditiveLeave()
        {
            switch (CurrentScene)
            {
                default: return false;
                case "Grove of Beginnings" when CurrentPosition == 1:
                    UI.Hud.Instance.Tip("Catch up to Auraline with [Left Shift]");
                    return true;
                case "Grove of Beginnings" when CurrentPosition == 2:
                    UI.Hud.Instance.Tip("If you are ever lost or forget where Auraline went,\nOpen the map with [Escape]", 6000);
                    return true;
                case "Grove of Beginnings" when CurrentPosition == 3:
                    UI.Hud.Instance.Tip("Swing your sword with [Left Click]\nZoom in and out with [Scroll Wheel]\nRotate the camera with [Scroll Click]", 8000);
                    return true;
            }
        }

        public bool OverrideEnter()
        {
            return false;
        }
        public bool OverrideLeave()
        {
            return false;
        }
    }
}
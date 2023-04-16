using System;

using UnityEngine;

using Simplex;


namespace Game
{
    public class Fairy : MonoBehaviour
    {
        #region Position
        [Serializable]
        public struct Position
        {
            public Vector3 position;
            public string[] dialog;
        }
        #endregion Position


        public ParticleSystem particles;

        public int spinSpeed = 200;
        public float SpinRadius
        {
            get => particles.shape.position.x;
            set
            {
                ParticleSystem.ShapeModule shape = particles.shape;
                new Transition(() => SpinRadius, value => shape.position = new Vector3(value, 0, 0), SpinRadius, value, "Fairy Spin").Curve(Function.Quadratic, Direction.Out, 600).Start();
            }
        }

        public string sceneWarp;
        public int currentDialog;
        public int currentPosition;
        public Position[] positions;


        private void Start()
        {
            if (positions.IsEmpty()) return;
            transform.position = positions[currentPosition].position;
            SpinRadius = 1;
        }
        private void Update()
        {
            transform.eulerAngles += new Vector3(0, spinSpeed * Time.deltaTime, 0);
        }

        public bool NextDialog(bool autoNextPosition = true)
        {
            Position position = positions[currentPosition];
            if (position.dialog.OutOfRange(currentDialog))
            {
                if (autoNextPosition) NextPosition();
                return false;
            }
            else
            {
                string dialog = position.dialog[currentDialog++];
                Debug.Log(dialog);
                return true;
            }
        }
        public void NextPosition()
        {
            transform.Transition(TransformField.Position, Unit.X, transform.position.x, transform.position.x + 150).Curve(Function.Sine, Direction.In, 10f).Start();
            transform.Transition(TransformField.Position, Unit.Z, transform.position.z, transform.position.z + 150).Curve(Function.Sine, Direction.In, 10f).Start();
            transform.Transition(TransformField.Position, Unit.Y, transform.position.y, transform.position.y + 5).Curve(Function.Back, Direction.In, 2400).Start();

            currentDialog = 0;
            currentPosition++;
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
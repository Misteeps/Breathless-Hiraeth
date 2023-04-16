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
            public float x;
            public float y;
            public float z;
            public string sceneJump;
            public string[] dialogs;
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
                new Transition(() => SpinRadius, value => shape.position = new Vector3(value, 0, 0), SpinRadius, value, "Fairy Spin").Curve(Function.Quadratic, Direction.Out, 400).Start();
            }
        }

        public Position[] positions;


        private void Start()
        {

        }
        private void Update()
        {
            transform.eulerAngles += new Vector3(0, spinSpeed * Time.deltaTime, 0);
        }
    }
}
using System;

using UnityEngine;

using Simplex;


namespace Game
{
    public class Fairy : MonoBehaviour
    {
        private ParticleSystem particles;

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


        void Start()
        {
            particles = GetComponent<ParticleSystem>();
        }
        void Update()
        {
            transform.eulerAngles += new Vector3(0, spinSpeed * Time.deltaTime, 0);

            if (Input.GetKeyDown(KeyCode.BackQuote)) SpinRadius = 0;
            if (Input.GetKeyDown(KeyCode.Alpha1)) SpinRadius = 1;
            if (Input.GetKeyDown(KeyCode.Alpha2)) SpinRadius = 2;
        }
    }
}
using System;

using UnityEngine;
using UnityEngine.AI;

using Simplex;


namespace Game
{
    public class Monster : MonoBehaviour
    {
        public Animator animator;
        public NavMeshAgent agent;
        public CapsuleCollider hitbox;

        [SerializeField] private Size size;
        public Size Size
        {
            get => size;
            set
            {
                size = value;
                agent.speed = 4 - ((int)size * 0.25f);
                agent.angularSpeed = 600 - ((int)size * 50);
            }
        }

        private int animIDMoveSpeed;


        private void Start()
        {
            animIDMoveSpeed = Animator.StringToHash("MoveSpeed");
        }

        public void Attack(Vector3 position)
        {
            agent.destination = position;
            animator.SetFloat(animIDMoveSpeed, Mathf.InverseLerp(0, agent.speed, new Vector2(agent.velocity.x, agent.velocity.y).magnitude));
        }
    }
}
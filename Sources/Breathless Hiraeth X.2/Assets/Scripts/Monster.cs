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

        public Encounter encounter;

        public float maxSpeed;
        public Size size;

        private int animIDMoveSpeed;


        private void Start()
        {
            animIDMoveSpeed = Animator.StringToHash("MoveSpeed");
        }
        private void Update()
        {
            animator.SetFloat(animIDMoveSpeed, Mathf.InverseLerp(0, agent.speed, new Vector2(agent.velocity.x, agent.velocity.y).magnitude));
        }

        public void Move(Vector3 position, float speedModifier = 1)
        {
            agent.destination = position;
            agent.speed = maxSpeed * speedModifier;
        }
    }
}
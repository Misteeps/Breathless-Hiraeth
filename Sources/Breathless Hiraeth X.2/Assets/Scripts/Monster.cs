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

        public float SpeedModifier { get; set; }

        private int animIDMoveSpeed;
        public int AnimDuration => (int)(animator.GetCurrentAnimatorStateInfo(0).length * 1000);


        private void Start()
        {
            SpeedModifier = 1;

            animIDMoveSpeed = Animator.StringToHash("MoveSpeed");
        }
        private void Update()
        {
            animator.SetFloat(animIDMoveSpeed, Mathf.InverseLerp(0, maxSpeed, new Vector2(agent.velocity.x, agent.velocity.y).magnitude));
        }

        public void Move(Vector3 position, float speedScale = 1)
        {
            agent.destination = position;
            agent.speed = maxSpeed * SpeedModifier * speedScale;
        }
        public async void Attack()
        {
            SpeedModifier = 0;

            animator.CrossFade("Attack", 0.25f);
            await GeneralUtilities.DelayMS(AnimDuration);

            SpeedModifier = 1;
        }
    }
}
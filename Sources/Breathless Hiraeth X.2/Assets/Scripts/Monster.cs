using System;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.AI;

using Simplex;


namespace Game
{
    public class Monster : MonoBehaviour
    {
        #region Attack Data
        [Serializable]
        public class AttackData
        {
            public string name;
            public int damage;
            public int angle;
            public float percent;

#if UNITY_EDITOR
            public Monster monster;
            public AttackData(Monster monster)
            {
                this.monster = monster;
                damage = 1;
                angle = 40;
                percent = 0.33f;
            }
#endif
        }
        #endregion Attack Data


        public Animator animator;
        public NavMeshAgent agent;
        public CapsuleCollider hitbox;

        public Encounter encounter;

        public float maxSpeed;
        public float speedModifier;
        public Size size;
        public AttackData[] attacks;

        private bool attackAnim;
        private bool lockAnim;

        public float Reach { get => agent.stoppingDistance + 0.5f; set => agent.stoppingDistance = value - 0.5f; }

        private int animIDMoveSpeed;
        public int AnimDuration => (int)(animator.GetCurrentAnimatorStateInfo(0).length * 1000);


        private void Start()
        {
            speedModifier = 1;

            animIDMoveSpeed = Animator.StringToHash("MoveSpeed");
        }
        private void Update()
        {
            animator.SetFloat(animIDMoveSpeed, Mathf.InverseLerp(0, maxSpeed, new Vector2(agent.velocity.x, agent.velocity.y).magnitude));
        }

        public void Move(Vector3 position, float speedScale = 1)
        {
            agent.destination = position;
            agent.speed = maxSpeed * speedModifier * speedScale;
        }
        public async void Attack()
        {
            if (attackAnim || lockAnim) return;

            attackAnim = true;

            if (attacks == null) ConsoleUtilities.Warn($"Monster {gameObject:info} has not attacks");
            else
            {
                AttackData attack = RNG.Generic.From(attacks);
                await Animate($"Attack {attack.name}");
                Debug.Log(attack.name);
            }

            attackAnim = false;
        }

        private async Task Animate(string clip)
        {
            speedModifier = 0.25f;

            animator.CrossFade(clip, 0.25f);
            await GeneralUtilities.DelayMS(AnimDuration);

            speedModifier = 1;
        }


#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, Reach);

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, agent.stoppingDistance);
        }
#endif
    }
}
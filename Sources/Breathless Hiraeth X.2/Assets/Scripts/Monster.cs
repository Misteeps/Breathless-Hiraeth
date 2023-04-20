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
            speedModifier = 0.25f;

            if (attacks == null) ConsoleUtilities.Warn($"Monster {gameObject:info} has not attacks");
            else
            {
                AttackData attack = RNG.Generic.From(attacks);
                animator.CrossFade($"Attack {attack.name}", 0.2f);
                await GeneralUtilities.DelayMS(AnimDuration(attack.percent));
                if (!attackAnim) return;
                AttackHit(attack.angle, attack.damage);
                await GeneralUtilities.DelayMS(AnimDuration(1 - attack.percent));
                if (!attackAnim) return;
            }

            attackAnim = false;
            speedModifier = 1;
        }

        private void AttackHit(float maxAngle, int damage)
        {
            float distance = Vector3.Distance(transform.position, Monolith.Player.transform.position);
            float angle = Vector3.Angle(Monolith.Player.transform.position - transform.position, transform.rotation * Vector3.forward);
            if (distance < 2 && angle < maxAngle)
                Monolith.Player.TakeDamage(damage);
        }

        private int AnimDuration(float percentage) => (int)(animator.GetCurrentAnimatorStateInfo(0).length * percentage * 1000);


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
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
        public Div healthbar;

        public int maxHealth;
        public int health;
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
            maxHealth = 30;
            health = maxHealth;

            agent.avoidancePriority = RNG.Generic.Int(0, 100);
            animIDMoveSpeed = Animator.StringToHash("MoveSpeed");
        }
        private void Update()
        {
            animator.SetFloat(animIDMoveSpeed, Mathf.InverseLerp(0, maxSpeed, new Vector2(agent.velocity.x, agent.velocity.y).magnitude));
            PositionHealthBar();
        }

        public void Move(Vector3 position, float speedScale = 1)
        {
            if (lockAnim) return;

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

        public void AttackHit(float maxAngle, int damage)
        {
            float distance = Vector3.Distance(transform.position, Monolith.Player.transform.position);
            float angle = Vector3.Angle(Monolith.Player.transform.position - transform.position, transform.rotation * Vector3.forward);
            if (distance < Reach && angle < maxAngle)
                Monolith.Player.TakeDamage(damage);
        }
        public async void TakeDamage(int damage)
        {
            if (health <= 0) return;

            attackAnim = false;
            lockAnim = true;
            speedModifier = 0;

            health -= damage;
            UpdateHealthbar();

            if (health <= 0) { Die(); return; }
            else
            {
                animator.CrossFade("Flinch", 0.08f);
                await GeneralUtilities.DelayMS(AnimDuration(1));
            }

            lockAnim = false;
            speedModifier = 1;
        }
        public async void Die()
        {
            lockAnim = true;
            speedModifier = 0;

            agent.enabled = false;
            hitbox.enabled = false;
            animator.CrossFade("Die", 0.08f);

            await GeneralUtilities.DelayMS(1000);
            RemoveHealthbar();

            await GeneralUtilities.DelayMS(4000);
            Destroy(gameObject);
        }

        private int AnimDuration(float percentage) => (int)(animator.GetCurrentAnimatorStateInfo(0).length * percentage * 1000);

        private void PositionHealthBar()
        {
            if (healthbar == null) return;

            Vector3 head = new Vector3(transform.position.x, transform.position.y + agent.height, transform.position.z);
            Vector2 position = UnityEngine.UIElements.RuntimePanelUtils.CameraTransformWorldToPanel(UI.Hud.Instance.panel, head, Monolith.Camera);
            healthbar.style.top = position.y - 40 - (healthbar.resolvedStyle.height / 2);
            healthbar.style.left = position.x - (healthbar.resolvedStyle.width / 2);
        }
        private void UpdateHealthbar()
        {
            if (healthbar == null)
            {
                healthbar = UIUtilities.Create<Div>("healthbar");
                healthbar.Create<Div>("bar");
                UI.Hud.Instance.Insert(0, healthbar);
            }

            healthbar[0].style.width = new UnityEngine.UIElements.Length((float)health / (float)maxHealth * 100f, UnityEngine.UIElements.LengthUnit.Percent);
        }
        private void RemoveHealthbar() => healthbar?.RemoveFromHierarchy();


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
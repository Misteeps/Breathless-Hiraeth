using System;
using System.Collections.Generic;

using UnityEngine;

using Simplex;


namespace Game
{
    public class Player : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 2.4f;
        public float combatSpeed = 3.6f;
        public float sprintSpeed = 5.8f;
        public float speedModifier = 1;
        [Range(0.0f, 0.3f)]
        public float rotationSpeed = 0.12f;
        public float acceleration = 10.0f;
        [Space(4)]
        public float jumpHeight = 1.2f;
        public float gravity = -15.0f;
        [Space(4)]
        public float jumpCooldown = 0f;
        public float fallTime = 0.15f;
        private const float terminalVelocity = 53.0f;

        [Header("Ground Check")]
        public bool grounded = true;
        public float groundOffset = -0.14f;
        public float groundRadius = 0.28f;
        public LayerMask groundLayers;

        [Header("Combat")]
        [SerializeField] private bool visibleSword;
        [SerializeField] private float combatTimer;
        public int attackChain;
        public float attackTimer;
        public bool VisibleSword
        {
            get => visibleSword;
            set
            {
                if (value == visibleSword) return;
                visibleSword = value;

                (int start, int end) = (visibleSword) ? (0, 1) : (1, 0);
                sword.Transition(TransformField.LocalScale, Unit.X, start, end).Curve(Function.Cubic, Direction.Out, 320).Start();
                sword.Transition(TransformField.LocalScale, Unit.Y, start, end).Curve(Function.Cubic, Direction.Out, 320).Start();
                sword.Transition(TransformField.LocalScale, Unit.Z, start, end).Curve(Function.Cubic, Direction.Out, 320).Start();
            }
        }
        public bool Combat
        {
            get => combatTimer != 0;
            set
            {
                if (value)
                {
                    combatTimer = 5;
                    animator.SetBool(animIDCombat, true);
                    VisibleSword = true;
                }
                else
                {
                    combatTimer = 0;
                    attackChain = 0;
                    animator.SetBool(animIDCombat, false);
                }
            }
        }

        [Header("References")]
        public Transform sword;
        public Animator animator;
        public CharacterController controller;

        [Header("Debug")]
        [SerializeField] private int health;
        [SerializeField] private bool invincible;
        [SerializeField] private bool lockActions;
        [SerializeField] private float speed;
        [SerializeField] private float animationSpeedX;
        [SerializeField] private float animationSpeedY;
        [SerializeField] private float targetRotation;
        [SerializeField] private float rotationVelocity;
        [SerializeField] private float verticalVelocity;
        [SerializeField] private float jumpCooldownTimer;
        [SerializeField] private float fallTimer;
        private List<Encounter> encounters;

        public int Health
        {
            get => health;
            set
            {
                health = value;
                UI.Hud.Instance.SetHealth(health);
            }
        }
        public bool CanAttack => !lockActions && !(Combat && attackTimer > 0);

        private int animIDMoveX;
        private int animIDMoveY;
        private int animIDCombat;
        private int animIDJump;
        private int animIDGrounded;
        private int animIDFreeFall;


        private void Start()
        {
            animIDMoveX = Animator.StringToHash("Move X");
            animIDMoveY = Animator.StringToHash("Move Y");
            animIDCombat = Animator.StringToHash("Combat");
            animIDJump = Animator.StringToHash("Jump");
            animIDGrounded = Animator.StringToHash("Grounded");
            animIDFreeFall = Animator.StringToHash("FreeFall");
        }
        private void Update()
        {
            CheckGround();
            MoveVertical();

            if (!Combat) MoveHorizontalNormal();
            else
            {
                MoveHorizontalCombat();

                attackTimer -= Time.deltaTime;
                if (Settings.autoExitCombat)
                {
                    combatTimer -= Time.deltaTime;
                    if (combatTimer < 0)
                        Combat = false;
                }
            }

            if (Inputs.Attack.Down && CanAttack) Attack();
            else if (Inputs.Ability1.Down && CanAttack) Ability1();
            else if (Inputs.Ability2.Down && CanAttack) Ability2();
            else if (Inputs.Ability3.Down && CanAttack) Ability3();
            else if (Inputs.Ability4.Down && CanAttack) Ability4();
            else if (Inputs.Sprint.Down && !lockActions) Combat = false;

            if (transform.position.y < 0)
                Enable(true, new Vector3(0, 100, 0));
        }

        public async void WakeUp()
        {
#if UNITY_EDITOR
            await GeneralUtilities.DelayFrame(1);
#else
            enabled = false;
            VisibleSword = false;
            animator.Play("Wake Up");

            await GeneralUtilities.DelayMS(8000);

            enabled = true;
            VisibleSword = true;
#endif
        }

        public void Enable(bool enabled, Vector3 position)
        {
            transform.eulerAngles = Vector3.zero;
            transform.position = position;
            Enable(enabled);
        }
        public void Enable(bool enabled)
        {
            Health = 10;
            grounded = true;
            invincible = false;
            lockActions = false;
            speed = 0;
            animationSpeedX = 0;
            animationSpeedY = 0;
            targetRotation = 0;
            rotationVelocity = 0;
            verticalVelocity = 0;
            jumpCooldownTimer = 0;
            fallTime = 0;
            encounters = new List<Encounter>();

            animator.Play("Movement 1D");
            animator.SetLayerWeight(0, 1);
            animator.SetLayerWeight(1, 1);
            animator.SetLayerWeight(2, 1);
            animator.SetLayerWeight(3, 0);

            this.enabled = enabled;
            animator.enabled = enabled;
            controller.enabled = enabled;
        }

        private void CheckGround()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundOffset, transform.position.z);
            grounded = Physics.CheckSphere(spherePosition, groundRadius, groundLayers, QueryTriggerInteraction.Ignore);
        }
        private void MoveVertical()
        {
            if (grounded)
            {
                fallTimer = fallTime;

                animator.SetBool(animIDJump, false);
                animator.SetBool(animIDGrounded, true);
                animator.SetBool(animIDFreeFall, false);

                if (verticalVelocity < 0.0f)
                    verticalVelocity = -2f;

                if (Inputs.Jump.Down && jumpCooldownTimer <= 0.0f && !lockActions)
                {
                    verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                    animator.SetBool(animIDJump, true);
                }

                if (jumpCooldownTimer >= 0.0f)
                    jumpCooldownTimer -= Time.deltaTime;
            }
            else
            {
                jumpCooldownTimer = jumpCooldown;

                animator.SetBool(animIDGrounded, false);

                if (fallTimer >= 0.0f) fallTimer -= Time.deltaTime;
                else animator.SetBool(animIDFreeFall, true);
            }

            if (verticalVelocity < terminalVelocity)
                verticalVelocity += gravity * Time.deltaTime;
        }
        private void MoveHorizontalNormal()
        {
            float targetSpeed = ((Inputs.Sprint.Held) ? sprintSpeed : moveSpeed) * speedModifier;
            Vector3 direction = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) direction += Vector3.forward;
            if (Input.GetKey(KeyCode.S)) direction += Vector3.back;
            if (Input.GetKey(KeyCode.A)) direction += Vector3.left;
            if (Input.GetKey(KeyCode.D)) direction += Vector3.right;

            if (direction == Vector3.zero) targetSpeed = 0.0f;
            speed = LerpSpeed(targetSpeed);

            if (direction != Vector3.zero)
            {
                direction = direction.normalized;
                targetRotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + Camera.Rotation;
                transform.rotation = LerpRotation(targetRotation);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;
            controller.Move(targetDirection.normalized * (speed * Time.deltaTime) + new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);

            animationSpeedY = Mathf.Lerp(animationSpeedY, targetSpeed, Time.deltaTime * acceleration);
            animator.SetFloat(animIDMoveY, animationSpeedY);
            VisibleSword = targetSpeed <= 3;
        }
        private void MoveHorizontalCombat()
        {
            Vector3 lookDirection = new Vector3(Inputs.MouseX - (Screen.width / 2), 0, Inputs.MouseY - (Screen.height / 2));
            targetRotation = Quaternion.LookRotation(lookDirection).eulerAngles.y + Camera.Rotation;
            transform.rotation = LerpRotation(targetRotation);

            float targetSpeed = combatSpeed * speedModifier;
            Vector3 moveDirection = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) moveDirection += Vector3.forward;
            if (Input.GetKey(KeyCode.S)) moveDirection += Vector3.back;
            if (Input.GetKey(KeyCode.A)) moveDirection += Vector3.left;
            if (Input.GetKey(KeyCode.D)) moveDirection += Vector3.right;

            if (moveDirection == Vector3.zero) targetSpeed = 0.0f;
            speed = LerpSpeed(targetSpeed);

            Vector3 targetDirection = Quaternion.Euler(0.0f, Camera.Rotation, 0.0f) * moveDirection;
            controller.Move(targetDirection.normalized * (speed * Time.deltaTime) + new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);

            Vector3 velocity = transform.InverseTransformDirection(targetDirection * targetSpeed);
            animationSpeedX = Mathf.Lerp(animationSpeedX, velocity.x, Time.deltaTime * acceleration);
            animationSpeedY = Mathf.Lerp(animationSpeedY, velocity.z, Time.deltaTime * acceleration);
            animator.SetFloat(animIDMoveX, animationSpeedX);
            animator.SetFloat(animIDMoveY, animationSpeedY);
        }

        private void ImpulseMovement(Vector3 moveDirection, float force, float duration = 0.25f, Function function = Function.Sine, Direction direction = Direction.InOut)
        {
            new Transition(() => force, value =>
            {
                force = value;
                controller.Move(moveDirection.normalized * (force * Time.deltaTime) + new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);

                Vector3 velocity = transform.InverseTransformDirection(moveDirection * force * 3);
                animationSpeedX = Mathf.Lerp(animationSpeedX, velocity.x, Time.deltaTime * acceleration);
                animationSpeedY = Mathf.Lerp(animationSpeedY, velocity.z, Time.deltaTime * acceleration);
                animator.SetFloat(animIDMoveX, animationSpeedX);
                animator.SetFloat(animIDMoveY, animationSpeedY);
            }, force, 2, "Player Move Impulse").Curve(function, direction, duration).Start();
        }
        private void ImpulseSpeed(float start, float end, float duration = 0.25f, Function function = Function.Exponential, Direction direction = Direction.Out) => new Transition(() => speedModifier, value => speedModifier = value, start, end, "Player Modify Speed").Curve(function, direction, duration).Start();

        private float LerpSpeed(float targetSpeed)
        {
            float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;
            float speedOffset = 0.1f;

            return (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
                ? Mathf.Round(Mathf.Lerp(currentHorizontalSpeed, targetSpeed, Time.deltaTime * acceleration) * 1000f) / 1000f
                : targetSpeed;
        }
        private Quaternion LerpRotation(float targetRotation) => Quaternion.Euler(0.0f, Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSpeed), 0.0f);

        private void Attack()
        {
            Combat = true;
            if (attackTimer < -0.4f)
                attackChain = 0;

            switch (attackChain)
            {
                default: attackChain = 1; goto case 0;

                case 0:
                    animator.CrossFade("Attack Slash Left", 0.3f);
                    ImpulseMovement(transform.forward, 4);
                    ImpulseSpeed(0, 1);

                    attackTimer = 0.25f;
                    attackChain = RNG.Generic.Int(1, 3);
                    break;

                case 1:
                    animator.CrossFade("Attack Slash Right", 0.3f);
                    ImpulseMovement(transform.forward, 5);
                    ImpulseSpeed(0, 1);

                    attackTimer = 0.25f;
                    attackChain = 3;
                    break;

                case 2:
                    animator.CrossFade("Attack Spin", 0.3f);
                    ImpulseMovement(transform.forward, 6);
                    ImpulseSpeed(0, 1);

                    attackTimer = 0.3f;
                    attackChain = 3;
                    break;

                case 3:
                    animator.CrossFade("Attack Slam", 0.3f);
                    ImpulseMovement(transform.forward, 3, 0.4f);
                    ImpulseSpeed(0, 1, 0.4f);

                    attackTimer = 0.4f;
                    attackChain = 0;
                    break;
            }
        }
        private void Ability1()
        {
            Combat = true;
        }
        private void Ability2()
        {
            Combat = true;
        }
        private void Ability3()
        {
            Combat = true;
        }
        private void Ability4()
        {
            Combat = true;
        }

        public async void AttackHit()
        {
            int hits = 0;
            foreach (Encounter encounter in encounters)
                foreach (Monster monster in encounter.monsters)
                {
                    float distance = Vector3.Distance(transform.position, monster.transform.position);
                    float angle = Vector3.Angle(monster.transform.position - transform.position, transform.rotation * Vector3.forward);
                    if (distance < 2 && angle < 90)
                    {
                        monster.TakeDamage(10);
                        hits++;
                    }
                }

            if (hits > 0)
            {
                Time.timeScale = 0.1f;
                await GeneralUtilities.DelayMS(50);
                Time.timeScale = 1;
            }
        }
        public async void TakeDamage(int damage)
        {
            if (invincible) return;

            animator.Play((RNG.Generic.Bool()) ? "Hurt Head" : "Hurt Stomach");
            Health -= damage;

            if (Health <= 0) { Die(); return; }
            else
            {
                invincible = true;
                await GeneralUtilities.DelayMS(320);
                invincible = false;
            }
        }
        public async void Die()
        {
            foreach (Encounter encounter in encounters)
                encounter.State = Encounter.Status.Patrol;

            this.enabled = false;
            controller.enabled = false;
            animator.SetLayerWeight(0, 0);
            animator.SetLayerWeight(1, 0);
            animator.SetLayerWeight(2, 0);
            animator.SetLayerWeight(3, 1);
            animator.CrossFade((RNG.Generic.Bool()) ? "Death Kneel" : "Death Fall", 0.2f);

            UI.Overlay.Instance.Transition(VisualElementField.BackgroundColor, Unit.A, 0, 1).Curve(Function.Sine, Direction.InOut, 6f).Start();
            await GeneralUtilities.DelayMS(6400);

            Enable(true, new Vector3(0, 100, 0));
            UI.Overlay.Instance.Transition(VisualElementField.BackgroundColor, Unit.A, 1, 0).Curve(Function.Sine, Direction.InOut, 6f).Start();
        }

        public void EnterEncounter(Encounter encounter) => encounters.Add(encounter);
        public void LeaveEncounter(Encounter encounter) => encounters.Remove(encounter);
        private bool CheckAggro()
        {
            foreach (Encounter encounter in encounters)
            {
                foreach (Monster monster in encounter.monsters)
                    if (Vector3.Distance(transform.position, monster.transform.position) < 50)
                        return true;

                encounter.State = Encounter.Status.Patrol;
                LeaveEncounter(encounter);
            }

            return false;
        }

        private void OnTriggerEnter(Collider other)
        {
            switch (other.gameObject.layer)
            {
                case 6: FairyTrigger(other.GetComponent<Fairy>()); break;
                case 28: other.GetComponent<Encounter>().State = Encounter.Status.Notice; break;
            }
        }
        private async void FairyTrigger(Fairy fairy)
        {
            fairy.trigger.enabled = false;

            if (CheckAggro())
            {
                UI.Hud.Instance.FairyDialog((RNG.Generic.Int(0, 4)) switch
                {
                    0 => "Shouldn't you deal with those monsters first?",
                    1 => "You seem a bit busy at the moment.",
                    2 => "Those monsters arn't backing off any time soon.",
                    _ => "Deal with those monsters first then we can resume our journey!",
                });

                while (Vector3.Distance(transform.position, fairy.transform.position) < 6)
                {
                    await GeneralUtilities.DelayFrame(1);
                    UI.Hud.Instance.PositionFairyDialog(fairy.transform.position);
                }

                UI.Hud.Instance.FairyDialog(null);
                fairy.trigger.enabled = true;
                return;
            }

            if (fairy.OverrideEnter()) return;
            fairy.AdditiveEnter();

            fairy.SpinRadius = 0;
            speedModifier = 0.6f;
            lockActions = true;
            Combat = false;

            if (fairy.DisplayDialog(fairy.CurrentDialog))
            {
                Camera.ZoomOffset = 4;
                while (true)
                {
                    await GeneralUtilities.DelayFrame(1);
                    UI.Hud.Instance.PositionFairyDialog(fairy.transform.position);

                    if (Inputs.Click.Down || Inputs.Breath.Down)
                        if (!fairy.DisplayDialog(fairy.CurrentDialog + 1))
                            break;

                    if (Vector3.Distance(transform.position, fairy.transform.position) > 6)
                    {
                        UI.Hud.Instance.FairyDialog(null);
                        fairy.trigger.enabled = true;
                        break;
                    }
                }
            }

            Camera.ZoomOffset = 0;
            speedModifier = 1;
            lockActions = false;
            Combat = false;
        }
    }
}
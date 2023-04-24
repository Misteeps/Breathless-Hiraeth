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

        [Header("Abilities")]
        public GameObject abilityNormal1;
        public GameObject abilityNormal2;
        public GameObject abilityNormal3;
        public GameObject abilityNormal4;
        public GameObject abilitySuper1;
        public GameObject abilitySuper2;
        public GameObject abilitySuper3;
        public GameObject abilitySuper4;
        public float abilityCooldown1;
        public float abilityCooldown2;
        public float abilityCooldown3;
        public float abilityCooldown4;

        [Header("VFX")]
        public ParticleSystem swordSlash1;
        public ParticleSystem swordSlash2;
        public ParticleSystem swordSlash3;
        public ParticleSystem heartsUpgrade;
        public ParticleSystem memoriesUpgrade;
        public ParticleSystem statsUpgrade;
        public ParticleSystem natureBoost;
        public ParticleSystem voidImplosionSmall;
        public ParticleSystem voidImplosionLarge;
        public ParticleSystem swordSummon;
        public ParticleSystem celestialBreath;
        public ParticleSystem celestialBurst;

        [Header("SFX")]
        public AudioClip slash1;
        public AudioClip slash2;
        public AudioClip slash3;
        public AudioClip slash4;
        public AudioClip hit1;
        public AudioClip hit2;
        public AudioClip footstep1;
        public AudioClip footstep2;
        public AudioClip hurt;

        [Header("References")]
        public Transform sword;
        public Animator animator;
        public CharacterController controller;
        public new AudioSource audio;
        public AudioSource footsteps;

        [Header("Debug")]
        [SerializeField] private int health;
        [SerializeField] private bool breathing;
        [SerializeField] public bool castingSuper;
        [SerializeField] public bool invincible;
        [SerializeField] public bool lockActions;
        [SerializeField] private bool aimAbility;
        [SerializeField] private float lastFootstep;
        [SerializeField] private float speed;
        [SerializeField] private float animationSpeedX;
        [SerializeField] private float animationSpeedY;
        [SerializeField] private float targetRotation;
        [SerializeField] private float rotationVelocity;
        [SerializeField] private float verticalVelocity;
        [SerializeField] private float jumpCooldownTimer;
        [SerializeField] private float fallTimer;
        [SerializeField] private float deltaTime;
        private List<Encounter> encounters;

        public int Health
        {
            get => health;
            set
            {
                health = Mathf.Clamp(value, 0, Progress.hearts);
                UI.Hud.Instance.SetHealth(health);
            }
        }
        public bool Breathing
        {
            get => breathing;
            set
            {
                if (value == breathing) return;

                if (value)
                {
                    enabled = false;
                    invincible = true;
                    VisibleSword = false;
                    Combat = false;
                    animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                    abilityCooldown1 = 0;
                    abilityCooldown2 = 0;
                    abilityCooldown3 = 0;
                    abilityCooldown4 = 0;
                    UI.Hud.Instance.UpdateAbilities();
                    float start = Monolith.Pressure;
                    new Transition(() => 0, value =>
                    {
                        VisibleSword = false;
                        Monolith.Pressure = Mathf.Lerp(start, 20, value);
                        animator.SetLayerWeight(5, value);
                        Time.timeScale = 1 - value;
                        Game.Camera.ColorAdjustments.saturation.value = Mathf.Lerp(0, -20, value);
                    }, 0, 1, "Breathing").Curve(Function.Circular, Direction.Out, 1000).Modify(1, true).Start();
                    breathing = true;
                }
                else
                {
                    breathing = false;
                    enabled = true;
                    invincible = false;
                    VisibleSword = false;
                    Combat = !Inputs.Sprint.Held;
                    animator.updateMode = AnimatorUpdateMode.Normal;
                    new Transition(() => 1, value => animator.SetLayerWeight(5, value), 1, 0).Curve(Function.Circular, Direction.Out, 2000).Modify(1, true).Start();
                    new Transition(() => 0, value =>
                    {
                        Monolith.Pressure = Mathf.Lerp(20, 0, value);
                        Time.timeScale = value;
                        Game.Camera.ColorAdjustments.saturation.value = Mathf.Lerp(-20, 0, value);
                    }, 0, 1, "Breathing").Curve(Function.Circular, Direction.Out, 200).Modify(1, true).Start();
                }
            }
        }
        public bool VisibleSword
        {
            get => visibleSword;
            set
            {
                if (value == visibleSword || Breathing) return;
                visibleSword = value;

                (int start, int end) = (visibleSword) ? (0, 1) : (1, 0);
                sword.Transition(TransformField.LocalScale, Unit.X, start, end).Curve(Function.Cubic, Direction.Out, 320).Modify(1, true).Start();
                sword.Transition(TransformField.LocalScale, Unit.Y, start, end).Curve(Function.Cubic, Direction.Out, 320).Modify(1, true).Start();
                sword.Transition(TransformField.LocalScale, Unit.Z, start, end).Curve(Function.Cubic, Direction.Out, 320).Modify(1, true).Start();
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
                    Camera.ZoomOffset = (Settings.combatZoom) ? -4 : 0;
                }
                else
                {
                    combatTimer = 0;
                    attackChain = 0;
                    aimAbility = false;
                    animator.SetBool(animIDCombat, false);
                    Camera.ZoomOffset = 0;
                }
            }
        }
        public bool CanAttack => !lockActions && !aimAbility && !(Combat && attackTimer > 0);
        public bool CanAbility => !lockActions && !aimAbility;

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
            deltaTime = Time.deltaTime;
            abilityCooldown1 -= deltaTime;
            abilityCooldown2 -= deltaTime;
            abilityCooldown3 -= deltaTime;
            abilityCooldown4 -= deltaTime;
            lastFootstep -= deltaTime;
            UI.Hud.Instance.CooldownAbilities();

            CheckGround();
            MoveVertical();

            if (!Combat) MoveHorizontalNormal();
            else
            {
                MoveHorizontalCombat();

                attackTimer -= deltaTime;
                if (Settings.autoExitCombat)
                {
                    combatTimer -= deltaTime;
                    if (combatTimer < 0)
                        Combat = false;
                }
            }

            if (Inputs.Attack.Down && CanAttack) Attack();
            else if (Inputs.Ability1.Down && CanAbility) TryAbility(1, abilityNormal1);
            else if (Inputs.Ability2.Down && CanAbility) TryAbility(2, abilityNormal2);
            else if (Inputs.Ability3.Down && CanAbility) TryAbility(3, abilityNormal3);
            else if (Inputs.Ability4.Down && CanAbility) TryAbility(4, abilityNormal4);
            else if (Inputs.Breath.Down && Monolith.Pressure >= 30 && grounded) Breath();
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
            transform.rotation = new Quaternion();
            transform.position = position;
            Enable(enabled);
        }
        public void Enable(bool enabled)
        {
            grounded = true;
            invincible = false;
            lockActions = false;
            aimAbility = false;
            abilityCooldown1 = 0;
            abilityCooldown2 = 0;
            abilityCooldown3 = 0;
            abilityCooldown4 = 0;
            castingSuper = false;
            invincible = false;
            lockActions = false;
            aimAbility = false;
            lastFootstep = 0;
            speed = 0;
            animationSpeedX = 0;
            animationSpeedY = 0;
            targetRotation = 0;
            rotationVelocity = 0;
            verticalVelocity = 0;
            jumpCooldownTimer = 0;
            fallTime = 0;
            encounters = new List<Encounter>();

            Health = Progress.hearts;
            Breathing = false;
            VisibleSword = false;
            Combat = false;
            UI.Hud.Instance.UpdateAbilities();

            this.enabled = enabled;
            animator.enabled = enabled;
            controller.enabled = enabled;

            animator.Play("Movement 1D");
            animator.SetLayerWeight(0, 1);
            animator.SetLayerWeight(1, 1);
            animator.SetLayerWeight(2, 1);
            animator.SetLayerWeight(3, 1);
            animator.SetLayerWeight(4, 0);
            animator.SetLayerWeight(5, 0);
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
                    jumpCooldownTimer -= deltaTime;
            }
            else
            {
                jumpCooldownTimer = jumpCooldown;

                animator.SetBool(animIDGrounded, false);

                if (fallTimer >= 0.0f) fallTimer -= deltaTime;
                else animator.SetBool(animIDFreeFall, true);
            }

            if (verticalVelocity < terminalVelocity)
                verticalVelocity += gravity * deltaTime;
        }
        private void MoveHorizontalNormal()
        {
            float targetSpeed = ((Inputs.Sprint.Held) ? sprintSpeed : moveSpeed) * speedModifier + (Progress.speed / 20f);
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
            controller.Move(targetDirection.normalized * (speed * deltaTime) + new Vector3(0.0f, verticalVelocity, 0.0f) * deltaTime);

            animationSpeedY = Mathf.Lerp(animationSpeedY, targetSpeed, deltaTime * acceleration);
            animator.SetFloat(animIDMoveY, animationSpeedY);
            VisibleSword = targetSpeed <= 3;
        }
        private void MoveHorizontalCombat()
        {
            Vector3 lookDirection = new Vector3(Inputs.MouseX - (Screen.width / 2), 0, Inputs.MouseY - (Screen.height / 2));
            targetRotation = Quaternion.LookRotation(lookDirection).eulerAngles.y + Camera.Rotation;
            transform.rotation = LerpRotation(targetRotation);

            float targetSpeed = combatSpeed * speedModifier + (Progress.speed / 20f);
            Vector3 moveDirection = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) moveDirection += Vector3.forward;
            if (Input.GetKey(KeyCode.S)) moveDirection += Vector3.back;
            if (Input.GetKey(KeyCode.A)) moveDirection += Vector3.left;
            if (Input.GetKey(KeyCode.D)) moveDirection += Vector3.right;

            if (moveDirection == Vector3.zero) targetSpeed = 0.0f;
            speed = LerpSpeed(targetSpeed);

            Vector3 targetDirection = Quaternion.Euler(0.0f, Camera.Rotation, 0.0f) * moveDirection;
            controller.Move(targetDirection.normalized * (speed * deltaTime) + new Vector3(0.0f, verticalVelocity, 0.0f) * deltaTime);

            Vector3 velocity = transform.InverseTransformDirection(targetDirection * targetSpeed);
            animationSpeedX = Mathf.Lerp(animationSpeedX, velocity.x, deltaTime * acceleration);
            animationSpeedY = Mathf.Lerp(animationSpeedY, velocity.z, deltaTime * acceleration);
            animator.SetFloat(animIDMoveX, animationSpeedX);
            animator.SetFloat(animIDMoveY, animationSpeedY);
        }

        private void ImpulseMovement(Vector3 moveDirection, float force, float duration = 0.25f, Function function = Function.Sine, Direction direction = Direction.InOut)
        {
            new Transition(() => force, value =>
            {
                force = value;
                controller.Move(moveDirection.normalized * (force * deltaTime) + new Vector3(0.0f, verticalVelocity, 0.0f) * deltaTime);

                Vector3 velocity = transform.InverseTransformDirection(moveDirection * force * 3);
                animationSpeedX = Mathf.Lerp(animationSpeedX, velocity.x, deltaTime * acceleration);
                animationSpeedY = Mathf.Lerp(animationSpeedY, velocity.z, deltaTime * acceleration);
                animator.SetFloat(animIDMoveX, animationSpeedX);
                animator.SetFloat(animIDMoveY, animationSpeedY);
            }, force, 2, "Player Move Impulse").Curve(function, direction, duration).Modify(1, true).Start();
        }
        private void ImpulseSpeed(float start, float end, float duration = 0.25f, Function function = Function.Exponential, Direction direction = Direction.Out) => new Transition(() => speedModifier, value => speedModifier = value, start, end, "Player Modify Speed").Curve(function, direction, duration).Modify(1, true).Start();

        private float LerpSpeed(float targetSpeed)
        {
            float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;
            float speedOffset = 0.1f;

            return (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
                ? Mathf.Round(Mathf.Lerp(currentHorizontalSpeed, targetSpeed, deltaTime * acceleration) * 1000f) / 1000f
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
                    audio.PlayOneShot(slash1);
                    swordSlash1.Play();
                    animator.CrossFade("Attack Slash Left", 0.3f);
                    ImpulseMovement(transform.forward, 4);
                    ImpulseSpeed(0, 1);

                    attackTimer = 0.25f;
                    attackChain = RNG.Generic.Int(1, 3);
                    break;

                case 1:
                    audio.PlayOneShot(slash2);
                    swordSlash2.Play();
                    animator.CrossFade("Attack Slash Right", 0.3f);
                    ImpulseMovement(transform.forward, 5);
                    ImpulseSpeed(0, 1);

                    attackTimer = 0.25f;
                    attackChain = 3;
                    break;

                case 2:
                    audio.PlayOneShot(slash3);
                    swordSlash2.Play();
                    animator.CrossFade("Attack Spin", 0.3f);
                    ImpulseMovement(transform.forward, 6);
                    ImpulseSpeed(0, 1);

                    attackTimer = 0.3f;
                    attackChain = 3;
                    break;

                case 3:
                    audio.PlayOneShot(slash4);
                    swordSlash3.Play();
                    animator.CrossFade("Attack Slam", 0.3f);
                    ImpulseMovement(transform.forward, 3, 0.4f);
                    ImpulseSpeed(0, 1, 0.4f);

                    attackTimer = 0.4f;
                    attackChain = 0;
                    break;
            }
        }
        private async void TryAbility(int index, GameObject abilityPrefab)
        {
            Label label;
            float cooldown;
            switch (index)
            {
                default: throw new ArgumentOutOfRangeException("Ability index");
                case 1: label = UI.Hud.Instance.ability1; cooldown = abilityCooldown1; break;
                case 2: label = UI.Hud.Instance.ability2; cooldown = abilityCooldown2; break;
                case 3: label = UI.Hud.Instance.ability3; cooldown = abilityCooldown3; break;
                case 4: label = UI.Hud.Instance.ability4; cooldown = abilityCooldown4; break;
            }

            if (cooldown > 0 || Progress.abilities < index)
            {
                Audio.UI.global.PlayOneShot(Monolith.Refs.uiError, 0.6f);
                label.AddToClassList("error");
                await GeneralUtilities.DelayMS(10);
                label.RemoveFromClassList("error");
            }
            else
            {
                label.AddToClassList("lit");
                Ability(index, abilityPrefab);
            }
        }
        private async void Ability(int index, GameObject abilityPrefab)
        {
            aimAbility = true;
            Combat = true;
            VisibleSword = false;

            if (Settings.abilityZoom)
                Camera.ZoomOffset = -8;

            Ability ability = Instantiate(abilityPrefab).GetComponent<Ability>();
            while (true)
            {
                await GeneralUtilities.DelayFrame(1);
                combatTimer = 5;
                ability.Aim();
                if (Inputs.Attack.Held)
                {
                    if (ability.aimDecal.enabled) { ability.Cast(); SetAbilityCooldown(index, ability.cooldown); }
                    else UI.Hud.Instance.Tip("Ability too far", 2000);
                    break;
                }
                if (Inputs.CancelAbility.Held || !aimAbility)
                {
                    ability.Destroy();
                    break;
                }
            }

            aimAbility = false;
            Combat = !Inputs.Sprint.Held;
            UI.Hud.Instance.UpdateAbilities();
        }
        private void SetAbilityCooldown(int index, float cooldown)
        {
            switch (index)
            {
                default: throw new ArgumentOutOfRangeException("Ability index");
                case 1: abilityCooldown1 = cooldown - (Progress.cooldown * 0.4f); break;
                case 2: abilityCooldown2 = cooldown - (Progress.cooldown * 0.4f); break;
                case 3: abilityCooldown3 = cooldown - (Progress.cooldown * 0.8f); break;
                case 4: abilityCooldown4 = cooldown - (Progress.cooldown * 0.8f); break;
            }
        }
        private async void Breath()
        {
            Breathing = true;
            while (Breathing)
            {
                await GeneralUtilities.DelayFrame(1);
                if (castingSuper) continue;

                Vector3 lookDirection = new Vector3(Inputs.MouseX - (Screen.width / 2), 0, Inputs.MouseY - (Screen.height / 2));
                targetRotation = Quaternion.LookRotation(lookDirection).eulerAngles.y + Camera.Rotation;
                transform.eulerAngles = new Vector3(0, targetRotation, 0);

                if (Inputs.Ability1.Down && CanAbility) TryAbility(1, abilitySuper1);
                else if (Inputs.Ability2.Down && CanAbility) TryAbility(2, abilitySuper2);
                else if (Inputs.Ability3.Down && CanAbility) TryAbility(3, abilitySuper3);
                else if (Inputs.Ability4.Down && CanAbility) TryAbility(4, abilitySuper4);
            }
            castingSuper = false;
        }
        public void Footstep()
        {
            if (lastFootstep > 0) return;
            lastFootstep = 0.2f;

            float volume = Mathf.InverseLerp(0, 6, speed);
            footsteps.PlayOneShot((RNG.Generic.Bool()) ? footstep1 : footstep2, volume);
        }

        public async void AttackHit()
        {
            bool hit = false;
            for (int i = 0; i < Monolith.encounters.Length; i++)
            {
                Encounter encounter = Monolith.encounters[i];
                if (!encounter.gameObject.activeSelf || Vector3.Distance(transform.position, encounter.transform.position) > encounter.ChaseRange) continue;
                foreach (Monster monster in encounter.monsters)
                {
                    float distance = Vector3.Distance(transform.position, monster.transform.position);
                    float angle = Vector3.Angle(monster.transform.position - transform.position, transform.rotation * Vector3.forward);
                    if (distance < 2 && angle < 135)
                    {
                        monster.TakeDamage(10 + (Progress.damage * 4));
                        hit = true;
                    }
                }
            }

            if (hit)
            {
                audio.PlayOneShot((RNG.Generic.Bool()) ? hit1 : hit2);
                Time.timeScale = 0.1f;
                await GeneralUtilities.DelayMS(60);
                Time.timeScale = 0.4f;
                await GeneralUtilities.DelayMS(40);
                Time.timeScale = 1;
            }
        }
        public async void TakeDamage(int damage)
        {
            if (invincible) return;

            switch (Monolith.PressureStage)
            {
                case 1: damage = Mathf.RoundToInt(damage * 2); break;
                case 2: damage = Mathf.RoundToInt(damage * 4); break;
                case 3: damage = Mathf.RoundToInt(damage * 6); break;
            }

            audio.PlayOneShot(hurt);
            animator.Play((RNG.Generic.Bool()) ? "Hurt Head" : "Hurt Stomach");
            Health -= damage;

            if (Health <= 0) { Die(); return; }
            else
            {
                invincible = true;
                await GeneralUtilities.DelayMS(1200);
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
            animator.SetLayerWeight(3, 0);
            animator.SetLayerWeight(4, 1);
            animator.SetLayerWeight(5, 0);
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
                case 7: MemoryTrigger(other.gameObject); break;
                case 28: other.GetComponent<Encounter>().State = Encounter.Status.Notice; break;
            }
        }
        private async void FairyTrigger(Fairy fairy)
        {
            fairy.trigger.enabled = false;
            aimAbility = false;

            if (CheckAggro())
            {
                audio.PlayOneShot(Monolith.Refs.upgrade2, 0.4f);
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
        private void MemoryTrigger(GameObject memory)
        {
            memory.GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            memory.GetComponent<AudioSource>().Transition(AudioSourceField.Volume, Unit.X, 0.2f, 0, () => memory.SetActive(false)).Curve(Function.Quadratic, Direction.Out, 800).Start();
            if (Progress.guids.Contains(memory.name))
                return;

            audio.PlayOneShot(Monolith.Refs.upgrade2, 0.6f);
            Progress.guids.Add(memory.name);
            Progress.memories++;
            memoriesUpgrade.Play();
            UI.Hud.Instance.Banner("Memory Found");
        }
    }
}
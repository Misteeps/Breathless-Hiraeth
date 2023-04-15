using UnityEngine;

using Cinemachine;
using Simplex;


namespace Game
{
    public class Player : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 2.4f;
        public float combatSpeed = 3.6f;
        public float sprintSpeed = 5.8f;
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

        [Header("States")]
        [SerializeField] private bool visibleSword;
        [SerializeField] private float combatTimer;
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
                    animator.SetBool(animIDCombat, false);
                }
            }
        }

        [Header("References")]
        public Transform sword;
        public Animator animator;
        public CharacterController controller;

        [Header("Debug")]
        [SerializeField] private float speed;
        [SerializeField] private float animationSpeedX;
        [SerializeField] private float animationSpeedY;
        [SerializeField] private float targetRotation;
        [SerializeField] private float rotationVelocity;
        [SerializeField] private float verticalVelocity;
        [SerializeField] private float jumpCooldownTimer;
        [SerializeField] private float fallTimer;

        private int animIDMoveX;
        private int animIDMoveY;
        private int animIDCombat;
        private int animIDJump;
        private int animIDGrounded;
        private int animIDFreeFall;


        public void Start()
        {
            jumpCooldownTimer = jumpCooldown;
            fallTimer = fallTime;

            animIDMoveX = Animator.StringToHash("Move X");
            animIDMoveY = Animator.StringToHash("Move Y");
            animIDCombat = Animator.StringToHash("Combat");
            animIDJump = Animator.StringToHash("Jump");
            animIDGrounded = Animator.StringToHash("Grounded");
            animIDFreeFall = Animator.StringToHash("FreeFall");
        }
        public void Update()
        {
            CheckGround();
            MoveVertical();
            if (Combat) MoveHorizontalCombat();
            else MoveHorizontalNormal();

            if (Inputs.Attack.Down) Combat = true;
            if (Inputs.Ability1.Down) Combat = true;
            if (Inputs.Ability2.Down) Combat = true;
            if (Inputs.Ability3.Down) Combat = true;
            if (Inputs.Ability4.Down) Combat = true;
            else if (Inputs.Dash.Down) Combat = false;
            else if (Combat && Settings.autoExitCombat)
            {
                combatTimer -= Time.deltaTime;
                if (combatTimer < 0)
                    Combat = false;
            }

            if (transform.position.y < -20)
                Enable(true, new Vector3(0, 100, 0));
        }

        public void Enable(bool enabled, Vector3 position)
        {
            transform.eulerAngles = Vector3.zero;
            transform.position = position;
            Enable(enabled);
        }
        public void Enable(bool enabled)
        {
            grounded = true;
            speed = 0;
            animationSpeedX = 0;
            animationSpeedY = 0;
            targetRotation = 0;
            rotationVelocity = 0;
            verticalVelocity = 0;
            jumpCooldownTimer = 0;
            fallTime = 0;

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

                if (Inputs.Jump.Down && jumpCooldownTimer <= 0.0f)
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
            float targetSpeed = Inputs.Dash.Held ? sprintSpeed : moveSpeed;
            Vector3 direction = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) direction += Vector3.forward;
            if (Input.GetKey(KeyCode.S)) direction += Vector3.back;
            if (Input.GetKey(KeyCode.A)) direction += Vector3.left;
            if (Input.GetKey(KeyCode.D)) direction += Vector3.right;

            if (direction == Vector3.zero) targetSpeed = 0.0f;
            speed = LerpSpeed(targetSpeed);

            controller.Move(direction.normalized * (speed * Time.deltaTime) + new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);
            if (direction != Vector3.zero)
            {
                direction = direction.normalized;
                targetRotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                transform.rotation = LerpRotation(targetRotation);
            }

            animationSpeedY = Mathf.Lerp(animationSpeedY, targetSpeed, Time.deltaTime * acceleration);
            animator.SetFloat(animIDMoveY, animationSpeedY);
            VisibleSword = targetSpeed <= 3;
        }
        private void MoveHorizontalCombat()
        {
            Vector3 lookDirection = new Vector3(Inputs.MouseX - (Screen.width / 2), 0, Inputs.MouseY - (Screen.height / 2));
            transform.rotation = LerpRotation(Quaternion.LookRotation(lookDirection).eulerAngles.y);

            float targetSpeed = combatSpeed;
            Vector3 moveDirection = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) moveDirection += Vector3.forward;
            if (Input.GetKey(KeyCode.S)) moveDirection += Vector3.back;
            if (Input.GetKey(KeyCode.A)) moveDirection += Vector3.left;
            if (Input.GetKey(KeyCode.D)) moveDirection += Vector3.right;

            if (moveDirection == Vector3.zero) targetSpeed = 0.0f;
            speed = LerpSpeed(targetSpeed);

            controller.Move(moveDirection.normalized * (speed * Time.deltaTime) + new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);

            Vector3 velocity = transform.InverseTransformDirection(moveDirection * targetSpeed);
            animationSpeedX = Mathf.Lerp(animationSpeedX, velocity.x, Time.deltaTime * acceleration);
            animationSpeedY = Mathf.Lerp(animationSpeedY, velocity.z, Time.deltaTime * acceleration);
            animator.SetFloat(animIDMoveX, animationSpeedX);
            animator.SetFloat(animIDMoveY, animationSpeedY);
        }
        private float LerpSpeed(float targetSpeed)
        {
            float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;
            float speedOffset = 0.1f;

            return (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
                ? Mathf.Round(Mathf.Lerp(currentHorizontalSpeed, targetSpeed, Time.deltaTime * acceleration) * 1000f) / 1000f
                : targetSpeed;
        }
        private Quaternion LerpRotation(float targetRotation) => Quaternion.Euler(0.0f, Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSpeed), 0.0f);
    }
}
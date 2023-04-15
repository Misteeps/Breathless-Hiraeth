using UnityEngine;

using Cinemachine;


namespace Game
{
    public class Player : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 2.0f;
        public float sprintSpeed = 5.335f;
        [Range(0.0f, 0.3f)]
        public float rotationSpeed = 0.12f;
        public float acceleration = 10.0f;
        [Space(4)]
        public float jumpHeight = 1.2f;
        public float gravity = -15.0f;
        [Space(4)]
        public float jumpCooldown = 0f;
        public float fallTime = 0.15f;

        [Header("Ground Check")]
        public bool grounded = true;
        public float groundOffset = -0.14f;
        public float groundRadius = 0.28f;
        public LayerMask groundLayers;

        [Header("References")]
        public Animator animator;
        public CharacterController controller;

        [Header("Debug")]
        public float speed;
        public float animationBlend;
        public float targetRotation = 0.0f;
        public float rotationVelocity;
        public float verticalVelocity;
        public float jumpCooldownTimer;
        public float fallTimer;

        private const float terminalVelocity = 53.0f;

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
            MoveHorizontal();

            if (transform.position.y < -20)
                Enable(true, Vector3.zero);
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
            animationBlend = 0;
            targetRotation = 0;
            rotationVelocity = 0;
            verticalVelocity = 0;
            jumpCooldownTimer = 0;
            fallTime = 0;

            this.enabled = enabled;
            animator.enabled = enabled;
            controller.enabled = enabled;
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
        private void MoveHorizontal()
        {
            float targetSpeed = Inputs.Dash.Held ? sprintSpeed : moveSpeed;
            Vector3 direction = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) direction += Vector3.forward;
            if (Input.GetKey(KeyCode.S)) direction += Vector3.back;
            if (Input.GetKey(KeyCode.A)) direction += Vector3.left;
            if (Input.GetKey(KeyCode.D)) direction += Vector3.right;
            if (direction == Vector3.zero) targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;
            float speedOffset = 0.1f;
            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed, Time.deltaTime * acceleration);
                speed = Mathf.Round(speed * 1000f) / 1000f;
            }
            else
                speed = targetSpeed;

            controller.Move(direction.normalized * (speed * Time.deltaTime) + new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);
            if (direction != Vector3.zero)
            {
                direction = direction.normalized;
                targetRotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0.0f, Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSpeed), 0.0f);
            }

            animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * acceleration);
            animator.SetFloat(animIDMoveY, animationBlend);
        }

        private void CheckGround()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundOffset, transform.position.z);
            grounded = Physics.CheckSphere(spherePosition, groundRadius, groundLayers, QueryTriggerInteraction.Ignore);
        }
    }
}
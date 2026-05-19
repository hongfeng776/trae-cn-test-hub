using UnityEngine;

namespace ForestMessenger.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("移动设置")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float acceleration = 15f;
        [SerializeField] private float deceleration = 20f;
        [SerializeField] private float velocityPower = 0.9f;

        [Header("跳跃设置")]
        [SerializeField] private float jumpForce = 16f;
        [SerializeField] private float fallMultiplier = 2.5f;
        [SerializeField] private float lowJumpMultiplier = 2f;
        [SerializeField] private float coyoteTime = 0.1f;
        [SerializeField] private float jumpBufferTime = 0.1f;

        [Header("地面检测")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayer;

        private Rigidbody2D rb;
        private CapsuleCollider2D col;
        private SpriteRenderer spriteRenderer;
        private Animator animator;

        private float horizontalInput;
        private bool isGrounded;
        private float coyoteTimeCounter;
        private float jumpBufferCounter;
        private bool isJumping;
        private bool jumpInputReleased;

        public bool IsGrounded => isGrounded;
        public float CurrentSpeed => Mathf.Abs(rb.velocity.x);
        public bool IsJumping => isJumping;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<CapsuleCollider2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            animator = GetComponentInChildren<Animator>();
        }

        private void Start()
        {
            if (groundCheck == null)
            {
                CreateGroundCheck();
            }
        }

        private void Update()
        {
            HandleInput();
            CheckGrounded();
            UpdateTimers();
            HandleJumpInput();
            UpdateAnimations();
        }

        private void FixedUpdate()
        {
            HandleMovement();
            ApplyGravity();
        }

        private void HandleInput()
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
        }

        private void CheckGrounded()
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            
            if (isGrounded && rb.velocity.y <= 0)
            {
                isJumping = false;
                jumpInputReleased = false;
            }
        }

        private void UpdateTimers()
        {
            if (isGrounded)
            {
                coyoteTimeCounter = coyoteTime;
            }
            else
            {
                coyoteTimeCounter -= Time.deltaTime;
            }

            jumpBufferCounter -= Time.deltaTime;
        }

        private void HandleJumpInput()
        {
            if (Input.GetButtonDown("Jump"))
            {
                jumpBufferCounter = jumpBufferTime;
            }

            if (Input.GetButtonUp("Jump"))
            {
                jumpInputReleased = true;
            }

            if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !isJumping)
            {
                Jump();
            }
        }

        private void Jump()
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isJumping = true;
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }

        private void HandleMovement()
        {
            float targetVelocity = horizontalInput * moveSpeed;
            float speedDiff = targetVelocity - rb.velocity.x;
            float accelRate = (Mathf.Abs(targetVelocity) > 0.01f) ? acceleration : deceleration;
            float movement = Mathf.Pow(Mathf.Abs(speedDiff) * accelRate, velocityPower) * Mathf.Sign(speedDiff);

            rb.AddForce(movement * Vector2.right);

            if (horizontalInput > 0.01f)
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
            }
            else if (horizontalInput < -0.01f)
            {
                transform.localScale = new Vector3(-1f, 1f, 1f);
            }
        }

        private void ApplyGravity()
        {
            if (rb.velocity.y < 0)
            {
                rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
            }
            else if (rb.velocity.y > 0 && jumpInputReleased)
            {
                rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
            }
        }

        private void UpdateAnimations()
        {
            if (animator != null)
            {
                animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
                animator.SetBool("IsGrounded", isGrounded);
                animator.SetFloat("VerticalVelocity", rb.velocity.y);
            }
        }

        private void CreateGroundCheck()
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0f, -col.bounds.extents.y, 0f);
            groundCheck = groundCheckObj.transform;
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }
    }
}

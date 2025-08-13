using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [Range(1f, 20f)] public float moveSpeed = 7f;
    float horizontalMovement;


    [Header("Deceleration")]
    [Range(1f, 100f)] public float deceleration = 45f;
    float currentSpeed = 0f;


    [Header("Jumping")]
    [Range(1f, 30f)] public float jumpForce = 11f;
    [Range(0.1f, 0.75f)] public float jumpCut = 0.3f;


    [Header("Wall Slide")]
    [SerializeField] Transform wallCheck;
    [SerializeField] LayerMask wallLayer;
    bool isWallSliding;
    public float wallSlidingSpeed = 2f;


    [Header("Wall Jump")]
    [SerializeField] Vector2 wallJumpingPower = new Vector2(7f, 11f);
    [SerializeField] float wallJumpingTime = 0.2f;
    float wallJumpingCounter;
    float wallJumpingDirection;
    bool isWallJumping;
    

    [Header("Gravity")]
    public float baseGravity = 2f;
    public float maxFallSpeed = 25f;
    public float fallHorizontalMovement = 3.5f;
    bool isGrounded;


    Rigidbody2D rb;
    BoxCollider2D feetCollider;


    #region Awake

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        feetCollider = GetComponent<BoxCollider2D>();
    }

    #endregion

    #region Update

    void Update()
    {
        GroundCheck();
        Gravity();
        WallSlide();
        WallJump();

        if (!isWallJumping)
        {
            FlipSprite();
        }
    }

    #endregion

    #region FixedUpdate

    void FixedUpdate()
    {

        float targetSpeed = horizontalMovement * moveSpeed;

        if (Mathf.Abs(horizontalMovement) > 0.01f)
        {
            currentSpeed = targetSpeed;
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.fixedDeltaTime);
        }

        if (isWallJumping && rb.velocity.y <= 0)
        {
            isWallJumping = false;
        }

        if (!isWallJumping)
        {
            rb.velocity = new Vector2(currentSpeed, rb.velocity.y);
        }
    }

    #endregion

    #region Move

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
    }

    #endregion

    #region FlipSprite

    void FlipSprite()
    {
        if (Mathf.Abs(horizontalMovement) > 0.01f)
        {
            transform.localScale = new Vector2(Mathf.Sign(horizontalMovement), 1f);
        }
    }

    #endregion

    #region Jump

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
        else if (context.canceled && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y * jumpCut);
        }

        if (context.performed && wallJumpingCounter > 0)
        {
            isWallJumping = true;
            rb.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0f;

            if (transform.localScale.x != wallJumpingDirection)
            {
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }
        }
    }

    #endregion

    #region Gravity

    void Gravity()
    {
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = baseGravity * fallHorizontalMovement;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
        }
        else
        {
            rb.gravityScale = baseGravity;
        }
    }

    #endregion 

    #region GroundCheck

    void GroundCheck()
    {
        isGrounded = feetCollider.IsTouchingLayers(LayerMask.GetMask("Ziemia"));

        if (isGrounded)
        {
            isWallJumping = false;
        }
    }

    private bool IsWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }

    void WallSlide()
    {
        if (IsWalled() && !isGrounded && horizontalMovement != 0)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }
    }

    #endregion
}
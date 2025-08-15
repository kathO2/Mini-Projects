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


    [Header("Acceleration & Deceleration")]
    [Range(1f, 100f)] public float acceleration = 60f;
    [Range(1f, 100f)] public float deceleration = 75f;
    float currentSpeed = 0f;


    [Header("Jumping")]
    [Range(1f, 30f)] public float jumpForce = 9f;
    [Range(0.1f, 0.75f)] public float jumpCut = 0.3f; 


    [Header("Coyote Time")]
    [SerializeField] float coyoteTime = 0.1f; 
    float coyoteTimeCounter; 


    [Header("Jump Buffer")]
    [SerializeField] float jumpBufferTime = 0.1f; 
    float jumpBufferCounter; 


    [Header("WallCheck")]
    public Transform wallCheckPos; 
    public Vector2 wallCheckSize = new Vector2(0.03f, 0.71f); 
    public LayerMask wallLayer; 


    [Header("WallMovement")]
    [Range(1f, 10f)] public float wallSlideSpeed = 2f; 
    bool isWallSliding; 
    

    [Header("WallJump")]
    [SerializeField] Vector2 wallJumpPower = new Vector2(6f, 8f); 
    bool isWallJumping; 
    float wallJumpDirection; 
    float wallJumpTime = 0.1f; 
    float wallJumpCounter; 


    [Header("Gravity")]
    public float baseGravity = 2f;
    public float maxFallSpeed = 25f;
    public float fallHorizontalSpeed = 3.5f; 
    bool isGrounded;

    [Header("Ghost")]
    [SerializeField] GameObject ghost;
    [SerializeField] Transform ghostSpawner;


    Rigidbody2D rb;
    BoxCollider2D feetCollider;


    #region Awake

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        feetCollider = GetComponent<BoxCollider2D>(); 
    }

    #endregion

    #region FixedUpdate

    void FixedUpdate()
    {
        float targetSpeed = horizontalMovement * moveSpeed;

        if (Mathf.Abs(horizontalMovement) > 0.01f)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
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

    #region Update

    void Update()
    {
        GroundCheck();
        Gravity();
        TryJump(); 
        WallSlide(); 
        WallJump(); 

        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (!isWallJumping)
        {
            FlipSprite();
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
        if (context.performed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else if (context.canceled && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y * jumpCut);
        }

        if (context.performed && wallJumpCounter > 0f)
        {
            isWallJumping = true;
            rb.velocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y);
            wallJumpCounter = 0f;
            jumpBufferCounter = 0f;

            if (transform.localScale.x != wallJumpDirection)
            {
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }
        }
    }

    void TryJump()
    {
        if ((isGrounded || coyoteTimeCounter > 0f) && jumpBufferCounter > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            coyoteTimeCounter = 0f;
            jumpBufferCounter = 0f;
        }
    }

    #endregion

    #region Gravity

    void Gravity()
    {
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = baseGravity * fallHorizontalSpeed;
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
        isGrounded = feetCollider.IsTouchingLayers(LayerMask.GetMask("Platform", "Wall"));

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            isWallJumping = false;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    #endregion

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(wallCheckPos.position, wallCheckSize);
    }

    #region WallCheck

    bool WallCheck()
    {
        return Physics2D.OverlapBox(wallCheckPos.position, wallCheckSize, 0, wallLayer);
    }

    #endregion
    
    #region WallSlide

    void WallSlide()
    {
        if (!isGrounded && WallCheck() && horizontalMovement != 0)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlideSpeed, float.MaxValue)); 
        }
        else
        {
            isWallSliding = false;
        }
    }

    #endregion

    #region WallJump

    void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpDirection = -transform.localScale.x;
            wallJumpCounter = wallJumpTime; 
        }
        else
        {
            wallJumpCounter -= Time.deltaTime; 
        }
    }

    #endregion

    public void Ghost(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Instantiate(ghost, ghostSpawner.position, transform.rotation);
            Debug.Log("GHOST");
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [Range(1f, 20f)] public float moveSpeed = 5f;
    float horizontalMovement;

    [Header("Jumping")]
    [Range(1f, 30f)] public float jumpForce = 5f;
    [Range(0.1f, 0.75f)] public float jumpCut = 0.3f;
    [SerializeField] int maxJumps = 2;
    int jumpsLeft;

    [Header("Gravity")]
    public float baseGravity = 2;
    public float maxFallSpeed = 18f;
    public float fallSpeedMultiplier = 2f;

    Rigidbody2D rb;
    BoxCollider2D feetCollider;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        feetCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        GroundCheck();
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector2(horizontalMovement * moveSpeed, rb.velocity.y);
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext context)
    {
            if (context.performed && jumpsLeft > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpsLeft--;
            }
            else if (context.canceled && rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(0f, rb.velocity.y * jumpCut);
            }     
    }

    void GroundCheck()
    {
        if (feetCollider.IsTouchingLayers(LayerMask.GetMask("Ziemia")))
        {
            jumpsLeft = maxJumps;           
        }  
    }
}

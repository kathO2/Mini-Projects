using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayersMovement : MonoBehaviour
{
    [Range(1f, 20f)] public float moveSpeed = 10f;
    Rigidbody2D rb;
    public bool isPlayer1;

    private float movement;

    public Vector3 startPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
    }

    void FixedUpdate()
    {
        if (isPlayer1)
        {
            movement = Input.GetAxisRaw("Vertical1");
        }
        else
        {
            movement = Input.GetAxisRaw("Vertical2");
        }

        rb.velocity = new Vector2(rb.velocity.x, movement * moveSpeed);
    }

    public void Reset()
    {
        rb.velocity = Vector2.zero;
        transform.position = startPosition;
    }
}

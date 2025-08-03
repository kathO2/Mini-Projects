using UnityEngine;

public class MarioMovement : MonoBehaviour
{
    private Rigidbody2D rb;

    Vector2 velocity;
    float inputAxis;

    public float moveSpeed = 8f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        HorizontalMovement();
    }

    void HorizontalMovement()
    {
        inputAxis = Input.GetAxis("Horizontal");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeetDetector : MonoBehaviour
{
    Rigidbody2D rb;
    [SerializeField] float bounceForce = 20f;

    void Awake()
    {
        rb = GetComponentInParent<Rigidbody2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Bounce"))
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);
        }
    }
}

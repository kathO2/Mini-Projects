using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    [SerializeField] float ghostSpeed = 20f;

    Rigidbody2D ghostRb;
    PlayerMovement player;

    float xSpeed;

    void Awake()
    {
        ghostRb = GetComponent<Rigidbody2D>();
        player = FindObjectOfType<PlayerMovement>();

        xSpeed = player.transform.localScale.x * ghostSpeed;
    }

    void FixedUpdate()
    {
        ghostRb.velocity = new Vector2(xSpeed, 0f);
    }
}

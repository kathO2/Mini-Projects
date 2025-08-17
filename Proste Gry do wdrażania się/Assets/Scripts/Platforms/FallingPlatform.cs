using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    public float fallWait = 2f;
    public float destroyWait = 1f;

    bool isFalling;
    Rigidbody2D rb;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isFalling && collision.gameObject.tag == "Player")
        {
            StartCoroutine(Fall());
        }
    }

    IEnumerator Fall()
    {
        isFalling = true;
        yield return new WaitForSeconds(fallWait);
        rb.bodyType = RigidbodyType2D.Dynamic;
        Destroy(gameObject, destroyWait);
    }
}

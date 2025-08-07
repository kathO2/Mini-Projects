using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [Range(1f, 20f)] public float ballSpeed = 5f;
    Rigidbody2D rb;
    public Vector3 startPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        Launch();
    }

    void Launch()
    {
        // Losuje liczbę całkowitą 0 lub 1.
        // Jeśli wylosowano 0, 'x' będzie równe -1 (ruch w lewo),
        // w przeciwnym wypadku (czyli 1) 'x' będzie równe 1 (ruch w prawo).
        float x = Random.Range(0, 2) == 0 ? -1 : 1; 

        // Losuje liczbę całkowitą 0 lub 1.
        // Jeśli wylosowano 0, 'y' będzie równe -1 (ruch w dół),
        // w przeciwnym wypadku (czyli 1) 'y' będzie równe 1 (ruch w górę).
        float y = Random.Range(0, 2) == 0 ? -1 : 1; 

        // Ustawia prędkość (velocity) komponentu Rigidbody2D.
        // Tworzy nowy wektor, który określa kierunek i siłę ruchu.
        // x * ballSpeed: prędkość pozioma (w prawo lub w lewo).
        // ballSpeed * y: prędkość pionowa (w górę lub w dół).
        rb.velocity = new Vector2(x * ballSpeed, ballSpeed * y);
    }

    public void Reset()
    {
        rb.velocity = Vector2.zero;
        transform.position = startPosition;
        Launch();
    }
}
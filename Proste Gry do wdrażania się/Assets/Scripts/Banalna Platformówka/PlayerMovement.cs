using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    // === RUCH W POZIOMIE ===
    [Header("Movement")]
    [Range(1f, 20f)] public float moveSpeed = 5f;  // Prędkość poruszania się w lewo/prawo
    float horizontalMovement;                      // Wartość z wejścia gracza (od -1 do 1)

    // === SKAKANIE ===
    [Header("Jumping")]
    [Range(1f, 30f)] public float jumpForce = 5f;   // Siła skoku
    [Range(0.1f, 0.75f)] public float jumpCut = 0.3f; // Skrócenie skoku, jeśli przycisk zostanie puszczony wcześniej
    [SerializeField] int maxJumps = 2;              // Ilość dostępnych skoków zanim dotkniesz ziemi (np. 1 = podwójny skok)
    int jumpsLeft;                                  // Liczba pozostałych skoków

    // === GRAWITACJA ===
    [Header("Gravity")]
    public float baseGravity = 2f;                  // Podstawowa grawitacja
    public float maxFallSpeed = 18f;                // Maksymalna prędkość spadania
    public float fallSpeedMultiplier = 2f;          // Mnożnik grawitacji, gdy postać spada (daje bardziej dynamiczne odczucia)

    Rigidbody2D rb;                                 // Referencja do fizyki 2D
    CapsuleCollider2D feetCollider;                 // Collider do sprawdzania, czy postać dotyka ziemi

    void Awake()
    {
        // Pobranie komponentów na starcie
        rb = GetComponent<Rigidbody2D>();
        feetCollider = GetComponent<CapsuleCollider2D>();
    }

    void Update()
    {
        GroundCheck();   // Sprawdzenie, czy jesteśmy na ziemi
        Gravity();       // Zastosowanie zmiennej grawitacji
    }

    // === Dynamiczna GRAWITACJA ===
    void Gravity()
    {
        if (rb.velocity.y < 0)
        {
            // Postać spada — zwiększamy grawitację dla lepszego feelingu
            rb.gravityScale = baseGravity * fallSpeedMultiplier;

            // Ograniczamy maksymalną prędkość spadania
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
        }
        else
        {
            // Gdy postać leci w górę — używamy zwykłej grawitacji
            rb.gravityScale = baseGravity;
        }
    }

    void FixedUpdate()
    {
        // Ruch poziomy — aktualizowany w stałym czasie fizycznym
        rb.velocity = new Vector2(horizontalMovement * moveSpeed, rb.velocity.y);
    }

    // === ODCZYT RUCHU POZIOMEGO Z WEJŚCIA ===
    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x; // Odczyt kierunku poziomego z wejścia gracza
    }

    // === SKOK ===
    public void Jump(InputAction.CallbackContext context)
    {
        // Gdy przycisk został naciśnięty i mamy dostępne skoki
        if (context.performed && jumpsLeft > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce); // Nadajemy prędkość w górę
            jumpsLeft--; // Odejmujemy 1 skok
        }
        // Gdy gracz puścił przycisk skoku w trakcie lotu w górę
        else if (context.canceled && rb.velocity.y > 0)
        {
            // Przycinamy skok — zmniejszamy siłę, by nie polecieć tak wysoko
            rb.velocity = new Vector2(0f, rb.velocity.y * jumpCut);
        }
    }

    // === SPRAWDZENIE, CZY POSTAĆ STOI NA ZIEMI ===
    void GroundCheck()
    {
        if (feetCollider.IsTouchingLayers(LayerMask.GetMask("Ziemia")))
        {
            // Jeśli dotykamy warstwy "Ziemia" — resetujemy ilość skoków
            jumpsLeft = maxJumps;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [Range(1f, 20f)] public float moveSpeed = 5f;     // Prędkość ruchu w poziomie
    float horizontalMovement;                         // Wartość wejścia poziomego (od -1 do 1)

    [Header("Jumping")]
    [Range(1f, 30f)] public float jumpForce = 5f;      // Siła skoku
    [Range(0.1f, 0.75f)] public float jumpCut = 0.3f;  // Skraca skok, jeśli przycisk zostanie puszczony
    [SerializeField] int maxJumps = 2;                 // Całkowita liczba skoków (1 = podwójny skok)
    [SerializeField] int maxExtraJumps = 1;            // Liczba dodatkowych skoków po pierwszym
    int jumpsLeft;                                     // Pozostałe dodatkowe skoki (nie licząc pierwszego)

    [Header("Coyote Time")]
    [SerializeField] float coyoteTime = 0.2f;          // Czas na skok po zejściu z krawędzi
    float coyoteTimeCounter;                           // Timer odliczający pozostały coyote time

    [Header("Gravity")]
    public float baseGravity = 2f;                     // Normalna grawitacja
    public float maxFallSpeed = 18f;                   // Maksymalna prędkość spadania
    public float fallSpeedMultiplier = 2f;             // Zwiększa grawitację podczas spadania

    bool isGrounded;                                   // Czy postać dotyka ziemi

    Rigidbody2D rb;                                    // Komponent fizyczny
    CapsuleCollider2D feetCollider;                    // Collider do sprawdzania kontaktu z ziemią

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        feetCollider = GetComponent<CapsuleCollider2D>();
    }

    void Update()
    {
        GroundCheck();   // Sprawdza, czy jesteśmy na ziemi
        Gravity();       // Dostosowuje grawitację w zależności od kierunku ruchu
    }

    #region Grawitacja

    void Gravity()
    {
        if (rb.velocity.y < 0)
        {
            // Spadanie — zwiększamy grawitację i ograniczamy prędkość
            rb.gravityScale = baseGravity * fallSpeedMultiplier;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
        }
        else
        {
            // Wznoszenie — standardowa grawitacja
            rb.gravityScale = baseGravity;
        }
    }

    #endregion

    void FixedUpdate()
    {
        // Poruszanie się w poziomie — fizyka aktualizowana w stałych odstępach
        rb.velocity = new Vector2(horizontalMovement * moveSpeed, rb.velocity.y);
    }

    public void Move(InputAction.CallbackContext context)
    {
        // Odczyt wartości z kontrolera / klawiatury
        horizontalMovement = context.ReadValue<Vector2>().x;
    }

    #region Skok

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Skok ze stanu "na ziemi" lub w czasie coyoteTime
            if (isGrounded || coyoteTimeCounter > 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                coyoteTimeCounter = 0f;

                // Resetujemy dodatkowe skoki po pierwszym
                jumpsLeft = maxExtraJumps;
            }
            // Dodatkowe skoki w powietrzu
            else if (jumpsLeft > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpsLeft--;
            }
        }
        // Skracanie skoku, gdy gracz puszcza przycisk w trakcie wznoszenia
        else if (context.canceled && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y * jumpCut);
        }
    }

    #endregion

    #region GroundCheck

    void GroundCheck()
    {
        // Sprawdzamy kontakt z warstwą "Ziemia"
        isGrounded = feetCollider.IsTouchingLayers(LayerMask.GetMask("Ziemia"));

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;   // Resetujemy timer coyoteTime
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;  // Odliczanie czasu od zejścia z platformy
        }
    }

    #endregion
}

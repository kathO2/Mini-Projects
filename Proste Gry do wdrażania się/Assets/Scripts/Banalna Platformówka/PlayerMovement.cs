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

    [Header("Jump Buffer")]
    [SerializeField] float jumpBufferTime = 0.2f;      // Czas jump buffer
    float jumpBufferCounter;                           // Timer odliczający pozostały jump buffer

    [Header("Gravity")]
    public float baseGravity = 2f;                     // Normalna grawitacja
    public float maxFallSpeed = 18f;                   // Maksymalna prędkość spadania
    public float fallSpeedMultiplier = 2f;             // Zwiększa grawitację podczas spadania

    bool isGrounded;                                   // Czy postać dotyka ziemi

    Rigidbody2D rb;                                    // Komponent fizyczny
    BoxCollider2D feetCollider;                        // Collider do sprawdzania kontaktu z ziemią

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        feetCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        GroundCheck();   // Sprawdza, czy jesteśmy na ziemi
        Gravity();       // Dostosowuje grawitację w zależności od kierunku ruchu

        // Jeśli gracz nacisnął przycisk skoku niedawno, zmniejszamy licznik jump buffera
        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        TryJump(); // Sprawdzamy czy można teraz wykonać skok (na podstawie coyoteTime i jumpBuffer)
    }

    #region Grawitacja

    void Gravity()
    {
        if (rb.velocity.y < 0)
        {
            // Spadanie — zwiększamy grawitację i ograniczamy prędkość spadania
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
        // Ta metoda jest wywoływana przez Input System przy naciśnięciu lub puszczeniu przycisku skoku

        if (context.performed)
        {
            // Gracz nacisnął przycisk skoku — NIE SKACZEMY od razu!
            // Zamiast tego ustawiamy licznik jumpBufferCounter — oznacza to "chcę skoczyć"
            jumpBufferCounter = jumpBufferTime;
        }
        // Gracz puścił przycisk skoku — skracamy skok (jump cut), jeśli postać się jeszcze wznosi
        else if (context.canceled && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y * jumpCut);
        }
    }

    void TryJump()
    {
        // Metoda wywoływana w każdej klatce (w Update) — sprawdza, czy można wykonać skok
        // Tu dopiero faktycznie wykonywany jest skok

        // Jeżeli gracz jest na ziemi LUB jesteśmy jeszcze w "coyote time"
        // oraz gracz wcześniej nacisnął skok (buffer wciąż aktywny)
        if ((isGrounded || coyoteTimeCounter > 0f) && jumpBufferCounter > 0f)
        {
            // WYKONUJEMY SKOK!
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);

            // Resetujemy buffery i liczniki
            coyoteTimeCounter = 0f;
            jumpBufferCounter = 0f;

            // Resetujemy ilość dodatkowych skoków
            jumpsLeft = maxExtraJumps;
        }
        // Jeżeli jesteśmy w powietrzu, ale mamy jeszcze dodatkowe skoki i aktywny jump buffer
        else if (jumpBufferCounter > 0f && jumpsLeft > 0)
        {
            // WYKONUJEMY DODATKOWY SKOK
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpsLeft--;
            jumpBufferCounter = 0f;
        }

        // Jeśli warunki nie są spełnione, nie dzieje się nic — jumpBufferCounter stopniowo wygasa
    }

    #endregion

    #region GroundCheck

    void GroundCheck()
    {
        // Sprawdzamy kontakt z warstwą "Ziemia"
        isGrounded = feetCollider.IsTouchingLayers(LayerMask.GetMask("Ziemia"));

        if (isGrounded)
        {
            // Jesteśmy na ziemi — resetujemy licznik coyoteTime
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            // Nie jesteśmy na ziemi — odliczamy pozostały czas coyoteTime
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    #endregion
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BanalnaPlatformowkaMovement : MonoBehaviour
{
    [Header("Movement")]
    [Range(1f, 20f)] public float moveSpeed = 5f; // Prędkość ruchu w poziomie
    float horizontalMovement; // Wartość wejścia poziomego (od -1 do 1)


    [Header("Acceleration/Deceleration")]
    [Range(1f, 100f)] public float acceleration = 20f;
    [Range(1f, 100f)] public float deceleration = 25f;
    float currentSpeed = 0f;


    [Header("Jumping")]
    [Range(1f, 30f)] public float jumpForce = 5f; // Siła skoku
    [Range(0.1f, 0.75f)] public float jumpCut = 0.3f; // Skraca skok, jeśli przycisk zostanie puszczony
    [SerializeField] int maxJumps = 2; // Całkowita liczba skoków (1 = pojedynczy skok, 2 = podwójny skok)
    [SerializeField] int maxExtraJumps = 1; // Liczba dodatkowych skoków po pierwszym
    int jumpsLeft; // Liczba pozostałych dodatkowych skoków


    [Header("Coyote Time")]
    [SerializeField] float coyoteTime = 0.2f; // Czas, w którym można jeszcze skoczyć po zejściu z platformy
    float coyoteTimeCounter; // Licznik czasu coyote time


    [Header("Jump Buffer")]
    [SerializeField] float jumpBufferTime = 0.2f; // Okno czasowe, w którym skok zostanie zapamiętany
    float jumpBufferCounter; // Licznik buffera skoku


    [Header("WallCheck")]
    public Transform wallCheckPos; // Punkt referencyjny do sprawdzania kolizji ze ścianą
    public Vector2 wallCheckSize = new Vector2(0.49f, 0.03f); // Rozmiar boxa do detekcji ściany
    public LayerMask wallLayer; // Warstwa odpowiadająca ścianom


    [Header("WallMovement")]
    [Range(1f, 10f)] public float wallSlideSpeed = 2f; // Prędkość opadania podczas ślizgu po ścianie
    bool isWallSliding; // Flaga informująca o tym, czy postać ślizga się po ścianie


    // Wall Jumping
    bool isWallJumping; // Flaga informująca o tym, czy trwa wall jump
    float wallJumpDirection; // Kierunek odbicia od ściany
    float wallJumpTime = 0.5f; // Czas na wykonanie wall jumpa po przyklejeniu do ściany
    float wallJumpTimer; // Licznik wall jumpa
    [SerializeField] Vector2 wallJumpPower = new Vector2(5f, 5f); // Siła wybicia podczas wall jumpa


    [Header("Gravity")]
    public float baseGravity = 2f; // Bazowa grawitacja
    public float maxFallSpeed = 18f; // Maksymalna prędkość opadania
    public float fallhorizontalMovement = 2f; // Wzmocnienie grawitacji przy spadaniu
    bool isGrounded; // Czy postać stoi na ziemi


    Rigidbody2D rb; // Komponent fizyczny
    BoxCollider2D feetCollider; // Collider służący do sprawdzania kontaktu z podłożem


    #region Awake

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); // Przypisanie Rigidbody2D
        feetCollider = GetComponent<BoxCollider2D>(); // Przypisanie BoxCollider2D
    }

    #endregion

    #region Update

    void Update()
    {
        GroundCheck(); // Sprawdzenie czy stoimy na ziemi
        Gravity(); // Zastosowanie odpowiedniej siły grawitacji
        TryJump(); // Próbujemy wykonać skok
        FlipSprite(); // Odwracamy postać w zależności od kierunku ruchu
        WallSlide(); // Obsługa ślizgu po ścianie
        WallJump(); // Obsługa wall jumpa

        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime; // Zmniejszamy licznik jump buffera
        }
    }

    #endregion

    #region FixedUpdate

    void FixedUpdate()
    {
        if (isWallJumping)
        {
            return;     // jeżeli postać skacze ze ściany - nie nadpisuj jej prędkości, prędkością poruszania się postaci po ziemii
        }

        float targetSpeed = horizontalMovement * moveSpeed;

        if (Mathf.Abs(horizontalMovement) > 0.01f)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime); // przyspieszanie(obecna prędkość, maksymalna prędkość, osiągamy tą maksymalną prędkość w określonym tempie)
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.fixedDeltaTime); // hamowanie (obecna prędkość, zatrzymanie się całkowite, tempo z jakim schodzimy do zatrzymania się)
        }

        rb.velocity = new Vector2(currentSpeed, rb.velocity.y);
    }

    #endregion

    #region Move

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
    }

    #endregion

    #region FlipSprite

    void FlipSprite()
    {
        // Obraca sprite'a zgodnie z kierunkiem ruchu
        if (Mathf.Abs(horizontalMovement) > 0.01f)
        {
            transform.localScale = new Vector2(Mathf.Sign(horizontalMovement), 1f);
        }
    }

    #endregion

    #region Jump

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpBufferCounter = jumpBufferTime; // Zapisujemy intencję skoku (jump buffer)
        }
        else if (context.canceled && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y * jumpCut); // Skrócenie skoku, jeśli gracz puści przycisk
        }

        // Wykonanie wall jumpa, jeśli jesteśmy w czasie pozwalającym na to
        if (context.performed && wallJumpTimer > 0f)
        {
            isWallJumping = true;
            rb.velocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y);
            wallJumpTimer = 0;

            Invoke(nameof(CancelWallJump), wallJumpTime + 0.1f); // Dezaktywacja wall jumpa po czasie
        }
    }

    void TryJump()
    {
        // Skok z ziemi lub przy użyciu coyote time
        if ((isGrounded || coyoteTimeCounter > 0f) && jumpBufferCounter > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            coyoteTimeCounter = 0f;
            jumpBufferCounter = 0f;
            jumpsLeft = maxExtraJumps; // Resetujemy możliwość podwójnego skoku
        }
        // Skok w powietrzu, jeśli mamy jeszcze dodatkowe skoki
        else if (jumpBufferCounter > 0f && jumpsLeft > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpsLeft--; // Zużywamy jeden skok
            jumpBufferCounter = 0f;
        }
    }

    #endregion

    #region Gravity

    void Gravity()
    {
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = baseGravity * fallhorizontalMovement; // Zwiększamy grawitację przy opadaniu
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed)); // Ograniczamy maksymalną prędkość spadania
        }
        else
        {
            rb.gravityScale = baseGravity; // Przy normalnym skoku stosujemy bazową grawitację
        }
    }

    #endregion 

    #region GroundCheck

    void GroundCheck()
    {
        // Sprawdzamy, czy dolny collider dotyka warstwy "Ziemia"
        isGrounded = feetCollider.IsTouchingLayers(LayerMask.GetMask("Ziemia"));

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime; // Resetujemy coyote time
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime; // Odliczamy czas coyote time
        }
    }

    #endregion

    void OnDrawGizmosSelected()
    {
        // Pokazujemy w edytorze obszar detekcji ściany
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(wallCheckPos.position, wallCheckSize);
    }

    #region WallCheck

    bool WallCheck()
    {
        // Sprawdzamy, czy postać dotyka ściany
        return Physics2D.OverlapBox(wallCheckPos.position, wallCheckSize, 0, wallLayer);
    }

    #endregion
    
    #region WallSlide

    void WallSlide()
    {
        // Sprawdzamy warunki do ślizgu po ścianie: nie stoimy na ziemi, dotykamy ściany, poruszamy się w kierunku ściany
        if (!isGrounded && WallCheck() && horizontalMovement == Mathf.Sign(transform.localScale.x))
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -wallSlideSpeed)); // Ograniczamy prędkość spadania

            jumpsLeft = maxExtraJumps; // Resetujemy możliwość skoku w powietrzu
        }
        else
        {
            isWallSliding = false;
        }
    }

    #endregion


    #region WallJump

    void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpDirection = -transform.localScale.x; // Ustawiamy kierunek odbicia przeciwny do ściany
            wallJumpTimer = wallJumpTime; // Uruchamiamy timer na możliwość wall jumpa

            CancelInvoke(nameof(CancelWallJump)); // Anulujemy poprzednie wywołanie dezaktywacji wall jumpa
        }
        else if (wallJumpTimer > 0f)
        {
            wallJumpTimer -= Time.deltaTime; // Odliczamy czas do wykonania wall jumpa
        }
    }

    void CancelWallJump()
    {
        isWallJumping = false; // Kończymy wall jump
    }

    #endregion
}
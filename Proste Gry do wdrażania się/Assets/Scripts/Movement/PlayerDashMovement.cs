using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDashMovement : MonoBehaviour
{
    [Header("Movement")]
    [Range(1f, 20f)] public float moveSpeed = 7f; // Prędkość poruszania się postaci.
    float horizontalMovement; // Przechowuje dane wejścia poziomego (z klawiszy A/D lub gałki).


    [Header("Acceleration & Deceleration")]
    [Range(1f, 100f)] public float acceleration = 60f; // Szybkość przyspieszania.
    [Range(1f, 100f)] public float deceleration = 75f; // Szybkość zwalniania.
    float currentSpeed = 0f; // Aktualna prędkość pozioma postaci.


    [Header("Jumping")]
    [Range(1f, 30f)] public float jumpForce = 9f; // Siła skoku.
    [Range(0.1f, 0.75f)] public float jumpCut = 0.3f; // Redukcja siły skoku po puszczeniu przycisku.


    [Header("Coyote Time")]
    [SerializeField] float coyoteTime = 0.1f; // Czas na wykonanie skoku po zejściu z platformy.
    float coyoteTimeCounter; // Licznik czasu coyote.


    [Header("Jump Buffer")]
    [SerializeField] float jumpBufferTime = 0.1f; // Czas, w którym naciśnięcie skoku jest buforowane przed zetknięciem z ziemią.
    float jumpBufferCounter; // Licznik buforu skoku.


    [Header("WallCheck")]
    public Transform wallCheckPos; // Obiekt, z którego sprawdzane jest położenie ściany.
    public Vector2 wallCheckSize = new Vector2(0.03f, 0.71f); // Rozmiar "pudełka" sprawdzającego ścianę.
    public LayerMask wallLayer; // Warstwa, na której znajdują się ściany.


    [Header("WallMovement")]
    [Range(1f, 10f)] public float wallSlideSpeed = 2f; // Prędkość ślizgania się po ścianie.
    bool isWallSliding; // Flaga, czy postać się ślizga po ścianie.


    [Header("WallJump")]
    [SerializeField] Vector2 wallJumpPower = new Vector2(6f, 8f); // Siła i kierunek skoku od ściany.
    bool isWallJumping; // Flaga, czy postać wykonuje skok od ściany.
    float wallJumpDirection; // Kierunek skoku od ściany.
    float wallJumpTime = 0.1f; // Krótki czas opóźnienia, aby skok od ściany był bardziej przewidywalny.
    float wallJumpCounter; // Licznik skoku od ściany.


    [Header("Gravity")]
    public float baseGravity = 2f; // Podstawowa siła grawitacji.
    public float maxFallSpeed = 25f; // Maksymalna prędkość spadania.
    public float fallHorizontalSpeed = 3.5f; // Modyfikator grawitacji podczas spadania.
    private bool isGrounded; // Flaga, czy postać jest na ziemi.

    [Header("Dash")]
    [SerializeField] float dashingSpeed = 25f; // Prędkość dasha.
    [SerializeField] float dashingTime = 0.2f; // Czas trwania dasha.
    Vector2 dashingDir; // Kierunek dasha.
    bool isDashing; // Flaga, czy postać wykonuje dasha.
    bool canDash = true; // Flaga, czy postać może wykonać dasha.
    float originalGravity; // Oryginalna wartość grawitacji, do przywrócenia po dasha.
    bool justDashed; // Flaga używana do resetowania dasha po dotknięciu ziemi.
    int playerLayer; // Numer warstwy "Player".
    int dashLayer; // Numer warstwy "Dash".
    int dashCheckLayer; // Numer warstwy "DashCheck" (używana do detekcji utknięcia).

    [Header("Dash Trigger")]
    [SerializeField] CapsuleCollider2D dashTrigger; // Referencja do kolidera-triggera do sprawdzania utknięcia.


    // Referencje do komponentów.
    Rigidbody2D rb;
    CapsuleCollider2D bodyCollider;
    BoxCollider2D feetCollider;

    #region Awake

    // Wywoływana raz, w momencie inicjalizacji obiektu.
    void Awake()
    {
        // Pobieranie komponentów.
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<CapsuleCollider2D>();
        feetCollider = GetComponent<BoxCollider2D>();
        
        // Zapisywanie oryginalnej wartości grawitacji.
        originalGravity = rb.gravityScale;

        // Pobieranie numerów warstw po nazwie.
        playerLayer = gameObject.layer;
        dashLayer = LayerMask.NameToLayer("Dash");
        dashCheckLayer = LayerMask.NameToLayer("DashCheck");
    }

    #endregion

    #region FixedUpdate

    // Wywoływana z ustalonym interwałem czasowym, idealna do operacji fizyki.
    void FixedUpdate()
    {
        // Jeśli postać wykonuje dasha, ustawiamy jej prędkość i wychodzimy z metody.
        if (isDashing)
        {
            rb.velocity = dashingDir * dashingSpeed;
            return;
        }

        // Obliczanie docelowej prędkości.
        float targetSpeed = horizontalMovement * moveSpeed;

        // Płynne przyspieszanie.
        if (Mathf.Abs(horizontalMovement) > 0.01f)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
        }
        // Płynne zwalnianie.
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.fixedDeltaTime);
        }

        // Resetowanie skoku od ściany, jeśli postać spada.
        if (isWallJumping && rb.velocity.y <= 0)
        {
            isWallJumping = false;
        }

        // Ustawianie prędkości poziomej, jeśli postać nie skacze od ściany.
        if (!isWallJumping)
        {
            rb.velocity = new Vector2(currentSpeed, rb.velocity.y);
        }
    }

    #endregion

    #region Update

    // Wywoływana raz na klatkę.
    void Update()
    {
        // Wywołanie metod odpowiedzialnych za fizykę i stan gracza.
        GroundCheck();
        Gravity();
        TryJump();
        WallSlide();
        WallJump();

        // Odliczanie buforu skoku.
        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // Obracanie sprite'a, jeśli postać nie skacze od ściany.
        if (!isWallJumping)
        {
            FlipSprite();
        }

        // Resetowanie dasha, gdy postać jest na ziemi i nie jest w trakcie dasha.
        if (isGrounded && !justDashed)
        {
            canDash = true;
        }

        // Ustawienie flagi `justDashed` na false, gdy postać jest w powietrzu.
        if (!isGrounded)
        {
            justDashed = false;
        }
    }

    #endregion

    #region Move

    // Metoda wywoływana przez system wejścia Unity (Input System).
    public void Move(InputAction.CallbackContext context)
    {
        // Odczytanie wartości poziomego ruchu.
        horizontalMovement = context.ReadValue<Vector2>().x;
    }

    #endregion

    #region FlipSprite

    // Metoda obracająca sprite w zależności od kierunku ruchu.
    void FlipSprite()
    {
        // Sprawdzenie, czy postać się porusza.
        if (Mathf.Abs(horizontalMovement) > 0.01f)
        {
            // Zmiana skali X na -1 lub 1, aby odwrócić sprite.
            transform.localScale = new Vector2(Mathf.Sign(horizontalMovement), 1f);
        }
    }

    #endregion

    #region Jump

    // Metoda wywoływana przez system wejścia dla skoku.
    public void Jump(InputAction.CallbackContext context)
    {
        // Jeśli przycisk skoku został naciśnięty, aktywuj bufor skoku.
        if (context.performed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        // Jeśli przycisk został puszczony, a postać wciąż skacze do góry, skróć skok.
        else if (context.canceled && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y * jumpCut);
        }

        // Logika skoku od ściany.
        if (context.performed && wallJumpCounter > 0f)
        {
            isWallJumping = true; // Flaga, że postać wykonuje skok od ściany.
            rb.velocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y); // Aplikowanie siły skoku.
            wallJumpCounter = 0f; // Reset licznika skoku od ściany.
            jumpBufferCounter = 0f; // Reset bufora skoku.

            // Odwrócenie sprite'a w kierunku przeciwnym do ściany.
            if (transform.localScale.x != wallJumpDirection)
            {
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }
        }
    }

    // Metoda odpowiedzialna za faktyczne wykonanie skoku.
    void TryJump()
    {
        // Sprawdzenie, czy postać jest na ziemi (lub w czasie coyote) i czy bufor skoku jest aktywny.
        if ((isGrounded || coyoteTimeCounter > 0f) && jumpBufferCounter > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce); // Aplikowanie siły skoku.
            coyoteTimeCounter = 0f; // Reset czasu coyote.
            jumpBufferCounter = 0f; // Reset bufora skoku.
        }
    }

    #endregion

    #region Gravity

    // Metoda dostosowująca grawitację w zależności od prędkości pionowej.
    void Gravity()
    {
        // Zwiększenie grawitacji podczas spadania.
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = baseGravity * fallHorizontalSpeed;
            // Ograniczenie maksymalnej prędkości spadania.
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
        }
        // Przywrócenie podstawowej grawitacji w innych przypadkach.
        else
        {
            rb.gravityScale = baseGravity;
        }
    }

    #endregion

    #region GroundCheck

    // Metoda sprawdzająca, czy postać dotyka ziemi.
    void GroundCheck()
    {
        // Sprawdzenie, czy kolider stóp dotyka warstw "Platform" lub "Wall".
        isGrounded = feetCollider.IsTouchingLayers(LayerMask.GetMask("Platform", "Wall"));

        // Jeśli postać jest na ziemi, resetuj czas coyote.
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            isWallJumping = false; // Zapobieganie problemom z animacjami.
        }
        // Jeśli nie jest na ziemi, odliczaj czas coyote.
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    #endregion

    // Metoda do wizualizacji "pudełka" sprawdzającego ścianę w edytorze Unity.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(wallCheckPos.position, wallCheckSize);
    }

    #region WallCheck

    // Metoda sprawdzająca, czy w pobliżu znajduje się ściana.
    bool WallCheck()
    {
        return Physics2D.OverlapBox(wallCheckPos.position, wallCheckSize, 0, wallLayer);
    }

    #endregion

    #region WallSlide

    // Metoda do obsługi ślizgania się po ścianie.
    void WallSlide()
    {
        // Sprawdzenie warunków: nie jest na ziemi, dotyka ściany i porusza się poziomo.
        if (!isGrounded && WallCheck() && horizontalMovement != 0)
        {
            isWallSliding = true; // Ustaw flagę ślizgania.
            // Ograniczenie prędkości spadania do wartości wallSlideSpeed.
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlideSpeed, float.MaxValue));
        }
        // Jeśli warunki nie są spełnione, wyłącz ślizganie.
        else
        {
            isWallSliding = false;
        }
    }

    #endregion

    #region WallJump

    // Metoda do obsługi skoku od ściany.
    void WallJump()
    {
        // Jeśli postać ślizga się po ścianie, przygotuj się do skoku.
        if (isWallSliding)
        {
            isWallJumping = false; // Flaga, że skok od ściany może być aktywowany.
            wallJumpDirection = -transform.localScale.x; // Ustaw kierunek skoku.
            wallJumpCounter = wallJumpTime; // Ustaw czas opóźnienia skoku.
        }
        // W przeciwnym razie, odliczaj czas.
        else
        {
            wallJumpCounter -= Time.deltaTime;
        }
    }

    #endregion

    #region Dash

    // Metoda wywoływana przez system wejścia dla dasha.
    public void Dash(InputAction.CallbackContext context)
    {
        // Sprawdzenie, czy przycisk został naciśnięty i czy można wykonać dasha.
        if (context.performed && canDash)
        {
            isDashing = true; // Rozpocznij dasha.
            justDashed = true; // Flaga do kontroli resetowania dasha.

            // Zmień warstwę bodyCollider, aby postać mogła przenikać przez ściany.
            bodyCollider.gameObject.layer = dashLayer;

            // Odczytaj kierunek ruchu z wejścia gracza.
            Vector2 inputDirection = GetComponent<PlayerInput>().actions["Move"].ReadValue<Vector2>();

            // Ustaw kierunek dasha na podstawie wejścia lub kierunku, w którym patrzy postać.
            if (inputDirection == Vector2.zero)
            {
                dashingDir = new Vector2(transform.localScale.x, 0f);
            }
            else
            {
                dashingDir = inputDirection.normalized;
            }

            // Uniemożliwienie ponownego dasha, jeśli wykonano go w powietrzu lub w pionie.
            if (Mathf.Abs(dashingDir.y) > 0.01f || !isGrounded)
            {
                canDash = false;
            }

            rb.velocity = Vector2.zero; // Wyzerowanie prędkości przed dashem.
            rb.gravityScale = 0f; // Wyłączenie grawitacji.
            StartCoroutine(StopDashing()); // Uruchomienie korutyny do zakończenia dasha.
        }
    }

    #endregion

    #region StopDashing

    // Korutyna do zarządzania zakończeniem dasha.
    IEnumerator StopDashing()
    {
        float startTime = Time.time;

        // Pętla czeka, aż upłynie czas dasha LUB aż postać wyjdzie ze ściany.
        while (Time.time < startTime + dashingTime || dashTrigger.IsTouchingLayers(LayerMask.GetMask("Wall")))
        {
            yield return null; // Czekaj na kolejną klatkę.
        }

        isDashing = false; // Zakończ dasha.
        rb.gravityScale = originalGravity; // Przywróć oryginalną grawitację.
        rb.velocity = Vector2.zero; // Wyzeruj prędkość.

        // Przywróć oryginalną warstwę, aby postać ponownie kolidowała ze ścianami.
        bodyCollider.gameObject.layer = playerLayer;
    }

    #endregion
}
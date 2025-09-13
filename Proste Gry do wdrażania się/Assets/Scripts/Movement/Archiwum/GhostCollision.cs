using UnityEngine;

public class GhostCollision : MonoBehaviour
{
    private PlayerMovement playerMovement;
    
    private Rigidbody2D rb;
    private bool isStopped = false;

    public void InitializeGhost(PlayerMovement player, float lifeTime)
    {
        playerMovement = player;
        rb = GetComponent<Rigidbody2D>();
        Invoke("StopGhost", lifeTime);
    }
    
    void StopGhost()
    {
        if (rb != null && !isStopped)
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0;
            isStopped = true;
            if (playerMovement != null)
            {
                playerMovement.StartAttraction(transform.position);
            }
        }
    }
    
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Platform") || other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            CancelInvoke("StopGhost");
            StopGhost();
        }
    }
    
    void OnDestroy()
    {
        if (playerMovement != null)
        {
            playerMovement.OnGhostDestroyed();
        }
    }
}
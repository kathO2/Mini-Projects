using UnityEngine;

public class GhostCollision : MonoBehaviour
{
    private PlayerMovement playerMovement;
    
    // Publiczna metoda inicjalizująca ducha
    public void InitializeGhost(PlayerMovement player, float lifeTime)
    {
        playerMovement = player;
        Destroy(gameObject, lifeTime);
    }
    
    void OnCollisionEnter2D(Collision2D other)
    {
        // Sprawdzamy, czy duch zderzył się z warstwą "Platform"
        if (other.gameObject.layer == LayerMask.NameToLayer("Platform"))
        {
            // Niszczymy obiekt, co wywoła metodę OnDestroy()
            Destroy(gameObject);
        }
    }
    
    // Ta metoda jest wywoływana, gdy obiekt jest niszczony, niezależnie od przyczyny
    void OnDestroy()
    {
        if (playerMovement != null)
        {
            playerMovement.OnGhostDestroyed();
        }
    }
}
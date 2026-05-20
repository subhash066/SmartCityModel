using UnityEngine;

public class CarCrashDetector : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool enableDebugLogs = true;
    
    [Header("Crash Settings")]
    public string playerTag = "Player";
    public float crashCooldown = 1f;
    
    private float lastCrashTime = -10f;

    private void OnCollisionEnter(Collision collision)
    {
        // Check cooldown
        if (Time.time - lastCrashTime < crashCooldown) return;
        
        if (enableDebugLogs)
        {
            Debug.Log("<color=yellow><b>[CAR CRASH]</b></color> " + gameObject.name + " collided with: " + collision.gameObject.name);
        }

        // Check if hit player
        if (collision.gameObject.CompareTag(playerTag))
        {
            lastCrashTime = Time.time;
            
            if (enableDebugLogs)
            {
                Debug.Log("<color=red><b>[CAR CRASH]</b></color> *** HIT PLAYER! ***");
            }
            
            HandlePlayerHit(collision);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check cooldown
        if (Time.time - lastCrashTime < crashCooldown) return;
        
        if (enableDebugLogs)
        {
            Debug.Log("<color=yellow><b>[CAR CRASH]</b></color> " + gameObject.name + " trigger with: " + other.gameObject.name);
        }

        if (other.CompareTag(playerTag))
        {
            lastCrashTime = Time.time;
            
            if (enableDebugLogs)
            {
                Debug.Log("<color=red><b>[CAR CRASH]</b></color> *** TRIGGERED PLAYER! ***");
            }
        }
    }

    void HandlePlayerHit(Collision collision)
    {
        // Try to call PlayerHitResponse
        var playerResponse = collision.gameObject.GetComponent<PlayerHitResponse>();
        if (playerResponse != null)
        {
            Vector3 pushDir = transform.forward;
            pushDir.y = 0.5f;
            Vector3 force = pushDir * 20f;
            
            playerResponse.Knockback(force);
        }
        
        // Try NPCImpact as fallback
        var npcImpact = collision.gameObject.GetComponent<NPCImpact>();
        if (npcImpact != null)
        {
            Vector3 hitDir = transform.forward;
            hitDir.y = 0.5f;
            hitDir.Normalize();
            npcImpact.Knockback(hitDir, 20f);
        }
    }

    void OnDrawGizmos()
    {
        // Show car collision bounds
        Gizmos.color = Color.yellow;
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}

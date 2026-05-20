using UnityEngine;

public class CarCrashDetector : MonoBehaviour
{
    private float lastCrashTime = -10f;
    private const float cooldown = 1f;

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time - lastCrashTime < cooldown) return;
        
        if (!collision.gameObject.CompareTag("Player")) return;
        
        lastCrashTime = Time.time;
        
        Debug.Log("<color=red><b>[CAR CRASH]</b></color> HIT PLAYER!");
        
        var playerResponse = collision.gameObject.GetComponent<PlayerHitResponse>();
        if (playerResponse != null)
        {
            Vector3 force = transform.forward * 20f;
            force.y = 10f;
            playerResponse.Knockback(force);
        }
    }
}

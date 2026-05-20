using UnityEngine;
using UnityEngine.SceneManagement;
using StarterAssets;

public class PlayerHitResponse : MonoBehaviour
{
    [Header("Hit Limits & Transitions")]
    [Tooltip("How many hits before loading the target scene.")]
    public int maxHits = 2;
    [Tooltip("The name of the scene to load when hit limit is reached.")]
    public string targetSceneName = "zebra";

    [Header("Auto-Detection Settings")]
    [Tooltip("If true, the player will automatically detect car collisions itself without needing extra scripts on the cars.")]
    public bool autoDetectCarCollisions = true;
    [Tooltip("Tag to identify traffic cars.")]
    public string carTag = "Car";
    [Tooltip("Horizontal push force when hit.")]
    public float autoKnockbackForce = 25f;
    [Tooltip("Upward lift force when hit.")]
    public float autoUpwardForce = 5f;

    [Header("Hit Cooldown")]
    [Tooltip("Time in seconds where the player is invulnerable to consecutive hits.")]
    public float hitCooldown = 2.0f;

    private CharacterController _controller;
    private ThirdPersonController _tpController;
    private Vector3 _impactVelocity;
    private float _impactTimer;
    private float _cooldownTimer = 0f;
    private int _hitCount = 0;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _tpController = GetComponent<ThirdPersonController>();
    }

    void Update()
    {
        // Reduce invulnerability cooldown
        if (_cooldownTimer > 0)
        {
            _cooldownTimer -= Time.deltaTime;
        }

        // Apply knockback movement if active
        if (_impactTimer > 0)
        {
            _impactTimer -= Time.deltaTime;
            
            if (_controller != null && _controller.enabled)
            {
                _controller.Move(_impactVelocity * Time.deltaTime);
            }
            
            // Smoothly decay the knockback force over time
            _impactVelocity = Vector3.Lerp(_impactVelocity, Vector3.zero, Time.deltaTime * 3f);
        }
    }

    /// <summary>
    /// Applies knockback force to the player, triggers slow-motion, and increments hit count.
    /// Compatible with external triggers (like CarTraffic.cs calling this directly).
    /// </summary>
    public void Knockback(Vector3 force)
    {
        if (_cooldownTimer > 0) return; // Prevent double registration
        _cooldownTimer = hitCooldown;

        _hitCount++;
        Debug.Log($"<color=red><b>[CRITICAL IMPACT]</b></color> Hit Count: {_hitCount}/{maxHits}");
        
        _impactVelocity = force;
        _impactTimer = 1.5f;
        
        StartCoroutine(DramaticImpactEffect());
    }

    // --- Automatic Collision & Trigger Detection ---

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!autoDetectCarCollisions) return;
        CheckCollisionWithCar(hit.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!autoDetectCarCollisions) return;
        CheckCollisionWithCar(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!autoDetectCarCollisions) return;
        CheckCollisionWithCar(collision.gameObject);
    }

    private void CheckCollisionWithCar(GameObject hitObj)
    {
        if (_cooldownTimer > 0) return;

        // 1. Search for traffic/follower components anywhere in the colliding object or its parents
        var follower = hitObj.GetComponentInParent<CarWaypointFollower>();
        var traffic = hitObj.GetComponentInParent<CarTraffic>();

        // 2. Check tags on the hit object itself, its immediate parent, or the absolute root object
        bool isCar = hitObj.CompareTag(carTag) || 
                     (hitObj.transform.parent != null && hitObj.transform.parent.CompareTag(carTag)) ||
                     (hitObj.transform.root != null && hitObj.transform.root.gameObject.CompareTag(carTag)) ||
                     follower != null || 
                     traffic != null;

        if (isCar)
        {
            // Determine which object represents the car's main body/center
            Transform carBody = hitObj.transform;
            if (follower != null) carBody = follower.transform;
            else if (hitObj.transform.root != null) carBody = hitObj.transform.root;

            Debug.Log($"<color=orange><b>[PLAYER COLLISION]</b></color> Collided with car: {carBody.name} (via collider {hitObj.name})");
            
            // Calculate a knockback direction pushing away from the main car body
            Vector3 pushDir = (transform.position - carBody.position).normalized;
            pushDir.y = 0; // Keep push horizontal initially
            
            // Mix in the upward lift
            pushDir = (pushDir + Vector3.up * (autoUpwardForce / autoKnockbackForce)).normalized;
            Vector3 force = pushDir * autoKnockbackForce;

            Knockback(force);
        }
    }

    System.Collections.IEnumerator DramaticImpactEffect()
    {
        // 1. Slow down time for cinematic impact
        Time.timeScale = 0.2f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // Disable standard player movement script during knockback
        if (_tpController != null) _tpController.enabled = false;

        // 2. Wait for a brief moment in real-time (independent of timeScale)
        yield return new WaitForSecondsRealtime(1.0f);

        // 3. Restore time back to normal
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;

        // 4. Decide whether to load game over / transition scene, or restore control
        if (_hitCount >= maxHits)
        {
            Debug.Log("<color=yellow><b>[SCENE TRANSITION]</b></color> Loading target scene: " + targetSceneName);
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            // Re-enable player movement controls
            if (_tpController != null) _tpController.enabled = true;
        }
    }
}


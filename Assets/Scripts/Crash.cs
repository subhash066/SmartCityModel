using UnityEngine;

public class Crash : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool enableDebugLogs = true;
    
    [Header("Collision Settings")]
    public string playerTag = "Player";
    public string carTag = "Car";
    public float detectionRadius = 2f;
    
    private int collisionCount = 0;
    private bool isInitialized = false;
    private float checkInterval = 0.2f;
    private float checkTimer = 0f;
    private float startupDelay = 3f;
    private float timeSinceStart = 0f;
    private bool detectionActive = false;

    void Awake()
    {
        Debug.Log("<color=green><b>[CRASH SYSTEM]</b></color> Crash script initialized on: " + gameObject.name);
        
        // Verify collider setup (CharacterController also counts)
        Collider col = GetComponent<Collider>();
        CharacterController cc = GetComponent<CharacterController>();
        
        if (col == null && cc == null)
        {
            Debug.LogWarning("<color=yellow><b>[CRASH SYSTEM]</b></color> No Collider or CharacterController found. Adding SphereCollider.");
            gameObject.AddComponent<SphereCollider>();
        }
        else if (cc != null)
        {
            Debug.Log("<color=green><b>[CRASH SYSTEM]</b></color> CharacterController found (this is fine for player)");
        }
        else
        {
            Debug.Log("<color=green><b>[CRASH SYSTEM]</b></color> Collider found: " + col.GetType().Name + " (isTrigger: " + col.isTrigger + ")");
        }
        
        isInitialized = true;
        Debug.Log("<color=green><b>[CRASH SYSTEM]</b></color> Initialization complete!");
    }

    void Start()
    {
        Debug.Log("<color=green><b>[CRASH SYSTEM]</b></color> Start() called. Object position: " + transform.position);
    }

    void Update()
    {
        timeSinceStart += Time.deltaTime;
        
        // Wait for startup delay before enabling detection
        if (!detectionActive && timeSinceStart >= startupDelay)
        {
            detectionActive = true;
            if (enableDebugLogs)
            {
                Debug.Log("<color=green><b>[CRASH SYSTEM]</b></color> Detection now ACTIVE (startup delay complete)");
            }
        }
        
        if (!detectionActive) return;
        
        // Fallback: Use sphere overlap to detect nearby objects
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            DetectNearbyObjects();
        }
        
        // Visual debug indicator
        if (enableDebugLogs && Time.frameCount % 120 == 0)
        {
            Debug.Log("<color=cyan><b>[CRASH SYSTEM]</b></color> Still active. Collisions detected: " + collisionCount);
        }
    }

    void DetectNearbyObjects()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        
        foreach (Collider col in nearbyColliders)
        {
            if (col.gameObject == gameObject) continue;
            
            // Check for cars
            if (col.CompareTag(carTag) || col.GetComponent<CarWaypointFollower>() != null)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance <= detectionRadius * 0.5f) // Close enough to count as crash
                {
                    if (enableDebugLogs)
                    {
                        Debug.Log("<color=red><b>[CRASH SYSTEM]</b></color> *** CLOSE TO CAR: " + col.gameObject.name + " (distance: " + distance.ToString("F2") + "m) ***");
                    }
                    HandleCarCrashDirect(col);
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        collisionCount++;
        
        if (enableDebugLogs)
        {
            Debug.Log("<color=yellow><b>[CRASH SYSTEM]</b></color> OnCollisionEnter triggered! Collision #" + collisionCount);
            Debug.Log("<color=yellow><b>[CRASH SYSTEM]</b></color> Collided with: " + collision.gameObject.name);
            Debug.Log("<color=yellow><b>[CRASH SYSTEM]</b></color> Object tag: " + collision.gameObject.tag);
            Debug.Log("<color=yellow><b>[CRASH SYSTEM]</b></color> Contact points: " + collision.contactCount);
            Debug.Log("<color=yellow><b>[CRASH SYSTEM]</b></color> Relative velocity: " + collision.relativeVelocity.magnitude);
        }

        // Show contact points
        foreach (ContactPoint contact in collision.contacts)
        {
            if (enableDebugLogs)
            {
                Debug.Log("<color=yellow><b>[CRASH SYSTEM]</b></color> Contact at: " + contact.point);
            }
        }

        // Check for Player
        if (collision.gameObject.CompareTag(playerTag))
        {
            if (enableDebugLogs)
            {
                Debug.Log("<color=red><b>[CRASH SYSTEM]</b></color> *** COLLIDED WITH PLAYER! ***");
            }
            HandlePlayerCrash(collision);
        }
        // Check for Car (using CarWaypointFollower since CarTraffic was removed)
        else if (collision.gameObject.CompareTag(carTag) || 
                 collision.gameObject.GetComponent<CarWaypointFollower>() != null)
        {
            if (enableDebugLogs)
            {
                Debug.Log("<color=red><b>[CRASH SYSTEM]</b></color> *** COLLIDED WITH CAR! ***");
            }
            HandleCarCrash(collision);
        }
        else
        {
            if (enableDebugLogs)
            {
                Debug.Log("<color=gray><b>[CRASH SYSTEM]</b></color> Collided with other object: " + collision.gameObject.name);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enableDebugLogs)
        {
            Debug.Log("<color=magenta><b>[CRASH SYSTEM]</b></color> OnTriggerEnter triggered with: " + other.gameObject.name);
        }

        if (other.CompareTag(playerTag))
        {
            if (enableDebugLogs)
            {
                Debug.Log("<color=red><b>[CRASH SYSTEM]</b></color> *** PLAYER ENTERED TRIGGER! ***");
            }
        }
        else if (other.CompareTag(carTag) || other.GetComponent<CarWaypointFollower>() != null)
        {
            if (enableDebugLogs)
            {
                Debug.Log("<color=red><b>[CRASH SYSTEM]</b></color> *** CAR ENTERED TRIGGER! ***");
            }
        }
    }

    void HandlePlayerCrash(Collision collision)
    {
        Debug.Log("<color=red><b>[CRASH SYSTEM]</b></color> Processing player crash...");
        
        // Get the PlayerHitResponse component
        var playerResponse = collision.gameObject.GetComponent<PlayerHitResponse>();
        if (playerResponse != null)
        {
            Vector3 pushDir = transform.forward;
            pushDir.y = 0.5f;
            Vector3 force = pushDir * 20f;
            
            Debug.Log("<color=red><b>[CRASH SYSTEM]</b></color> Calling Knockback on player!");
            playerResponse.Knockback(force);
        }
        else
        {
            Debug.LogWarning("<color=yellow><b>[CRASH SYSTEM]</b></color> PlayerHitResponse not found on player!");
        }
    }

    void HandleCarCrash(Collision collision)
    {
        Debug.Log("<color=red><b>[CRASH SYSTEM]</b></color> Processing car crash...");
        
        // Visual feedback - flash the car
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            StartCoroutine(FlashColor());
        }
        
        Debug.Log("<color=red><b>[CRASH SYSTEM]</b></color> Car crash processed!");
    }

    void HandleCarCrashDirect(Collider carCollider)
    {
        Debug.Log("<color=red><b>[CRASH SYSTEM]</b></color> Direct car crash detected with: " + carCollider.gameObject.name);
        
        var renderer = carCollider.GetComponent<Renderer>();
        if (renderer != null)
        {
            StartCoroutine(FlashRendererColor(renderer));
        }
    }

    System.Collections.IEnumerator FlashColor()
    {
        var renderer = GetComponent<Renderer>();
        Color originalColor = renderer.material.color;
        
        renderer.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        renderer.material.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        renderer.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        renderer.material.color = originalColor;
    }

    System.Collections.IEnumerator FlashRendererColor(Renderer renderer)
    {
        Material mat = renderer.material;
        Color originalColor = mat.color;
        
        mat.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        mat.color = Color.yellow;
        yield return new WaitForSeconds(0.1f);
        mat.color = originalColor;
    }

    void OnDrawGizmos()
    {
        // Draw detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Draw close detection radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius * 0.5f);
    }

    void OnDrawGizmosSelected()
    {
        // Draw more detailed gizmos when selected
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 2.0f);
        
        // Draw forward direction
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * 3f);
    }
}

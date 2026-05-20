using UnityEngine;

public class Crash : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool enableDebugLogs = true;
    
    [Header("Collision Settings")]
    public string playerTag = "Player";
    public string carTag = "Car";
    
    private int collisionCount = 0;
    private bool isInitialized = false;

    void Awake()
    {
        Debug.Log("<color=green><b>[CRASH SYSTEM]</b></color> Crash script initialized on: " + gameObject.name);
        
        // Verify collider setup
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError("<color=red><b>[CRASH SYSTEM]</b></color> NO COLLIDER FOUND! Add a Collider to this object.");
        }
        else
        {
            Debug.Log("<color=green><b>[CRASH SYSTEM]</b></color> Collider found: " + col.GetType().Name + " (isTrigger: " + col.isTrigger + ")");
        }
        
        // Check if Rigidbody exists
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning("<color=yellow><b>[CRASH SYSTEM]</b></color> No Rigidbody found. Adding one for collision detection.");
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
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
        // Visual debug indicator
        if (enableDebugLogs && Time.frameCount % 60 == 0)
        {
            Debug.Log("<color=cyan><b>[CRASH SYSTEM]</b></color> Still active. Collisions detected: " + collisionCount);
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
            // Draw debug sphere at contact point
            Debug.DrawSphere(contact.point, 0.2f, Color.red, 2.0f);
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
        
        // You can add more crash effects here (sound, particles, etc.)
        Debug.Log("<color=red><b>[CRASH SYSTEM]</b></color> Car crash processed!");
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

    void OnDrawGizmos()
    {
        // Draw a sphere around this object to show detection area
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1.0f);
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

using UnityEngine;

public class NPCImpact : MonoBehaviour
{
    public static event System.Action<Transform, Vector3> OnNPCHit;

    public float knockbackForce = 15f;
    public float stunDuration = 5f;

    public bool IsStunned { get; private set; }

    private NPCMovement movement;
    private CharacterController controller;
    private Rigidbody rb;
    private float stunTimer;
    private Vector3 originalPos;
    private Quaternion originalRot;

    void Start()
    {
        movement = GetComponent<NPCMovement>();
        controller = GetComponent<CharacterController>();
        
        // Ensure Rigidbody is present and set up for physics
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        
        rb.mass = 70f; // Typical human mass
        rb.isKinematic = true; // Stay kinematic until hit
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Disable Animator as requested
        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.enabled = false;

        originalRot = transform.rotation;
    }

    void Update()
    {
        if (!IsStunned) return;

        stunTimer -= Time.deltaTime;

        if (stunTimer <= 0)
        {
            ResetNPC();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (IsStunned) return;

        bool isCar = collision.collider.GetComponent<CarTraffic>() != null
            || collision.collider.GetComponent<CarWaypointFollower>() != null;

        if (isCar)
        {
            Debug.Log($"<color=orange>[PHYSICS IMPACT]</color> {gameObject.name} collided with {collision.gameObject.name}!");
            Vector3 hitDir = (transform.position - collision.transform.position).normalized;
            hitDir.y = 0.5f;

            Knockback(hitDir, knockbackForce);
        }
    }

    // Also handle triggers just in case the car is a trigger
    void OnTriggerEnter(Collider other)
    {
        if (IsStunned) return;

        bool isCar = other.GetComponent<CarTraffic>() != null
            || other.GetComponent<CarWaypointFollower>() != null;

        if (isCar)
        {
            Debug.Log($"<color=orange>[TRIGGER IMPACT]</color> {gameObject.name} hit by {other.gameObject.name}!");
            Vector3 hitDir = (transform.position - other.transform.position).normalized;
            hitDir.y = 0.5f;

            Knockback(hitDir, knockbackForce);
        }
    }

    public void Knockback(Vector3 velocity)
    {
        Knockback(velocity.normalized, velocity.magnitude);
    }

    public void Knockback(Vector3 direction, float force)
    {
        if (IsStunned) return;

        IsStunned = true;
        stunTimer = stunDuration;

        if (movement != null) movement.enabled = false;
        if (controller != null) controller.enabled = false;

        // Enable physics
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.AddForce(direction * force, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * force, ForceMode.Impulse);

        OnNPCHit?.Invoke(transform, direction);
    }

    private void ResetNPC()
    {
        IsStunned = false;
        
        // Disable physics
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Reset rotation (stand up)
        transform.rotation = originalRot;

        if (controller != null) controller.enabled = true;
        if (movement != null) movement.enabled = true;
    }
}

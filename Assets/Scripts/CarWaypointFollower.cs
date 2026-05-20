using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarWaypointFollower : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 10f;
    public float rotationSpeed = 10f;
    public float waypointReachDistance = 2f;

    private int currentWaypointIndex = 0;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Make kinematic to prevent gravity and physics friction from causing shaking
            rb.isKinematic = true;
            // Enable interpolation to ensure buttery smooth movement matching the rendering frame rate
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        if (waypoints == null || waypoints.Length == 0)
            enabled = false;
    }

    public float groundOffset = 0.05f;
    private float yVelocity;
    private float lastGroundHeight;

    void FixedUpdate()
    {
        if (waypoints == null || waypoints.Length == 0 || rb == null)
            return;

        Transform target = waypoints[currentWaypointIndex];
        int prevIndex = currentWaypointIndex == 0 ? waypoints.Length - 1 : currentWaypointIndex - 1;
        Transform prevTarget = waypoints[prevIndex];
        
        if (target == null || prevTarget == null) return;

        // Path following logic
        Vector3 pathDir = (target.position - prevTarget.position).normalized;
        Vector3 toCar = transform.position - prevTarget.position;
        float dot = Vector3.Dot(toCar, pathDir);
        Vector3 projection = prevTarget.position + pathDir * dot;
        
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;

        if (direction.magnitude > 0.01f)
        {
            Vector3 centerCorrection = (projection - transform.position);
            centerCorrection.y = 0;
            
            float deviation = centerCorrection.magnitude;
            if (deviation > 0.01f)
            {
                // Smooth out the steering correction by scaling the Lerp weight proportional to the deviation distance.
                // This prevents the car from rapidly oscillating/wobbling back and forth across the center line.
                float correctionWeight = Mathf.Min(deviation * 1.5f, 1f) * 0.35f;
                direction = Vector3.Lerp(direction, centerCorrection / deviation, correctionWeight).normalized;
            }

            Vector3 nextPos = transform.position + direction * speed * Time.fixedDeltaTime;
            // Removed ground snapping as requested to fix shaking
            nextPos.y = transform.position.y; 

            rb.MovePosition(nextPos);

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * 3f * Time.fixedDeltaTime));
            }
        }

        float distance = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(target.position.x, 0, target.position.z)
        );

        if (distance < waypointReachDistance)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
                currentWaypointIndex = 0;
        }
    }
}

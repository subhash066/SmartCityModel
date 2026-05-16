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
            direction = Vector3.Lerp(direction, centerCorrection.normalized, 0.4f).normalized;

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

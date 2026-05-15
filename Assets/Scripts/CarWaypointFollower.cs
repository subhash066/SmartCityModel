using UnityEngine;

public class CarWaypointFollower : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 10f;
    public float rotationSpeed = 5f;
    public float waypointReachDistance = 2f;

    private int currentWaypointIndex = 0;

    void Update()
    {
        if (waypoints.Length == 0)
            return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];

        // Move toward waypoint
        Vector3 direction = (targetWaypoint.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // Rotate smoothly toward waypoint
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        // Check if waypoint reached
        float distance = Vector3.Distance(transform.position, targetWaypoint.position);

        if (distance < waypointReachDistance)
        {
            currentWaypointIndex++;

            // Loop back to first waypoint
            if (currentWaypointIndex >= waypoints.Length)
            {
                currentWaypointIndex = 0;
            }
        }
    }
}
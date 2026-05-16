using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class NPCMovement : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float rotationSpeed = 90f;
    public float waypointReachDistance = 1f;
    public float minWaitTime = 2f;
    public float maxWaitTime = 6f;
    public Vector2 moveBoundsMin = new Vector2(280f, 200f);
    public Vector2 moveBoundsMax = new Vector2(780f, 750f);
    public float obstacleRayLength = 2f;
    public LayerMask obstacleMask = -1;

    private CharacterController controller;
    private Animator animator;
    private Vector3 targetPosition;
    private float waitTimer;
    private float stuckTimer;
    private Vector3 lastPosition;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        PickNewTarget();
        waitTimer = 0;
        lastPosition = transform.position;
    }

    void Update()
    {
        waitTimer -= Time.deltaTime;
        if (waitTimer > 0)
        {
            SetAnimIdle();
            return;
        }

        Vector3 direction = (targetPosition - transform.position);
        direction.y = 0;

        if (direction.magnitude < waypointReachDistance)
        {
            PickNewTarget();
            return;
        }

        direction.Normalize();

        if (WillHitObstacle(direction))
        {
            Vector3 avoidDir = AvoidObstacle(direction);
            if (avoidDir != Vector3.zero)
                direction = avoidDir;
            else
                PickNewTarget();
        }

        Vector3 move = direction * moveSpeed * Time.deltaTime;
        controller.Move(move);

        SetAnimWalk();

        Quaternion targetRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

        stuckTimer += Time.deltaTime;
        float moved = Vector3.Distance(transform.position, lastPosition);
        if (stuckTimer > 1.5f && moved < 0.1f)
        {
            PickNewTarget();
            stuckTimer = 0;
        }
        lastPosition = transform.position;
    }

    void SetAnimIdle()
    {
        if (animator == null || !animator.enabled) return;
        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);
    }

    void SetAnimWalk()
    {
        if (animator == null || !animator.enabled) return;
        animator.SetBool("Walk", true);
        animator.SetBool("Run", false);
    }

    bool WillHitObstacle(Vector3 dir)
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.5f, dir, obstacleRayLength, obstacleMask);
    }

    Vector3 AvoidObstacle(Vector3 dir)
    {
        for (float angle = 15f; angle <= 90f; angle += 15f)
        {
            if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, Quaternion.Euler(0, angle, 0) * dir, obstacleRayLength, obstacleMask))
                return Quaternion.Euler(0, angle, 0) * dir;
            if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, Quaternion.Euler(0, -angle, 0) * dir, obstacleRayLength, obstacleMask))
                return Quaternion.Euler(0, -angle, 0) * dir;
        }
        return Vector3.zero;
    }

    void PickNewTarget()
    {
        // To encourage road crossing, we pick a target that is significantly far from current position
        float x = Random.Range(moveBoundsMin.x, moveBoundsMax.x);
        float z = Random.Range(moveBoundsMin.y, moveBoundsMax.y);
        
        // If we are currently on one side, try to pick the other side
        if (Random.value > 0.5f)
        {
            float midX = (moveBoundsMin.x + moveBoundsMax.x) / 2f;
            if (transform.position.x < midX) x = Random.Range(midX, moveBoundsMax.x);
            else x = Random.Range(moveBoundsMin.x, midX);
        }

        targetPosition = new Vector3(x, transform.position.y, z);
        waitTimer = Random.Range(0.5f, 2f); // Shorter wait times for more "crossing" action
        stuckTimer = 0;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            new Vector3((moveBoundsMin.x + moveBoundsMax.x) / 2f, 1, (moveBoundsMin.y + moveBoundsMax.y) / 2f),
            new Vector3(moveBoundsMax.x - moveBoundsMin.x, 2, moveBoundsMax.y - moveBoundsMin.y)
        );
    }
}

using UnityEngine;

[RequireComponent(typeof(CarWaypointFollower))]
public class CarTraffic : MonoBehaviour
{
    public float npcDetectRadius = 10f;
    public float npcDetectDistance = 10f;
    public float npcPushForce = 20f;

    private CarWaypointFollower follower;
    private float originalSpeed;

    private TrafficIntersection currentIntersection;
    private HealthbarGames.TrafficLightBase currentLight;

    void Start()
    {
        follower = GetComponent<CarWaypointFollower>();
        originalSpeed = follower.speed;
        if (originalSpeed <= 0) originalSpeed = 10f; // Default safety
    }

    void FixedUpdate()
    {
        DetectNPCs();
        bool obstacleInFront = CheckForCarsInFront();

        if ((currentIntersection != null && currentLight != null) || obstacleInFront)
        {
            bool stop = obstacleInFront;
            if (!stop && currentIntersection != null && currentLight != null)
            {
                stop = currentIntersection.ShouldStop(currentLight, transform.position, transform.forward);
            }

            if (stop)
            {
                // Much smoother deceleration
                follower.speed = Mathf.Lerp(follower.speed, 0f, Time.fixedDeltaTime * 1.5f);
                if (follower.speed < 0.05f)
                    follower.speed = 0f;
            }
            else
            {
                follower.speed = Mathf.Lerp(follower.speed, originalSpeed, Time.fixedDeltaTime * 1.5f);
            }
        }
        else
        {
            follower.speed = Mathf.Lerp(follower.speed, originalSpeed, Time.fixedDeltaTime * 2f);
        }
    }

    bool CheckForCarsInFront()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 1f;
        
        // Use BoxCast to detect cars across the entire width of the lane, preventing "ghosting"
        Vector3 halfExtents = new Vector3(1.2f, 0.5f, 0.5f);
        if (Physics.BoxCast(origin, halfExtents, transform.forward, out hit, transform.rotation, 20f))
        {
            if (hit.collider.gameObject != gameObject && 
                (hit.collider.GetComponent<CarTraffic>() != null || hit.collider.GetComponent<CarWaypointFollower>() != null))
            {
                // Hard stop distance increased to 8m to ensure they stay far apart
                if (hit.distance < 8f) return true;
                
                // Otherwise, slow down proportionally
                float slowFactor = Mathf.Clamp01((hit.distance - 8f) / 12f);
                follower.speed = Mathf.Lerp(follower.speed, originalSpeed * slowFactor, Time.fixedDeltaTime * 2f);
                return false; 
            }
        }
        return false;
    }

    void DetectNPCs()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        // NPC detection
        Collider[] sphereHits = Physics.OverlapSphere(origin, npcDetectRadius);
        foreach (var hit in sphereHits)
        {
            var impact = hit.GetComponent<NPCImpact>();
            if (impact != null && !impact.IsStunned)
                HitNPC(impact);

            // Player detection
            if (hit.CompareTag("Player"))
            {
                HitPlayer(hit.gameObject);
            }
        }
    }

    void HitPlayer(GameObject player)
    {
        Debug.Log($"<color=red><b>[PLAYER HIT]</b></color> Car hit the player!");
        
        Vector3 pushDir = transform.forward;
        pushDir.y = 0.5f;
        Vector3 force = pushDir * npcPushForce;

        // Try direct component call first (more reliable than SendMessage)
        var playerResponse = player.GetComponent<PlayerHitResponse>();
        if (playerResponse != null)
        {
            playerResponse.Knockback(force);
        }
        else
        {
            var npcImpact = player.GetComponent<NPCImpact>();
            if (npcImpact != null)
            {
                npcImpact.Knockback(force);
            }
        }
    }

    void HitNPC(NPCImpact impact)
    {
        Vector3 hitDir = transform.forward;
        hitDir.y = 0.5f;
        hitDir.Normalize();
        impact.Knockback(hitDir, npcPushForce);
    }

    public void EnterIntersection(TrafficIntersection intersection, HealthbarGames.TrafficLightBase light)
    {
        currentIntersection = intersection;
        currentLight = light;
    }

    public void ExitIntersection(TrafficIntersection intersection, HealthbarGames.TrafficLightBase light)
    {
        if (intersection == currentIntersection && light == currentLight)
        {
            currentIntersection = null;
            currentLight = null;
            follower.speed = originalSpeed;
        }
    }
}

using UnityEngine;

public class SmartStreetLight : MonoBehaviour
{
    public Light streetLight;
    public float detectionRadius = 15f;
    public float stayOnTime = 10f;
    public float fadeSpeed = 2f;

    private float timer = 0f;
    private float targetIntensity;
    private float originalIntensity;

    void Start()
    {
        if (streetLight == null) streetLight = GetComponentInChildren<Light>();
        if (streetLight != null)
        {
            originalIntensity = streetLight.intensity;
            streetLight.intensity = 0f;
        }
    }

    void Update()
    {
        timer -= Time.deltaTime;

        // Smart Detection using Physics Overlap (which is faster for spheres, but user wants Raycast)
        // We'll use a downward raycast in a circle or just a simple overlap for efficiency, 
        // but let's fulfill the raycast requirement by doing a "Scanning Raycast".
        
        if (IsSomethingNearby())
        {
            timer = stayOnTime;
        }

        targetIntensity = (timer > 0) ? originalIntensity : 0f;
        if (streetLight != null)
        {
            streetLight.intensity = Mathf.MoveTowards(streetLight.intensity, targetIntensity, Time.deltaTime * fadeSpeed * originalIntensity);
        }
    }

    bool IsSomethingNearby()
    {
        // Raycast requirement: Scan the area below the light
        RaycastHit hit;
        for (int i = 0; i < 8; i++)
        {
            Vector3 dir = Quaternion.Euler(0, i * 45, 0) * Vector3.forward;
            Vector3 origin = transform.position + Vector3.up * 5f;
            if (Physics.SphereCast(origin, 2f, dir, out hit, detectionRadius))
            {
                if (hit.collider.CompareTag("Player") || hit.collider.GetComponent<CarTraffic>() != null || hit.collider.GetComponent<NPCImpact>() != null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}

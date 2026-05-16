using UnityEngine;

public class SmartParkingSpot : MonoBehaviour
{
    public MeshRenderer indicatorRenderer;
    public Material occupiedMaterial;
    public Material vacantMaterial;
    public float detectionRadius = 1.5f;

    private bool isOccupied = false;

    void Start()
    {
        if (indicatorRenderer == null) indicatorRenderer = GetComponentInChildren<MeshRenderer>();
        UpdateIndicator();
    }

    void Update()
    {
        // Raycast down from above the spot to detect a car
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 3f;
        bool currentlyOccupied = false;

        if (Physics.Raycast(origin, Vector3.down, out hit, 4f))
        {
            if (hit.collider.GetComponent<CarTraffic>() != null || hit.collider.GetComponent<CarWaypointFollower>() != null)
            {
                currentlyOccupied = true;
            }
        }

        if (currentlyOccupied != isOccupied)
        {
            isOccupied = currentlyOccupied;
            UpdateIndicator();
            Debug.Log($"Parking Spot {gameObject.name} is now {(isOccupied ? "Occupied" : "Vacant")}");
        }
    }

    void UpdateIndicator()
    {
        if (indicatorRenderer != null)
        {
            indicatorRenderer.material = isOccupied ? occupiedMaterial : vacantMaterial;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = isOccupied ? Color.red : Color.green;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.5f, new Vector3(2, 1, 4));
    }
}

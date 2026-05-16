using UnityEngine;
using HealthbarGames;

public class TrafficIntersection : MonoBehaviour
{
    public TrafficLightBase lightN;    // North approach (coming from north, facing south)
    public TrafficLightBase lightS;    // South approach (coming from south, facing north)
    public TrafficLightBase lightE;    // East approach (coming from east, facing west)
    public TrafficLightBase lightW;    // West approach (coming from west, facing east)

    public float stopDistance = 15f;
    public float zoneLength = 30f;

    void Start()
    {
        SetupZones();
    }

    void SetupZones()
    {
        float halfLen = zoneLength / 2f;

        // Each approach zone is placed BEFORE the intersection
        // North approach: cars at -Z traveling +Z → zone at -Z side
        CreateZone(new Vector3(0, 1.5f, -(stopDistance + halfLen)), new Vector3(8, 3, zoneLength), lightN);
        // South approach: cars at +Z traveling -Z → zone at +Z side
        CreateZone(new Vector3(0, 1.5f, stopDistance + halfLen), new Vector3(8, 3, zoneLength), lightS);
        // West approach: cars at -X traveling +X → zone at -X side
        CreateZone(new Vector3(-(stopDistance + halfLen), 1.5f, 0), new Vector3(zoneLength, 3, 8), lightW);
        // East approach: cars at +X traveling -X → zone at +X side
        CreateZone(new Vector3(stopDistance + halfLen, 1.5f, 0), new Vector3(zoneLength, 3, 8), lightE);
    }

    void CreateZone(Vector3 pos, Vector3 size, TrafficLightBase light)
    {
        if (light == null) return;

        var go = new GameObject("StopZone_" + light.name);
        go.transform.parent = transform;
        go.transform.localPosition = pos;
        go.layer = 0;

        var box = go.AddComponent<BoxCollider>();
        box.size = size;
        box.isTrigger = true;

        var zone = go.AddComponent<IntersectionStopZone>();
        zone.intersection = this;
        zone.light = light;
    }

    public bool ShouldStop(TrafficLightBase light, Vector3 carPos, Vector3 carForward)
    {
        if (light == null) return false;

        var state = light.GetState();
        bool isStopState = state == TrafficLightBase.State.Stop
                        || state == TrafficLightBase.State.PrepareToGo
                        || state == TrafficLightBase.State.PrepareToStop;

        if (!isStopState) return false;

        // Check if car is actually approaching (not leaving)
        Vector3 toCar = carPos - transform.position;
        toCar.y = 0;

        bool onNS = Mathf.Abs(toCar.z) > Mathf.Abs(toCar.x);

        if (onNS)
        {
            bool comingFromSouth = toCar.z > 0;
            float expectedDir = comingFromSouth ? -1f : 1f;
            float dot = Vector3.Dot(carForward.normalized, Vector3.forward) * expectedDir;
            return dot > 0.3f;
        }
        else
        {
            bool comingFromWest = toCar.x < 0;
            float expectedDir = comingFromWest ? 1f : -1f;
            float dot = Vector3.Dot(carForward.normalized, Vector3.right) * expectedDir;
            return dot > 0.3f;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}

public class IntersectionStopZone : MonoBehaviour
{
    public TrafficIntersection intersection;
    public new TrafficLightBase light;

    void OnTriggerEnter(Collider other)
    {
        var car = other.GetComponentInParent<CarTraffic>();
        if (car != null) car.EnterIntersection(intersection, light);
    }

    void OnTriggerExit(Collider other)
    {
        var car = other.GetComponentInParent<CarTraffic>();
        if (car != null) car.ExitIntersection(intersection, light);
    }
}

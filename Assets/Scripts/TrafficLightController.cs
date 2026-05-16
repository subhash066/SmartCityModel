using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrafficLightController : MonoBehaviour
{
    [Header("NS Road Lights (North-South, Z-axis)")]
    public Light[] nsGreen;
    public Light[] nsYellow;
    public Light[] nsRed;

    [Header("EW Road Lights (East-West, X-axis)")]
    public Light[] ewGreen;
    public Light[] ewYellow;
    public Light[] ewRed;

    [Header("Timing")]
    public float greenTime = 12f;
    public float yellowTime = 3f;

    [Header("Zones")]
    public float stopDistance = 15f;
    public float zoneLength = 30f;

    public enum Phase { NS_Go, NS_Stop, EW_Go, EW_Stop }
    public Phase CurrentPhase { get; private set; } = Phase.NS_Go;

    void Start()
    {
        StartCoroutine(CycleLights());
        SetupZones();
    }

    private bool nsPedestrianRequested = false;
    private bool ewPedestrianRequested = false;

    public void RequestPedestrianCrossing(string road)
    {
        if (road == "NS") nsPedestrianRequested = true;
        if (road == "EW") ewPedestrianRequested = true;
    }

    IEnumerator CycleLights()
    {
        while (true)
        {
            // NS Road Go
            SetGroup(nsGreen, true); SetGroup(nsYellow, false); SetGroup(nsRed, false);
            SetGroup(ewGreen, false); SetGroup(ewYellow, false); SetGroup(ewRed, true);
            CurrentPhase = Phase.NS_Go;
            nsPedestrianRequested = false; // Clear request for this road
            NotifyCars();
            
            float timer = 0;
            while (timer < greenTime)
            {
                timer += Time.deltaTime;
                // Smart Timing: If NS is empty AND (EW has cars OR EW has pedestrian request), switch early
                if (timer > 3f && !HasCarsInRoad("NS") && (HasCarsInRoad("EW") || ewPedestrianRequested))
                    break;
                yield return null;
            }

            // NS Transition to Stop
            SetGroup(nsGreen, false); SetGroup(nsYellow, true);
            CurrentPhase = Phase.NS_Stop;
            yield return new WaitForSeconds(yellowTime);

            // EW Road Go
            SetGroup(nsYellow, false); SetGroup(nsRed, true);
            SetGroup(ewRed, false); SetGroup(ewGreen, true);
            CurrentPhase = Phase.EW_Go;
            ewPedestrianRequested = false;
            NotifyCars();
            
            timer = 0;
            while (timer < greenTime)
            {
                timer += Time.deltaTime;
                if (timer > 3f && !HasCarsInRoad("EW") && (HasCarsInRoad("NS") || nsPedestrianRequested))
                    break;
                yield return null;
            }

            // EW Transition to Stop
            SetGroup(ewGreen, false); SetGroup(ewYellow, true);
            CurrentPhase = Phase.EW_Stop;
            yield return new WaitForSeconds(yellowTime);

            SetGroup(ewYellow, false); SetGroup(ewRed, true);
        }
    }

    // New: Use raycast to detect NPCs waiting at the curb
    void FixedUpdate()
    {
        CheckForPedestrians();
    }

    void CheckForPedestrians()
    {
        // Scan for NPCs near the intersection using Raycasts
        Collider[] hits = Physics.OverlapSphere(transform.position, stopDistance + 5f);
        foreach (var hit in hits)
        {
            if (hit.GetComponent<NPCImpact>() != null)
            {
                Vector3 toNPC = hit.transform.position - transform.position;
                if (Mathf.Abs(toNPC.z) > Mathf.Abs(toNPC.x))
                    ewPedestrianRequested = true; // NPC waiting on NS road wants to cross EW
                else
                    nsPedestrianRequested = true;
            }
        }
    }

    bool HasCarsInRoad(string roadName)
    {
        foreach (var road in zoneCars.Values)
        {
            if (road == roadName) return true;
        }
        return false;
    }

    void SetGroup(Light[] lights, bool state)
    {
        if (lights == null) return;
        foreach (var l in lights)
            if (l != null) l.enabled = state;
    }

    void SetupZones()
    {
        float halfLen = zoneLength / 2f;

        // NS south approach: cars coming from +Z (south), traveling north (-Z)
        // Zone at z = stopDist to stopDist+zoneLength (positive Z side)
        CreateZone(new Vector3(0, 1.5f, stopDistance + halfLen), new Vector3(8, 3, zoneLength), "NS");
        // NS north approach: cars coming from -Z (north), traveling south (+Z)
        CreateZone(new Vector3(0, 1.5f, -(stopDistance + halfLen)), new Vector3(8, 3, zoneLength), "NS");

        // EW east approach: cars coming from -X (west), traveling east (+X)
        CreateZone(new Vector3(stopDistance + halfLen, 1.5f, 0), new Vector3(zoneLength, 3, 8), "EW");
        // EW west approach: cars coming from +X (east), traveling west (-X)
        CreateZone(new Vector3(-(stopDistance + halfLen), 1.5f, 0), new Vector3(zoneLength, 3, 8), "EW");
    }

    void CreateZone(Vector3 pos, Vector3 size, string road)
    {
        var go = new GameObject("StopZone_" + road);
        go.transform.parent = transform;
        go.transform.localPosition = pos;
        go.layer = 0;

        var box = go.AddComponent<BoxCollider>();
        box.size = size;
        box.isTrigger = true;

        var zone = go.AddComponent<TrafficStopZone>();
        zone.controller = this;
        zone.road = road;
    }

    public bool ShouldStop(Vector3 carPos, Vector3 carForward)
    {
        Vector3 toCar = carPos - transform.position;
        toCar.y = 0;

        bool onNS = Mathf.Abs(toCar.z) > Mathf.Abs(toCar.x);
        float approachDot;

        if (onNS)
        {
            bool comingFromSouth = toCar.z > 0;
            // If coming from south (+Z), car should travel north (-Z direction)
            float expectedDir = comingFromSouth ? -1f : 1f;
            approachDot = Vector3.Dot(carForward.normalized, Vector3.forward) * expectedDir;
        }
        else
        {
            bool comingFromWest = toCar.x < 0;
            float expectedDir = comingFromWest ? 1f : -1f;
            approachDot = Vector3.Dot(carForward.normalized, Vector3.right) * expectedDir;
        }

        bool approaching = approachDot > 0.3f;
        if (!approaching) return false;

        if (onNS)
            return CurrentPhase == Phase.NS_Stop || CurrentPhase == Phase.EW_Go;
        else
            return CurrentPhase == Phase.EW_Stop || CurrentPhase == Phase.NS_Go;
    }

    void NotifyCars()
    {
        var toRemove = new List<Collider>();
        foreach (var kvp in zoneCars)
        {
            if (kvp.Key == null || !kvp.Key.gameObject.activeInHierarchy)
            {
                toRemove.Add(kvp.Key);
                continue;
            }
        }
        foreach (var c in toRemove) zoneCars.Remove(c);
    }

    private Dictionary<Collider, string> zoneCars = new Dictionary<Collider, string>();

    public void EnterZone(Collider col, string road)
    {
        if (!zoneCars.ContainsKey(col))
            zoneCars[col] = road;
    }

    public void ExitZone(Collider col)
    {
        zoneCars.Remove(col);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
        Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, stopDistance + zoneLength);
    }
}

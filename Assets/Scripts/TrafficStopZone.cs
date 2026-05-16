using UnityEngine;

public class TrafficStopZone : MonoBehaviour
{
    public TrafficLightController controller;
    [HideInInspector] public string road;

    void OnTriggerEnter(Collider other)
    {
        controller.EnterZone(other, road);
    }

    void OnTriggerExit(Collider other)
    {
        controller.ExitZone(other);
    }
}

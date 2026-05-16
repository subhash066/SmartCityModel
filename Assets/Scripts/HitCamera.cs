using UnityEngine;

public class HitCamera : MonoBehaviour
{
    public float showDuration = 3.5f;
    public float smoothSpeed = 6f;

    private Camera hitCam;
    private Transform target;
    private float timer;
    private float zoomT;

    void Start()
    {
        var go = new GameObject("HitReplayCamera", typeof(Camera));
        hitCam = go.GetComponent<Camera>();
        if (Camera.main != null)
        {
            hitCam.CopyFrom(Camera.main);
            hitCam.depth = Camera.main.depth + 1;
            hitCam.fieldOfView = 30;
            hitCam.rect = new Rect(0.55f, 0.55f, 0.44f, 0.44f * 0.75f);
        }
        hitCam.tag = "Untagged";
        hitCam.enabled = false;
        hitCam.gameObject.SetActive(false);

        NPCImpact.OnNPCHit += OnHit;
    }

    void OnHit(Transform npc, Vector3 hitDir)
    {
        if (hitCam == null) return;
        target = npc;
        timer = showDuration;
        zoomT = 0;
        hitCam.enabled = true;
        hitCam.gameObject.SetActive(true);

        hitCam.transform.position = npc.position + Vector3.up * 5f - hitDir * 6f;
        hitCam.transform.LookAt(npc.position + Vector3.up * 1f);
    }

    void Update()
    {
        if (hitCam == null || !hitCam.enabled) return;

        if (target != null && timer > 0)
        {
            timer -= Time.deltaTime;
            zoomT = Mathf.Min(zoomT + Time.deltaTime * 1.5f, 1f);
            float eased = zoomT * (2f - zoomT);

            Vector3 behind = -target.forward * (5f - eased * 2f);
            Vector3 above = Vector3.up * (4f - eased * 1.5f);
            Vector3 desiredPos = target.position + above + behind;

            hitCam.transform.position = Vector3.Lerp(hitCam.transform.position, desiredPos, Time.deltaTime * smoothSpeed);
            hitCam.transform.LookAt(target.position + Vector3.up * 1f);
        }
        else
        {
            hitCam.enabled = false;
            hitCam.gameObject.SetActive(false);
            target = null;
        }
    }

    void OnDestroy()
    {
        NPCImpact.OnNPCHit -= OnHit;
    }
}

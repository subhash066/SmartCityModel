using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public int npcCount = 30;
    public Vector2 spawnAreaMin = new Vector2(280f, 200f);
    public Vector2 spawnAreaMax = new Vector2(780f, 750f);
    public GameObject npcPrefab;
    public bool spawnOnStart = true;

    void Start()
    {
        if (spawnOnStart)
            SpawnAll();
    }

    public void SpawnAll()
    {
        if (npcPrefab == null)
        {
            Debug.LogWarning("No NPC prefab assigned to NPCManager!");
            return;
        }

        for (int i = 0; i < npcCount; i++)
        {
            Vector3 pos = GetRandomSpawnPosition();
            GameObject npc = Instantiate(npcPrefab, pos, Quaternion.Euler(0, Random.Range(0f, 360f), 0), transform);

            if (npc.GetComponent<CharacterController>() == null)
            {
                var cc = npc.AddComponent<CharacterController>();
                cc.height = 1.8f;
                cc.radius = 0.3f;
                cc.center = new Vector3(0, 0.9f, 0);
            }

            NPCMovement movement = npc.GetComponent<NPCMovement>();
            if (movement == null)
                movement = npc.AddComponent<NPCMovement>();

            movement.moveSpeed = Random.Range(1.5f, 3f);
            movement.moveBoundsMin = spawnAreaMin;
            movement.moveBoundsMax = spawnAreaMax;

            if (npc.GetComponent<NPCImpact>() == null)
                npc.AddComponent<NPCImpact>();

            // Add Rigidbody for physics impact
            var rb = npc.GetComponent<Rigidbody>();
            if (rb == null) rb = npc.AddComponent<Rigidbody>();
            rb.mass = 70f;
            rb.isKinematic = true;

            npc.name = $"NPC_Dummy_{i}";

            // Cleanup player components if using PlayerArmature
            var inputs = npc.GetComponent("StarterAssetsInputs");
            if (inputs != null) Destroy(inputs);
            var tpController = npc.GetComponent("ThirdPersonController");
            if (tpController != null) Destroy(tpController);
            var pInput = npc.GetComponent("PlayerInput");
            if (pInput != null) Destroy(pInput);
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        int attempts = 0;
        while (attempts < 10)
        {
            float x = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
            float z = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
            Vector3 pos = new Vector3(x, 0, z);

            Collider[] hits = Physics.OverlapSphere(pos, 1.5f);
            bool blocked = false;
            foreach (var hit in hits)
            {
                if (!hit.isTrigger && hit.gameObject.layer != 8)
                {
                    blocked = true;
                    break;
                }
            }
            if (!blocked)
                return pos;

            attempts++;
        }
        return new Vector3(
            Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            0,
            Random.Range(spawnAreaMin.y, spawnAreaMax.y)
        );
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Vector3 center = new Vector3(
            (spawnAreaMin.x + spawnAreaMax.x) / 2f,
            1,
            (spawnAreaMin.y + spawnAreaMax.y) / 2f
        );
        Vector3 size = new Vector3(
            spawnAreaMax.x - spawnAreaMin.x,
            2,
            spawnAreaMax.y - spawnAreaMin.y
        );
        Gizmos.DrawCube(center, size);
    }
}

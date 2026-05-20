using UnityEngine;
using System.Collections;

public class CarHitPlayer : MonoBehaviour
{
    [Header("Collision Settings")]
    [Tooltip("If attached to the Car, set this to 'Player'. If attached to the Player, set this to 'Car' (or whatever tag the cars have).")]
    public string targetTag = "Player";
    
    [Header("Knockback Settings")]
    public float pushForce = 25f;
    public float upwardForce = 5f;
    public float knockbackDuration = 1.0f;

    [Header("Visual & Time Effects")]
    public bool slowTimeOnHit = true;
    [Range(0.01f, 1f)]
    public float timeScaleAmount = 0.2f;
    public float slowDuration = 1.0f;

    private bool isHit = false;

    private void OnCollisionEnter(Collision collision)
    {
        CheckAndHandleHit(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckAndHandleHit(other.gameObject);
    }

    private void CheckAndHandleHit(GameObject hitObj)
    {
        if (isHit) return;

        // Verify if the target has the correct tag
        if (hitObj.CompareTag(targetTag))
        {
            StartCoroutine(HandleHitSequence(hitObj));
        }
    }

    private IEnumerator HandleHitSequence(GameObject target)
    {
        isHit = true;
        Debug.Log($"<color=red><b>[CAR IMPACT]</b></color> {gameObject.name} hit {target.name}!");

        // Determine knockback direction (from the car/source to the target)
        Vector3 pushDir = (target.transform.position - transform.position).normalized;
        pushDir.y = 0; // Keep push horizontal
        pushDir = (pushDir + Vector3.up * (upwardForce / pushForce)).normalized;
        Vector3 finalForce = pushDir * pushForce;

        // Apply knockback to Player
        ApplyKnockback(target, finalForce);

        // Slow motion effect on impact
        if (slowTimeOnHit)
        {
            float originalTimeScale = Time.timeScale;
            float originalFixedDelta = Time.fixedDeltaTime;

            Time.timeScale = timeScaleAmount;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;

            yield return new WaitForSecondsRealtime(slowDuration);

            Time.timeScale = originalTimeScale;
            Time.fixedDeltaTime = originalFixedDelta;
        }

        isHit = false;
    }

    private void ApplyKnockback(GameObject target, Vector3 force)
    {
        // 1. Try your custom project script (PlayerHitResponse) first if it exists
        var hitResponse = target.GetComponent<PlayerHitResponse>();
        if (hitResponse != null)
        {
            hitResponse.Knockback(force);
            return;
        }

        // 2. Otherwise, check if target uses a CharacterController (like Unity's StarterAssets)
        var controller = target.GetComponent<CharacterController>();
        if (controller != null)
        {
            // Temporarily disable standard movement script so player doesn't fight the knockback
            var tp = target.GetComponent("ThirdPersonController") as MonoBehaviour;
            if (tp != null) tp.enabled = false;

            StartCoroutine(MoveController(controller, force, tp));
        }
        else
        {
            // 3. Fallback: If target uses standard Rigidbody physics
            var rb = target.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.AddForce(force, ForceMode.Impulse);
            }
        }
    }

    private IEnumerator MoveController(CharacterController controller, Vector3 force, MonoBehaviour tpController)
    {
        float timer = knockbackDuration;
        Vector3 currentVelocity = force;

        while (timer > 0 && controller != null && controller.enabled)
        {
            timer -= Time.deltaTime;
            controller.Move(currentVelocity * Time.deltaTime);
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, Time.deltaTime * 3f);
            yield return null;
        }

        // Re-enable standard controls
        if (tpController != null)
        {
            tpController.enabled = true;
        }
    }
}

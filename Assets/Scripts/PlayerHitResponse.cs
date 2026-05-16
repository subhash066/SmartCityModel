using UnityEngine;
using StarterAssets;

public class PlayerHitResponse : MonoBehaviour
{
    private CharacterController _controller;
    private ThirdPersonController _tpController;
    private Vector3 _impactVelocity;
    private float _impactTimer;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _tpController = GetComponent<ThirdPersonController>();
    }

    void Update()
    {
        if (_impactTimer > 0)
        {
            _impactTimer -= Time.deltaTime;
            _controller.Move(_impactVelocity * Time.deltaTime);
            _impactVelocity = Vector3.Lerp(_impactVelocity, Vector3.zero, Time.deltaTime * 3f);
        }
    }

    public void Knockback(Vector3 force)
    {
        Debug.Log("<color=red><b>[CRITICAL IMPACT]</b></color> Dramatic Camera Focus on Player!");
        _impactVelocity = force;
        _impactTimer = 1.5f;
        
        // Smart City Dramatic Effect: Slow motion on hit
        StartCoroutine(DramaticImpactEffect());
    }

    System.Collections.IEnumerator DramaticImpactEffect()
    {
        // Slow down time
        Time.timeScale = 0.2f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        if (_tpController != null) _tpController.enabled = false;

        // Wait for a brief moment in real-time
        yield return new WaitForSecondsRealtime(0.5f);

        // Restore time
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;

        yield return new WaitForSeconds(1f);
        if (_tpController != null) _tpController.enabled = true;
    }
}

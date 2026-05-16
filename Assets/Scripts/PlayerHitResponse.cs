using UnityEngine;
using UnityEngine.SceneManagement;
using StarterAssets;

public class PlayerHitResponse : MonoBehaviour
{
    private CharacterController _controller;
    private ThirdPersonController _tpController;
    private Vector3 _impactVelocity;
    private float _impactTimer;

    private int _hitCount = 0;
    private const int MaxHits = 2;

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
        _hitCount++;
        Debug.Log($"<color=red><b>[CRITICAL IMPACT]</b></color> Hit Count: {_hitCount}/{MaxHits}");
        
        _impactVelocity = force;
        _impactTimer = 1.5f;
        
        StartCoroutine(DramaticImpactEffect());
    }

    System.Collections.IEnumerator DramaticImpactEffect()
    {
        // Slow down time
        Time.timeScale = 0.2f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        if (_tpController != null) _tpController.enabled = false;

        // Wait for a brief moment in real-time
        yield return new WaitForSecondsRealtime(1.0f);

        // Restore time
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;

        if (_hitCount >= MaxHits)
        {
            Debug.Log("<color=yellow><b>[SCENE TRANSITION]</b></color> Loading Zebra Scene...");
            SceneManager.LoadScene("zebra");
        }
        else
        {
            if (_tpController != null) _tpController.enabled = true;
        }
    }
}

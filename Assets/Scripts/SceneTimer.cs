using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTimer : MonoBehaviour
{
    public string sceneToLoad = "zebra";
    public float delay = 30f;
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= delay)
        {
            Debug.Log($"<color=cyan>[TIMER]</color> 15 seconds passed. Teleporting to {sceneToLoad}...");
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}

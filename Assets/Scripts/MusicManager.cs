using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip backgroundMusic;
    private static MusicManager instance;
    private AudioSource audioSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            
            if (backgroundMusic != null) audioSource.clip = backgroundMusic;
            
            audioSource.loop = true;
            audioSource.playOnAwake = true;
            audioSource.volume = 0.5f;
            
            if (audioSource.clip != null)
            {
                if (!audioSource.isPlaying) audioSource.Play();
                Debug.Log($"<color=green>[MUSIC]</color> Now playing: {audioSource.clip.name}");
            }
            else
            {
                Debug.LogWarning("<color=orange>[MUSIC]</color> Music Manager initialized but NO CLIP assigned. Run Tools > Auto Setup Scene.");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

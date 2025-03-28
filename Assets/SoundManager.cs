using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgMusicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip bgMusic;
    public AudioClip buttonClick;
    public AudioClip spinning;
    public AudioClip win;
    public AudioClip fail;
    public AudioClip money;
    public AudioClip spinningStop;

    private void Awake()
    {
        // Singleton Pattern - Prevent duplicates
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayBGMusic();
    }

    // Play Background Music
    public void PlayBGMusic()
    {
        if (bgMusicSource != null && bgMusic != null)
        {
            bgMusicSource.clip = bgMusic;
            bgMusicSource.loop = true;
            bgMusicSource.Play();
        }
    }

    // Stop Background Music
    public void StopBGMusic()
    {
        if (bgMusicSource.isPlaying)
        {
            bgMusicSource.Stop();
        }
    }

    // Play Sound Effect
    public void PlaySFX(string soundName)
    {
        AudioClip clipToPlay = null;

        switch (soundName)
        {
            case "buttonClick":
                clipToPlay = buttonClick;
                break;
            case "spinning":
                clipToPlay = spinning;
                break;
            case "spinningStop":
                clipToPlay = spinningStop;
                break;
            case "win":
                clipToPlay = win;
                break;
            case "fail":
                clipToPlay = fail;
                break;
            case "money":
                clipToPlay = money;
                break;
            default:
                Debug.LogWarning("Sound not found: " + soundName);
                return;
        }

        if (clipToPlay != null)
        {
            sfxSource.PlayOneShot(clipToPlay);
        }
    }

    // Stop All Sound Effects
    public void StopSFX()
    {
        sfxSource.Stop();
    }
}

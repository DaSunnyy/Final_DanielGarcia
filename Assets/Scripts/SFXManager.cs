using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [Header("Player Sounds")]
    public AudioClip Jump;
    public AudioClip Hurt;
    public AudioClip Ouch;
    public AudioClip Win;
    public AudioClip MagnetAttract;
    public AudioClip MagnetShot;
    public AudioClip Gun;

    [Header("Enemy Sounds")]
    public AudioClip Boom;

    private AudioSource sfxSource;
    private AudioSource loopSource;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        sfxSource = gameObject.AddComponent<AudioSource>();
        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.loop = true;
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlayLoop(AudioClip clip)
    {
        if (clip == null) return;
        if (loopSource.isPlaying && loopSource.clip == clip) return;

        loopSource.clip = clip;

        if (clip == MagnetAttract)
            loopSource.volume = Mathf.Clamp(1.5f * sfxSource.volume, 0f, 1f);
        else
            loopSource.volume = sfxSource.volume;

        loopSource.Play();
    }

    public void StopLoop(AudioClip clip)
    {
        if (loopSource.clip == clip)
            loopSource.Stop();
    }
}
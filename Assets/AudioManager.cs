using UnityEngine;

// Singleton class for managing audio in the game.
// This class handles playing sound effects and music, including a specific sound for upgrades.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioSource sfxSource;
    public AudioSource musicSource;
    public AudioClip upgradeClip;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void PlayUpgradeSound()
    {
        PlaySFX(upgradeClip);
    }
}

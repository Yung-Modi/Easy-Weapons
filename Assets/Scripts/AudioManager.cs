using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioMixer mixer;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load saved volume
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        SetVolume(savedVolume);
    }

    public void SetVolume(float volume)
    {
        mixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public float GetSavedVolume()
    {
        return PlayerPrefs.GetFloat("MasterVolume", 1f);
    }
}

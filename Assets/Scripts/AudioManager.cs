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

    }

    // Set any exposed mixer parameter
    public void SetVolume(string parameter, float volume)
    {
        mixer.SetFloat(parameter, volume);
        PlayerPrefs.SetFloat(parameter, volume);
    }

    // Get saved value for any parameter
    public float GetSavedVolume(string parameter, float defaultValue = 1f)
    {
        return PlayerPrefs.GetFloat(parameter, defaultValue);
    }
}
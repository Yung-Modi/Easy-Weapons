using UnityEngine;

public static class PreferencesManager
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Update is called once per frame

    public static float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat("MusicVolume", 1f);
    }

    public static float GetMasterVolume()
    {
        return PlayerPrefs.GetFloat("MasterVolume", 1f);
    }

    public static void SetMusicVolume(float soundLevel)
    {
        PlayerPrefs.SetFloat("MusicVolume", soundLevel);
    }

    public static void SetMasterVolume(float soundLevel)
    {
        PlayerPrefs.SetFloat("MasterVolume", soundLevel);
    }
}

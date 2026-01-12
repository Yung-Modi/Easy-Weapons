using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement; // Required for scene management
using TMPro; // Use if using TextMeshPro for UI text

public class SettingsMenu : MonoBehaviour
{
    public AudioMixer audioMixer;
    public TMPro.TMP_Dropdown resolutionDropdown; // Use TextMeshPro dropdown
    private Resolution[] resolutions;

    void Start()
    {
        // ... (Code to populate resolution dropdown - see search result 0.1.22)
    }

    // Function to set master volume
    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", volume); // "MasterVolume" must be an exposed parameter in your Audio Mixer
        PlayerPrefs.SetFloat("masterVolume", volume); // Save the setting
    }

    // Function to set quality level
    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    // Function to toggle fullscreen mode
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    // Function to return to the main menu
    public void BackButton()
    {
        // Optional: Add a confirmation dialog or save settings explicitly here
        SceneManager.LoadScene("MainMenu"); // Load your main menu scene
    }
}

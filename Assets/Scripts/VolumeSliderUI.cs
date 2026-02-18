using UnityEngine;
using UnityEngine.UI;

public class VolumeSliderUI : MonoBehaviour
{
    public Slider volumeSlider;

    private void Start()
    {
        // Set slider to saved value
        volumeSlider.value = AudioManager.Instance.GetSavedVolume();

        // Listen for changes
        volumeSlider.onValueChanged.AddListener(ChangeVolume);
    }

    public void ChangeVolume(float value)
    {
        AudioManager.Instance.SetVolume(value);
    }
}

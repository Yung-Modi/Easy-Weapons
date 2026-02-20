using UnityEngine;
using UnityEngine.UI;

public class VolumeSliderUI : MonoBehaviour
{
    public Slider slider;
    public string mixerParameter; // Example: MusicVolume

    private void Start()
    {
        slider.value = AudioManager.Instance.GetSavedVolume(mixerParameter);
        slider.onValueChanged.AddListener(ChangeVolume);
    }

    private void ChangeVolume(float value)
    {
        AudioManager.Instance.SetVolume(mixerParameter, value);
    }
}
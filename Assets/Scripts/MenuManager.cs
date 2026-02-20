using UnityEngine;

public class MenuManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioManager.Instance.SetVolume("MasterVolume", AudioManager.Instance.GetSavedVolume("MasterVolume"));
        AudioManager.Instance.SetVolume("MusicVolume", AudioManager.Instance.GetSavedVolume("MusicVolume"));

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

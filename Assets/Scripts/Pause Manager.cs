using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public bool isPaused = false;
    public GameObject pauseMenu;
    public Weapon[] weapons; // Reference to the player's weapons
    public GameObject resumeButton; // Reference to the Resume button in the pause menu

    public void OnPause(InputValue inputValue)
    {
        // Example implementation: toggle pause state
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0 : 1;
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(isPaused);
            EventSystem.current.SetSelectedGameObject(resumeButton); // Set focus to Resume button when paused
        }

        if (isPaused)
        {
            Cursor.lockState = CursorLockMode.None;
            // Stop all weapons from firing when paused
            foreach (Weapon weapon in weapons)
            {
                weapon.isFiring = false;
                weapon.StopBeam(); // Assuming this method stops any ongoing firing effects
                weapon.showCrosshair = false;
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            foreach (Weapon weapon in weapons)
            {
                weapon.showCrosshair = true; // Re-enable crosshair when unpausing
                Debug.Log("Unpaused: Crosshair re-enabled for weapon.");
            }
        }

    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1;
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
        Cursor.lockState = CursorLockMode.Locked;
        foreach (Weapon weapon in weapons)
        {
            weapon.showCrosshair = true; // Re-enable crosshair when resuming
            Debug.Log("Resumed: Crosshair re-enabled for weapon.");
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0;
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(true);
            EventSystem.current.SetSelectedGameObject(resumeButton); // Set focus to Resume button when paused
        }
        Cursor.lockState = CursorLockMode.None;
        foreach (Weapon weapon in weapons)
        {
            weapon.isFiring = false;
            weapon.StopBeam(); // Assuming this method stops any ongoing firing effects
            weapon.showCrosshair = false; // Hide crosshair when paused
            Debug.Log("Paused: Crosshair hidden for weapon.");
        }
    }
}

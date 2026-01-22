using UnityEngine;
using TMPro; 

public class QuitManager : MonoBehaviour
{
    // This public method can be called by the UI button
    public void QuitGame()
    {
        // Log a message to the console for testing purposes
        Debug.Log("Quit game requested");

        // If we are running in the Unity Editor
#if UNITY_EDITOR
        // Exit Play Mode
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // Quit the application in a built game
            Application.Quit();
#endif
    }
}


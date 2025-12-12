using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    // Assign the UI Image that represents the fill (Image.type = Filled recommended)
    public Image fillImage;

    // Optional: assign the player's Health component. If left empty will search for GameObject tagged "Player".
    public Health playerHealth;

    void Start()
    {
        if (fillImage == null)
        {
            Debug.LogWarning("PlayerHealthUI: No fillImage assigned.");
            enabled = false;
            return;
        }

        if (playerHealth == null)
        {
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO != null)
                playerHealth = playerGO.GetComponent<Health>();
        }

        if (playerHealth == null)
        {
            Debug.LogWarning("PlayerHealthUI: No player Health found.");
            enabled = false;
            return;
        }

        playerHealth.OnHealthChanged += OnHealthChanged;
        OnHealthChanged(playerHealth.CurrentHealth, playerHealth.maxHealth);
    }

    void OnHealthChanged(float current, float max)
    {
        if (fillImage != null)
            fillImage.fillAmount = Mathf.Clamp01(current / max);
    }

    void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= OnHealthChanged;
    }
}
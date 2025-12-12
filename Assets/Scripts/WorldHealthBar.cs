using UnityEngine;
using UnityEngine.UI;

public class WorldHealthBar : MonoBehaviour
{
    // Health bar prefab: a World Space Canvas with an Image child named "Fill"
    public GameObject healthBarPrefab;
    public Vector3 offset = new Vector3(0f, 2f, 0f);

    private GameObject instance;
    private Image fillImage;
    private Transform camTransform;
    private Health health;

    void Start()
    {
        health = GetComponent<Health>();
        if (health == null)
        {
            enabled = false;
            return;
        }

        if (healthBarPrefab != null)
        {
            instance = Instantiate(healthBarPrefab);
            // Expect the prefab to contain an Image child named "Fill"
            var fillTransform = instance.transform.Find("Fill");
            if (fillTransform != null)
                fillImage = fillTransform.GetComponent<Image>();
            else
                Debug.LogWarning("WorldHealthBar: prefab missing child named 'Fill'");
        }

        if (Camera.main != null)
            camTransform = Camera.main.transform;

        // Subscribe to health changes
        health.OnHealthChanged += OnHealthChanged;

        // initialize
        OnHealthChanged(health.CurrentHealth, health.maxHealth);
    }

    void LateUpdate()
    {
        if (instance == null) return;

        // Position above the object
        instance.transform.position = transform.position + offset;

        // Billboard towards camera
        if (camTransform != null)
        {
            instance.transform.rotation = Quaternion.LookRotation(instance.transform.position - camTransform.position);
        }
    }

    void OnHealthChanged(float current, float max)
    {
        if (fillImage != null)
            fillImage.fillAmount = Mathf.Clamp01(current / max);

        if (current <= 0f)
        {
            if (instance != null)
                Destroy(instance);
            // unsubscribe to be safe
            if (health != null)
                health.OnHealthChanged -= OnHealthChanged;
            enabled = false;
        }
    }

    void OnDestroy()
    {
        if (instance != null)
            Destroy(instance);
        if (health != null)
            health.OnHealthChanged -= OnHealthChanged;
    }
}
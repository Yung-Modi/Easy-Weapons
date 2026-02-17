using UnityEngine;
using UnityEngine.UI;

public class DataManager : MonoBehaviour
{
    // Singleton instance
    public static DataManager Instance { get; private set; }

    // Private player reference
    private Player player;

    // UI references
    public Text coinsText;
    public Text killsText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // simple Unity singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = Player.Instance;

        if (killsText != null && killsText.gameObject.activeInHierarchy && player != null)
            killsText.text = "Points: " + player.kills;
    }

    // Update is called once per frame
    void Update()
    {
        // ensure player reference
        if (player == null)
            player = Player.Instance;

        if (killsText != null && killsText.gameObject.activeInHierarchy && player != null)
            killsText.text = "Points: " + player.kills;
    }
}

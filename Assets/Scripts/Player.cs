using UnityEngine;

public class Player : MonoBehaviour
{
    // Singleton instance
    public static Player Instance { get; private set; }

    // Player data
    public int points = 0;
    public int coins = 0;
    public int currentBlaster = 0;
    public int highestWave = 0;
    public string playerName = "PlayerName";
    public int[] ownedBlasters = new int[] { 1, 0, 0, 0, 0, 0, 0 };

    void Awake()
    {
        // Implement simple Unity singleton pattern
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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using UnityEngine;
using System.IO;

public class HighscoreManager : MonoBehaviour
{
    public GameManager gameManager; // Reference to your GameManager, assign in Inspector
    // Singleton
    public static HighscoreManager Instance { get; private set; }

    // Session and saved values
    public int highscoreKills = 0;

    // HUD options
    public bool showHUD = true;
    public bool topRight = true;
    public int guiMargin = 10;
    public int guiWidth = 220;
    public int guiHeight = 24;
    public int fontSize = 18;
    public Color fontColor = Color.black;
    public GUIStyle guiStyle;

    private readonly string fileName = "highscore.json";

    void Awake()
    {
        // Simple singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        LoadHighscore();
    }

    // Call this when an enemy is killed.
    // You can call HighscoreManager.RegisterKill() from your GameManager or Health.Die().
    public static void RegisterKill()
    {
        if (Instance != null)
            Instance.AddKill();
    }

    public void AddKill()
    {
        gameManager.kills++;
        if (gameManager.kills > highscoreKills)
        {
            highscoreKills = gameManager.kills;
            SaveHighscore();
        }
    }

    public void ResetSessionKills()
    {
        gameManager.kills = 0;
    }

    public void ResetHighscore()
    {
        highscoreKills = 0;
        string path = Path.Combine(Application.persistentDataPath, fileName);
        if (File.Exists(path))
        {
            try
            {
                File.Delete(path);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed to delete highscore file: {ex.Message}");
            }
        }
    }

    public void SaveHighscore()
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        try
        {
            var data = new HighscoreData { highscoreKills = this.highscoreKills };
            string json = JsonUtility.ToJson(data);
            File.WriteAllText(path, json);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Failed to save highscore to file: {ex.Message}");
        }
    }

    public void LoadHighscore()
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        if (!File.Exists(path))
        {
            highscoreKills = 0;
            return;
        }

        try
        {
            string json = File.ReadAllText(path);
            if (!string.IsNullOrEmpty(json))
            {
                var data = JsonUtility.FromJson<HighscoreData>(json);
                if (data != null)
                    highscoreKills = data.highscoreKills;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Failed to load highscore from file: {ex.Message}");
            highscoreKills = 0;
        }
    }

    void OnGUI()
    {
        if (!showHUD) return;

        if (guiStyle == null)
        {
            guiStyle = new GUIStyle(GUI.skin.label);
            guiStyle.alignment = TextAnchor.MiddleRight;
            guiStyle.fontSize = fontSize;
            guiStyle.normal.textColor = fontColor;
        }

        string text = $"High: {highscoreKills}";

        float x = Screen.width - guiMargin - guiWidth;
        float y = topRight ? guiMargin : Screen.height - guiMargin - guiHeight;
        Rect rect = new Rect(x, y, guiWidth, guiHeight);

        GUI.Label(rect, text, guiStyle);
    }

    // Simple serializable container for JSON storage
    [System.Serializable]
    private class HighscoreData
    {
        public int highscoreKills;
    }
}
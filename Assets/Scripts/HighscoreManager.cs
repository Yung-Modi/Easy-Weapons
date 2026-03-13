using System.IO;
using TMPro;
using UnityEngine;

public class HighScoreManager : MonoBehaviour
{
    public static HighScoreManager Instance;

    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI newHighScoreText;

    private HighScoreData data = new HighScoreData();
    private string filePath;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        filePath = Application.persistentDataPath + "/highscore.json";
    }

    void Start()
    {
        LoadHighScore();
        UpdateHighScoreUI();

        if (newHighScoreText != null)
            newHighScoreText.gameObject.SetActive(false);
    }

    public void CheckForHighScore(int finalKills)
    {
        if (finalKills > data.highScore)
        {
            data.highScore = finalKills;
            SaveHighScore();
            UpdateHighScoreUI();

            if (newHighScoreText != null)
                newHighScoreText.gameObject.SetActive(true);
        }
    }

    void SaveHighScore()
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
    }

    void LoadHighScore()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            data = JsonUtility.FromJson<HighScoreData>(json);
        }
        else
        {
            data.highScore = 0;
        }
    }

    void UpdateHighScoreUI()
    {
        if (highScoreText != null)
            highScoreText.text = "HIGH SCORE: " + data.highScore;
    }

    public void DeleteHighScore()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        data.highScore = 0;
        UpdateHighScoreUI();

        if (newHighScoreText != null)
            newHighScoreText.gameObject.SetActive(false);

        Debug.Log("High score data deleted.");
    }
}

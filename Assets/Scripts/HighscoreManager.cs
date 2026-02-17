using System.IO;
using System.Collections;
using TMPro;
using UnityEngine;

[System.Serializable]
public class HighScoreData
{
    public int highScore;
}

public class HighScoreManager : MonoBehaviour
{
    public static HighScoreManager Instance;

    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI newHighScoreText; // Optional popup

    private HighScoreData data = new HighScoreData();
    private string filePath;

    // coroutine handle so we can restart/stop the timer if another high score occurs
    private Coroutine hideCoroutine;

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
            {
                newHighScoreText.gameObject.SetActive(true);

                // restart hide timer
                if (hideCoroutine != null)
                    StopCoroutine(hideCoroutine);
                hideCoroutine = StartCoroutine(HideNewHighScoreCoroutine());
            }
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

    private IEnumerator HideNewHighScoreCoroutine()
    {
        yield return new WaitForSeconds(5f);
        if (newHighScoreText != null)
            newHighScoreText.gameObject.SetActive(false);
        hideCoroutine = null;
    }
}

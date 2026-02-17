using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton pattern for easy access
    public TextMeshProUGUI killCountText; // Assign your UI Text element here in the Inspector
    public int kills = 0;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void EnemyKilled()
    {
        kills++;
        UpdateKillCountUI();

        HighScoreManager.Instance.CheckForHighScore(kills);
    }

    void UpdateKillCountUI()
    {
        if (killCountText != null)
        {
            killCountText.text = "ENEMIES KILLED: " + kills;
        }
    }

    public void GameOver()
    {
        HighScoreManager.Instance.CheckForHighScore(kills);
    }

}
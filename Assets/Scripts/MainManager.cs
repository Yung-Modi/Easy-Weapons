using UnityEngine;

public class MainManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadPlayerData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SavePlayerData()
    {
        if (Player.Instance == null)
        {
            Debug.LogWarning("SavePlayerData: Player.Instance is null.");
            return;
        }

        SaveSystem.SavePlayer(Player.Instance);
    }

    public void LoadPlayerData()
    {
        SaveData data = SaveSystem.LoadPlayer();
        if (data == null)
        {
            Debug.Log("LoadPlayerData: no save data found.");
            return;
        }

        if (Player.Instance == null)
        {
            Debug.LogWarning("LoadPlayerData: Player.Instance is null.");
            return;
        }

        Player p = Player.Instance;
        p.kills = data.kills;
        p.playerName = data.playerName;
    }
}

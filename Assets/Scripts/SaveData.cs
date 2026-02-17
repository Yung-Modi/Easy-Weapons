using UnityEngine;

[System.Serializable]
public class SaveData
{
    // Player data fields (mirror Player.cs)
    public int kills;
    public string playerName;

    // Parameterless constructor for serializers
    public SaveData() { }

    // Construct SaveData from a Player instance
    public SaveData(Player player)
    {
        if (player == null)
        {
            kills = 0;
            playerName = "PlayerName";
            return;
        }

        kills = player.kills;
        playerName = player.playerName;
    }
}

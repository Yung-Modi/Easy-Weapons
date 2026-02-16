using UnityEngine;

[System.Serializable]
public class SaveData
{
    // Player data fields (mirror Player.cs)
    public int points;
    public int coins;
    public int currentBlaster;
    public int highestWave;
    public string playerName;
    public int[] ownedBlasters;

    // Parameterless constructor for serializers
    public SaveData() { }

    // Construct SaveData from a Player instance
    public SaveData(Player player)
    {
        if (player == null)
        {
            points = 0;
            coins = 0;
            currentBlaster = 0;
            highestWave = 0;
            playerName = "PlayerName";
            ownedBlasters = new int[] { 1, 0, 0, 0, 0, 0, 0 };
            return;
        }

        points = player.points;
        coins = player.coins;
        currentBlaster = player.currentBlaster;
        highestWave = player.highestWave;
        playerName = player.playerName;
        ownedBlasters = (player.ownedBlasters != null) ? (int[])player.ownedBlasters.Clone() : new int[] { 1, 0, 0, 0, 0, 0, 0 };
    }
}

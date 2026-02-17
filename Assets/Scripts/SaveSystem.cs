using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public static class SaveSystem
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Update is called once per frame
    // (Original MonoBehaviour comments preserved)

    // Save file name
    private static readonly string fileName = "save.dat";

    // Create a static SavePlayer method that takes a Player as a parameter, player.
    public static void SavePlayer(Player player)
    {
        // Create a new BinaryFormatter.
        BinaryFormatter formatter = new BinaryFormatter();

        // Create a string path set to a persistent data path.
        string path = Path.Combine(Application.persistentDataPath, fileName);

        // Create a new file stream from that path.
        FileStream stream = new FileStream(path, FileMode.Create);

        // Create a SaveData object constructed with the player parameter.
        SaveData data = new SaveData(player);

        // Use the formatter to serialize the data and add it to the file stream.
        formatter.Serialize(stream, data);

        // Close the file stream.
        stream.Close();
    }

    // Create a static LoadPlayer method that returns SaveData.
    public static SaveData LoadPlayer()
    {
        // Create a string path set to a persistent data path (Use same path).
        string path = Path.Combine(Application.persistentDataPath, fileName);

        // If the file exists at that path
        if (File.Exists(path))
        {
            // Create a new BinaryFormatter.
            BinaryFormatter formatter = new BinaryFormatter();

            // Open a new file stream from that path.
            FileStream stream = new FileStream(path, FileMode.Open);

            // Create a SaveData object and set it to the contents of the stream Deserialized by the formatter as a SaveData type.
            SaveData data = (SaveData)formatter.Deserialize(stream);

            // Close the file stream.
            stream.Close();

            // return the SaveData.
            return data;
        }
        else
        {
            // Log an #.
            Debug.Log("#");
            // return null.
            return null;
        }
    }
}

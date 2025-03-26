using UnityEngine;
using System.IO;

[System.Serializable]
public class PlayerData
{
    public float playerSpeed;
    public bool dashUnlocked;
    public int soulCount;
}

public static class SaveSystem
{
    private static string savePath = Application.persistentDataPath + "/save.json";

    public static void SaveData(PlayerData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
    }

    public static PlayerData LoadData()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            return JsonUtility.FromJson<PlayerData>(json);
        }

        return new PlayerData { playerSpeed = 5f, dashUnlocked = false, soulCount = 0 }; // Default values
    }
}
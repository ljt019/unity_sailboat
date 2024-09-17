using UnityEngine;
using System.IO;

[System.Serializable]
public class GameConfig
{
    public int UdpPort = 8080;
    public float MaxAdcValue = 1023f;
    public float MinAdcValue = 0f;
}

public class JsonConfigManager : MonoBehaviour
{
    private static string CONFIG_PATH => Path.Combine(Application.dataPath, "../config.json");

    public static GameConfig LoadConfig()
    {
        if (File.Exists(CONFIG_PATH))
        {
            string jsonContent = File.ReadAllText(CONFIG_PATH);
            return JsonUtility.FromJson<GameConfig>(jsonContent);
        }
        else
        {
            GameConfig defaultConfig = new GameConfig();
            SaveConfig(defaultConfig);
            return defaultConfig;
        }
    }

    public static void SaveConfig(GameConfig config)
    {
        string jsonContent = JsonUtility.ToJson(config, true); // True for pretty print
        File.WriteAllText(CONFIG_PATH, jsonContent);
    }
}
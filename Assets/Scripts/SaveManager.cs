using UnityEngine;

public static class SaveManager
{
    public static void SaveGame(string name)
    {
        PlayerPrefs.SetString("Name", name);

        PlayerPrefs.Save();
    }

    public static void SaveGame(int highScore)
    {
        PlayerPrefs.SetInt("HighScore", highScore);

        PlayerPrefs.Save();
    }

    public static void SaveGame(int highScore, string name)
    {
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.SetString("Name", name);

        PlayerPrefs.Save();
    }

    public static void SaveGame(SaveData saveData)
    {
        PlayerPrefs.SetInt("HighScore", saveData.highScore);
        PlayerPrefs.SetString("Name", saveData.name);

        PlayerPrefs.Save();
    }

    public static SaveData LoadGame()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        string name = PlayerPrefs.GetString("Name", "None");

        SaveData save = new SaveData(highScore, name);
        return save;
    }

    public static int GetHighScore()
    {
        return PlayerPrefs.GetInt("HighScore", 0);
    }

    public static string GetName()
    {
        return PlayerPrefs.GetString("Name", "None");
    }
}

[System.Serializable]
public class SaveData
{
    public int highScore;
    public string name;

    public SaveData(int highScore, string name)
    {
        this.highScore = highScore;
        this.name = name;
    }
}

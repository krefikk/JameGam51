using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private SaveData saveData;
    public SaveData SaveData => saveData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

        saveData = SaveManager.LoadGame();
    }
}

using UnityEngine;

public class InGameManager : MonoBehaviour
{
    public static InGameManager Instance;

    public bool paused = false;
    public bool gameEnded = false;

    [SerializeField] private Animator endGamePanel;
    [SerializeField] private Animator pausePanel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (paused)
                ContinueGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        paused = true;
        Time.timeScale = 0f;
        pausePanel.SetBool("Open", true);
    }

    public void ContinueGame()
    {
        paused = false;
        Time.timeScale = 1f;
        pausePanel.SetBool("Open", false);
    }

    public void EndGame()
    {
        gameEnded = true;
        endGamePanel.SetBool("Open", true);
    }
}

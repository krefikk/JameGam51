using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameManager : MonoBehaviour
{
    public static InGameManager Instance;

    private ScoreManager scoreManager;

    public bool paused = false;
    public bool gameEnded = false;
    public bool restarting = false;

    [SerializeField] private Animator endGamePanel;
    [SerializeField] private TextMeshProUGUI scoreTextMesh;
    [SerializeField] private TextMeshProUGUI timeTextMesh;

    // Animasyonlanacak
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private RectTransform oxygenBar;
    [SerializeField] private RectTransform timer;
    [SerializeField] private RectTransform score;
    [SerializeField] private Transform player;

    private Vector2 startPosHealth, startPosOxygen, startPosTimer, startPosScore;
    private Vector3 startPosPlayer;

    [SerializeField] private float animDuration = 0.6f;
    [SerializeField] private float offScreenOffset = 500f;

    [SerializeField] private Animator pausePanel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        scoreManager = FindFirstObjectByType<ScoreManager>();
    }

    private void OnEnable()
    {
        player.GetComponent<PlayerStats>().onDiverDie += EndGame;
    }

    private void OnDisable()
    {
        if (player != null)
        {
            var stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.onDiverDie -= EndGame;
            }
        }
    }

    private void Start()
    {
        SaveOriginalPositions();
        SetObjectsOffScreen();
        StartCoroutine(IntroSequence());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !gameEnded)
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
        paused = false;
        gameEnded = true;

        scoreTextMesh.text = "Score: " + scoreManager.CurrentScore.ToString();
        timeTextMesh.text = "Time: " + UIManager.Instance.SecondsToMinute(UIManager.Instance.Timer);

        CheckAndSetScore();

        pausePanel.SetBool("Open", false);
        endGamePanel.SetBool("Open", true);
    }

    public void RestartGame()
    {
        if (!restarting)
        {
            Time.timeScale = 1f;
            StartCoroutine(OutroSequenceAndRestart());
        }
    }

    public void GoToMainMenu()
    {
        if (!restarting)
        {
            Time.timeScale = 1f;
            StartCoroutine(OutroAndLoadMenu());
        }
    }

    private IEnumerator OutroAndLoadMenu()
    {
        restarting = true;

        StartCoroutine(MoveUI(healthBar, startPosHealth + Vector2.up * offScreenOffset));
        StartCoroutine(MoveUI(oxygenBar, startPosOxygen + Vector2.up * offScreenOffset));
        StartCoroutine(MoveUI(timer, startPosTimer + Vector2.up * offScreenOffset));
        StartCoroutine(MoveUI(score, startPosScore + Vector2.up * offScreenOffset));

        if (pausePanel != null) pausePanel.SetBool("Open", false);
        if (endGamePanel != null) endGamePanel.SetBool("Open", false);

        if (player != null)
            StartCoroutine(MoveTransform(player, player.position + Vector3.right * 15f));

        yield return new WaitForSeconds(animDuration);
        SceneManager.LoadScene("MainMenu");
    }

    private void SaveOriginalPositions()
    {
        startPosHealth = healthBar.anchoredPosition;
        startPosOxygen = oxygenBar.anchoredPosition;
        startPosTimer = timer.anchoredPosition;
        startPosScore = score.anchoredPosition;
        startPosPlayer = player.position;
    }

    private void SetObjectsOffScreen()
    {
        healthBar.anchoredPosition += Vector2.up * offScreenOffset;
        oxygenBar.anchoredPosition += Vector2.up * offScreenOffset;

        timer.anchoredPosition += Vector2.up * offScreenOffset;
        score.anchoredPosition += Vector2.up * offScreenOffset;

        player.position += Vector3.left * 10f;
    }

    private IEnumerator IntroSequence()
    {
        StartCoroutine(MoveUI(healthBar, startPosHealth));
        StartCoroutine(MoveUI(oxygenBar, startPosOxygen));
        StartCoroutine(MoveUI(timer, startPosTimer));
        StartCoroutine(MoveUI(score, startPosScore));

        StartCoroutine(MoveTransform(player, startPosPlayer));

        yield return null;
    }

    private IEnumerator OutroSequenceAndRestart()
    {
        restarting = true;

        StartCoroutine(MoveUI(healthBar, startPosHealth + Vector2.up * offScreenOffset));
        StartCoroutine(MoveUI(oxygenBar, startPosOxygen + Vector2.up * offScreenOffset));
        StartCoroutine(MoveUI(timer, startPosTimer + Vector2.up * offScreenOffset));
        StartCoroutine(MoveUI(score, startPosScore + Vector2.up * offScreenOffset));

        if (pausePanel != null) pausePanel.SetBool("Open", false);
        if (endGamePanel != null) endGamePanel.SetBool("Open", false);

        StartCoroutine(MoveTransform(player, player.position + Vector3.right * 15f));

        yield return new WaitForSeconds(animDuration);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator MoveUI(RectTransform target, Vector2 targetPos)
    {
        Vector2 initialPos = target.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < animDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / animDuration;
            t = t * t * (3f - 2f * t);

            target.anchoredPosition = Vector2.Lerp(initialPos, targetPos, t);
            yield return null;
        }
        target.anchoredPosition = targetPos;
    }

    private IEnumerator MoveTransform(Transform target, Vector3 targetPos)
    {
        Vector3 initialPos = target.position;
        float elapsed = 0f;

        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animDuration;
            t = t * t * (3f - 2f * t);

            target.position = Vector3.Lerp(initialPos, targetPos, t);
            yield return null;
        }
        target.position = targetPos;
    }

    private void CheckAndSetScore()
    {
        if (scoreManager.CurrentScore > SaveManager.GetHighScore())
        {
            SaveManager.SaveGame(scoreManager.CurrentScore);
            _ = LeaderboardManager.SetNewEntry();
        }
    }
}

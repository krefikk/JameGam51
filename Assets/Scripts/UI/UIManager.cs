using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public PlayerState state;

    [Header("Health Bar")]
    [SerializeField] private Animator[] healthBars = new Animator[5]; // index 0: leftest, index 4: rightest
    private int currentHealthIndex = 4;

    [Header("Oxygen Bar")]
    [SerializeField] private Slider oxygenBar;
    [SerializeField] private TextMeshProUGUI oxygenPercent;
    private bool canChangeOxygen = false;

    [Header("Timer")]
    [SerializeField] private TextMeshProUGUI timerMesh;
    private float timer;
    public float Timer => timer;

    [Header("Score")]
    [SerializeField] private TextMeshProUGUI scoreMesh;
    [SerializeField] private float scoreAnimationSpeed = 5f;
    private ScoreManager scoreManager;
    private float displayedScore = 0f;  

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        scoreManager = FindFirstObjectByType<ScoreManager>();
        state = FindFirstObjectByType<PlayerState>();

        timer = 0f;
        timerMesh.text = "00:00";
    }

    private void Start()
    {
        StartCoroutine(FillOxygenBar());
        foreach (var bar in healthBars)
        {
            bar.SetBool("Fill", true);
        }
    }

    private void Update()
    {
        if (state.dead)
            return;

        IncreaseTimer();
        UpdateAnimatedScore();
    }

    public void DecreaseHealthBar(int damage)
    {
        int i;
        int limit = currentHealthIndex - damage;
        if (limit < -1)
            limit = -1;

        for (i = currentHealthIndex; i > limit; i--)
        {
            healthBars[i].SetBool("Fill", false);
        }

        currentHealthIndex = i;
    }

    private IEnumerator FillOxygenBar()
    {
        oxygenBar.value = 0f;

        float timer = 0f;
        float duration = 1f;
        float startValue = 0f;
        float targetValue = oxygenBar.maxValue;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            oxygenBar.value = Mathf.Lerp(startValue, targetValue, timer / duration);
            yield return null;
        }

        oxygenBar.value = targetValue;
        canChangeOxygen = true;
    }

    public void UpdateOxygenBar(float capacity, float filled)
    {
        if (!canChangeOxygen)
            return;

        oxygenBar.value = filled / capacity;
        oxygenPercent.text = Mathf.FloorToInt(oxygenBar.value * 100).ToString() + "%";
    }

    private void IncreaseTimer()
    {
        timer += Time.deltaTime;
        timerMesh.text = SecondsToMinute(timer);
    }

    public string SecondsToMinute(float seconds)
    {
        if (seconds < 0) 
            seconds = 0;

        int minutes = Mathf.FloorToInt(seconds / 60);
        int remainingSeconds = Mathf.FloorToInt(seconds % 60);

        string minuteText = minutes.ToString("D2") + ":" + remainingSeconds.ToString("D2");
        return minuteText;
    }

    private void UpdateAnimatedScore()
    {
        if (scoreManager == null) return;

        float targetScore = scoreManager.CurrentScore;

        if (Mathf.Abs(displayedScore - targetScore) > 0.5f)
            displayedScore = Mathf.Lerp(displayedScore, targetScore, Time.deltaTime * scoreAnimationSpeed);
        else
            displayedScore = targetScore;

        scoreMesh.text = Mathf.RoundToInt(displayedScore).ToString();
    }
}

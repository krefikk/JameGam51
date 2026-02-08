using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Header("References")]
    private PlayerStats player;
    private PlayerState state;

    [Header("Score Settings")]
    [SerializeField] private float baseScorePerSecond = 10f;
    [SerializeField] private int pointsPerBubble = 100;
    [SerializeField] private int penaltyPerMine = 250;

    [Header("Risk Multipliers")]
    [Tooltip("When health decreases, how many times faster should the point gain rate increase ?")]
    [SerializeField] private float maxHealthMultiplier = 3f;

    [Tooltip("How many times faster should the score increase rate become when oxygen decreases ?")]
    [SerializeField] private float maxOxygenMultiplier = 2f;

    private float survivalScore;
    public int CurrentScore => GetTotalScore();

    private void Awake()
    {
        player = FindFirstObjectByType<PlayerStats>();
        if (player != null)
            state = player.GetComponent<PlayerState>();
    }

    private void Update()
    {
        if (player == null || state == null)
            return;

        if (state.dead)
            return;

        CalculateSurvivalScore();
    }

    private void CalculateSurvivalScore()
    {
        float healthRatio = Mathf.Clamp01((float)player.Health / player.maxHealth);
        float oxygenRatio = Mathf.Clamp01(player.Oxygen / player.maxOxygen);

        float currentHealthMult = Mathf.Lerp(maxHealthMultiplier, 1f, healthRatio);
        float currentOxygenMult = Mathf.Lerp(maxOxygenMultiplier, 1f, oxygenRatio);

        float totalMultiplier = currentHealthMult * currentOxygenMult;

        survivalScore += baseScorePerSecond * totalMultiplier * Time.deltaTime;
    }

    public int GetTotalScore()
    {
        int bubbleBonus = state.BubblesPopped * pointsPerBubble;
        int minePenalty = state.MinesExploded * penaltyPerMine;

        float finalScore = survivalScore + bubbleBonus - minePenalty;
        return Mathf.Max(0, Mathf.RoundToInt(finalScore));
    }
}
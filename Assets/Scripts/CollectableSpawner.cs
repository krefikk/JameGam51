using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CollectableSpawner : MonoBehaviour
{
    private PlayerState state;

    private Queue<Vector2> bubbleSpawnPositions = new Queue<Vector2>();
    private Queue<Vector2> mineSpawnPositions = new Queue<Vector2>();
    private List<Vector2> allSpawnPositions = new List<Vector2>();

    [SerializeField] private float queueRefreshTime = 5f;
    private float queueRefreshTimer = 0f;

    [SerializeField] private GameObject bubble;
    [SerializeField] private GameObject mine;

    [SerializeField] private float goldenBubbleChance = 0.1f;
    [SerializeField] private float dangerousMineChance = 0.05f;
    [SerializeField] private float goldenBubbleChanceIncreaseRate = 0.001f;
    [SerializeField] private float dangerousMineChanceIncreaseRate = 0.002f;

    [SerializeField] private Transform maxMineSpawnWidth;
    [SerializeField] private Transform minMineSpawnWidth;
    [SerializeField] private Transform maxBubbleSpawnWidth;
    [SerializeField] private Transform minBubbleSpawnWidth;

    [SerializeField] private Transform collectableSpawnHeight;

    [SerializeField] private float timeToMaxDifficulty = 360f;
    private float currentDifficultyTime = 0f;

    [SerializeField] private float minWaitTimeBetweenMineSpawns = 3.5f;
    [SerializeField] private float maxWaitTimeBetweenMineSpawns = 25f;

    [SerializeField] private float minWaitTimeBetweenBubbleSpawns = 5f;
    [SerializeField] private float maxWaitTimeBetweenBubbleSpawns = 15f;

    [SerializeField] private float minPaddingForBubbles;
    [SerializeField] private float minPaddingForMines;

    [SerializeField] private float waitTimeForSpawnAfterGameStarts = 5f;
    private int maxSpawnTry = 100;

    private bool spawningBubble = false;
    private bool spawningMine = false;

    private void Awake()
    {
        state = FindFirstObjectByType<PlayerState>();
    }

    private void Start()
    {
        StartCoroutine(WaitFor(waitTimeForSpawnAfterGameStarts));
    }

    private void Update()
    {
        if (state.dead)
            return;

        if (currentDifficultyTime < timeToMaxDifficulty)
            currentDifficultyTime += Time.deltaTime;

        RefreshQueues();
        SpawnMechanism();
        IncreaseChancesOverTime();
    }

    private void SpawnMechanism()
    {
        if (!spawningBubble)
            StartCoroutine(SpawnBubbleMechanismCoroutine());

        if (!spawningMine)
            StartCoroutine(SpawnMineMechanismCoroutine());
    }

    private IEnumerator SpawnBubbleMechanismCoroutine()
    {
        spawningBubble = true;
        SpawnRandomBubble();

        float ratio = Mathf.Clamp01(currentDifficultyTime / timeToMaxDifficulty);
        float dynamicMinWait = Mathf.Lerp(minWaitTimeBetweenBubbleSpawns, maxWaitTimeBetweenBubbleSpawns, ratio);

        float randomWaitTime = Random.Range(dynamicMinWait, maxWaitTimeBetweenBubbleSpawns);
        yield return new WaitForSeconds(randomWaitTime);

        spawningBubble = false;
    }

    private IEnumerator SpawnMineMechanismCoroutine()
    {
        spawningMine = true;
        SpawnRandomMine();

        float ratio = Mathf.Clamp01(currentDifficultyTime / timeToMaxDifficulty);
        float dynamicMaxWait = Mathf.Lerp(maxWaitTimeBetweenMineSpawns, minWaitTimeBetweenMineSpawns, ratio);

        float randomWaitTime = Random.Range(minWaitTimeBetweenMineSpawns, dynamicMaxWait);
        yield return new WaitForSeconds(randomWaitTime);

        spawningMine = false;
    }

    private IEnumerator WaitFor(float delay)
    {
        yield return new WaitForSeconds(delay);
    }

    private void RefreshQueues()
    {
        queueRefreshTimer += Time.deltaTime;
        if (queueRefreshTimer >= queueRefreshTime)
        {
            if (bubbleSpawnPositions.Count > 0)
                bubbleSpawnPositions.Dequeue();

            if (mineSpawnPositions.Count > 0)
                mineSpawnPositions.Dequeue();

            queueRefreshTimer = 0f;
        }

        allSpawnPositions.Clear();
        allSpawnPositions.AddRange(bubbleSpawnPositions);
        allSpawnPositions.AddRange(mineSpawnPositions);
    }

    private void IncreaseChancesOverTime()
    {
        goldenBubbleChance += Time.deltaTime * goldenBubbleChanceIncreaseRate;
        dangerousMineChance += Time.deltaTime * dangerousMineChanceIncreaseRate;

        if (goldenBubbleChance >= 1)
            goldenBubbleChance = 1;

        if (dangerousMineChance >= 1) 
            dangerousMineChance = 1;
    }

    public void SpawnRandomBubble()
    {
        int maxSpawnTryForBubble = 0;

        Vector2 spawnPos;
        do
        {
            float xPos = Random.Range(minBubbleSpawnWidth.position.x, maxBubbleSpawnWidth.position.x);
            spawnPos = new Vector2(xPos, collectableSpawnHeight.position.y);
            maxSpawnTryForBubble++;
        } 
        while (!CheckPositionAvailableForBubbles(spawnPos) && maxSpawnTryForBubble < maxSpawnTry);

        if (spawnPos == null)
            return;

        bubbleSpawnPositions.Enqueue(spawnPos);

        GameObject bubbleObj = Instantiate(bubble, spawnPos, Quaternion.identity);
        int random = Random.Range(0, 1000);
        if (random <= 1000 * goldenBubbleChance)
            bubbleObj.GetComponent<AirBubble>().SetOxygenAmount(25);
    }

    public void SpawnRandomMine()
    {
        int maxSpawnTryForMine = 0;

        Vector2 spawnPos;
        do
        {
            float xPos = Random.Range(minMineSpawnWidth.position.x, maxMineSpawnWidth.position.x);
            spawnPos = new Vector2(xPos, collectableSpawnHeight.position.y);
            maxSpawnTryForMine++;
        }
        while (!CheckPositionAvailableForMines(spawnPos) && maxSpawnTryForMine < maxSpawnTry);

        mineSpawnPositions.Enqueue(spawnPos);

        GameObject mineObj = Instantiate(mine, spawnPos, Quaternion.identity);
        int random = Random.Range(0, 1000);
        if (random <= 1000 * dangerousMineChance)
            mineObj.GetComponent<Mine>().SetDamage(2);
    }

    private bool CheckPositionAvailableForBubbles(Vector2 pos)
    {
        foreach (var position in bubbleSpawnPositions)
        {
            if (Mathf.Abs(pos.x - position.x) < minPaddingForBubbles)
                return false;
        }

        return true;
    }

    private bool CheckPositionAvailableForMines(Vector2 pos)
    {
        foreach (var position in allSpawnPositions)
        {
            if (Mathf.Abs(pos.x - position.x) < minPaddingForMines)
                return false;
        }

        return true;
    }
}

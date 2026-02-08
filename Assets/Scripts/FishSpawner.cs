using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct FishSpawnRequest
{
    public int amount;
    public int difficulty;
    public float waitTime;

    public FishSpawnRequest(int amount, int difficulty, float waitTime = 0)
    {
        this.amount = amount;
        this.difficulty = difficulty;
        this.waitTime = waitTime;
    }
}

public class FishSpawner : MonoBehaviour
{
    private PlayerState state;
    private Queue<FishSpawnRequest> awaitingRequests = new Queue<FishSpawnRequest>();

    [SerializeField] private GameObject rightPiranha;
    [SerializeField] private GameObject leftPiranha;

    [SerializeField] private Transform leftSideMaximumSpawnHeight;
    [SerializeField] private Transform leftSideMinimumSpawnHeight;
    [SerializeField] private Transform rightSideMaximumSpawnHeight;
    [SerializeField] private Transform rightSideMinimumSpawnHeight;

    [SerializeField] private GameObject indicator;
    [SerializeField] private Transform rightSideIndicatorMeridian;
    [SerializeField] private Transform leftSideIndicatorMeridian;

    [SerializeField] private float minimumYPadding;

    [SerializeField] private float timeToMaxDifficulty = 900f;
    private float currentGameTime = 0f;

    [Tooltip("Time that must pass after the indicator appears on the screen for the fish to start arriving")]
    [SerializeField] private float easyFishWaitTime = 2.5f;
    [SerializeField] private float midFishWaitTime = 1.5f;
    [SerializeField] private float hardFishWaitTime = 0.85f;

    [Tooltip("Time that must pass after a spawning sequence to spawn new fishes")]
    [SerializeField] private float easyFishSpawnWaitTime = 3.5f;
    [SerializeField] private float midFishSpawnWaitTime = 2.2f;
    [SerializeField] private float hardFishSpawnWaitTime = 1.2f;

    private bool spawningFishes = false;

    private void Awake()
    {
        state = FindFirstObjectByType<PlayerState>();
    }

    private void Update()
    {
        if (state.dead)
            return;

        if (currentGameTime < timeToMaxDifficulty)
            currentGameTime += Time.deltaTime;

        if (awaitingRequests.Count == 0 && !spawningFishes)
            GenerateNextWave();

        if (awaitingRequests.Count > 0 && !spawningFishes)
        {
            FishSpawnRequest nextRequest = awaitingRequests.Dequeue();
            spawningFishes = true;

            if (nextRequest.waitTime > 0)
                StartCoroutine(ProcessRequestWithDelay(nextRequest));
            else
                StartCoroutine(SpawnFishesCoroutine(nextRequest.amount, nextRequest.difficulty));
        }
    }

    private void GenerateNextWave()
    {
        float progress = Mathf.Clamp01(currentGameTime / timeToMaxDifficulty);

        int amount = DetermineAmount(progress);

        int difficulty = DetermineDifficulty(progress);
        float waveInterval = Mathf.Lerp(4f, 0.5f, progress);

        FishSpawnRequest request = new FishSpawnRequest(amount, difficulty, waveInterval);
        awaitingRequests.Enqueue(request);
    }

    private int DetermineAmount(float progress)
    {
        int baseAmount = Mathf.RoundToInt(Mathf.Lerp(2, 5, progress));

        if (progress < 0.2f)
        {
            if (Random.value < 0.2f)
                return 1;
        }

        return baseAmount;
    }

    private int DetermineDifficulty(float progress)
    {
        float randomValue = Random.value;

        if (progress < 0.25f)
            return 0;
        else if (progress < 0.60f)
            return (randomValue < 0.7f) ? 1 : 0;
        else
            return (randomValue < 0.6f) ? 2 : 1;
    }

    private IEnumerator ProcessRequestWithDelay(FishSpawnRequest request)
    {
        yield return new WaitForSeconds(request.waitTime);
        StartCoroutine(SpawnFishesCoroutine(request.amount, request.difficulty));
    }

    public void SpawnFishesByList(List<FishSpawnRequest> requests)
    {
        foreach (var request in requests)
        {
            awaitingRequests.Enqueue(request);
        }
    }

    private IEnumerator WaitFor(float time)
    {
        yield return new WaitForSeconds(time);
    }

    public void SpawnFishes(int amount, int difficulty)
    {
        if (!spawningFishes)
            StartCoroutine(SpawnFishesCoroutine(amount, difficulty));
        else
            awaitingRequests.Enqueue(new FishSpawnRequest(amount, difficulty));
    }

    private IEnumerator SpawnFishesCoroutine(int amount, int difficulty) // 0: easy, 1: mid, 2: hard
    {
        spawningFishes = true;

        List<Fish> fishes = SpawnRandomFishes(amount);
        List<GameObject> indicators = SpawnIndicators(fishes);

        switch (difficulty)
        {
            case 0:
                yield return new WaitForSeconds(easyFishWaitTime);
                break;
            case 1:
                yield return new WaitForSeconds(midFishWaitTime);
                break;
            case 2:
                yield return new WaitForSeconds(hardFishWaitTime);
                break;
        }

        StartMoving(indicators, fishes);

        switch (difficulty)
        {
            case 0:
                yield return new WaitForSeconds(easyFishSpawnWaitTime);
                break;
            case 1:
                yield return new WaitForSeconds(midFishSpawnWaitTime);
                break;
            case 2:
                yield return new WaitForSeconds(hardFishSpawnWaitTime);
                break;
        }

        spawningFishes = false;
    }

    public List<Fish> SpawnRandomFishes(int amount)
    {
        List<Vector2> spawnPositions = new List<Vector2>();
        List<Fish> spawnedFishes = new List<Fish>();

        for (int i = 0; i < amount; i++)
        {
            bool onLeft = Random.Range(0, 2) == 1 ? true : false;
            float height = Random.Range(leftSideMinimumSpawnHeight.position.y, leftSideMaximumSpawnHeight.position.y);

            if (onLeft)
            {
                Vector2 spawnPosition = new Vector2(leftSideMaximumSpawnHeight.position.x, height);
                if (CheckIfSpawnable(spawnPositions, spawnPosition))
                {
                    GameObject fishObj = Instantiate(rightPiranha, spawnPosition, Quaternion.identity);
                    spawnedFishes.Add(fishObj.GetComponent<Fish>());
                    spawnPositions.Add(spawnPosition);
                }
                else
                    i--;
            }
            else
            {
                Vector2 spawnPosition = new Vector2(rightSideMaximumSpawnHeight.position.x, height);
                if (CheckIfSpawnable(spawnPositions, spawnPosition))
                {
                    GameObject fishObj = Instantiate(leftPiranha, spawnPosition, Quaternion.identity);
                    spawnedFishes.Add(fishObj.GetComponent<Fish>());
                    spawnPositions.Add(spawnPosition);
                }
                else
                    i--;
            }
        }

        return spawnedFishes;
    }

    public List<GameObject> SpawnIndicators(List<Fish> fishes)
    {
        List<GameObject> indicators = new List<GameObject>();

        foreach (var fish in fishes)
        {
            if (OnLeft(fish.transform.position))
            {
                Vector2 spawnPos = new Vector2(leftSideIndicatorMeridian.position.x, fish.transform.position.y);
                GameObject indicatorObj = Instantiate(indicator, spawnPos, Quaternion.identity);
                indicators.Add(indicatorObj);
            }
            else
            {
                Vector2 spawnPos = new Vector2(rightSideIndicatorMeridian.position.x, fish.transform.position.y);
                GameObject indicatorObj = Instantiate(indicator, spawnPos, Quaternion.identity);
                indicators.Add(indicatorObj);
            }
        }

        return indicators;
    }

    private void StartMoving(List<GameObject> indicators, List<Fish> fishes)
    {
        foreach (var indicator in indicators)
        {
            Destroy(indicator);
        }

        foreach (var fish in fishes)
        {
            fish.StartMove();
        }
    }

    private bool CheckIfSpawnable(List<Vector2> positions, Vector2 position)
    {
        foreach (var pos in positions)
        {
            if (Vector2.Distance(pos, position) < minimumYPadding)
                return false;
        }

        return true;
    }

    private bool OnLeft(Vector2 pos)
    {
        // On right
        if (Mathf.Abs(pos.x - leftSideMaximumSpawnHeight.position.x) > Mathf.Abs(pos.x - rightSideMaximumSpawnHeight.position.x))
        {
            return false;
        }
        // On left
        else
        {
            return true;
        }
    }
}

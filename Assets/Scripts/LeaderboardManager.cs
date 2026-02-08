using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Exceptions;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    private const string leaderboardName = "Leaderboard";
    private const string leaderboardID = "leaderboard";

    [Header("Leaderboard Data")]
    [SerializeField] private List<TextMeshProUGUI> nameFields;
    [SerializeField] private List<TextMeshProUGUI> scoreFields;

    [Header("Player (Out-of-range row)")]
    [SerializeField] private TMP_Text playerRankField;
    [SerializeField] private TMP_Text playerNameField;
    [SerializeField] private TMP_Text playerScoreField;
    [SerializeField] private Color playerColor = Color.yellow;
    [SerializeField] private Color defaultNameColor = Color.white;

    private Dictionary<string, int> leaderboard;

    [Header("Flags")]
    private bool uiNeedsUpdate = true;
    private bool refreshInProgress = false;

    [Header("Timer")]
    [SerializeField] private float leaderboardUpdateTime = 90f;
    private float timer = 0f;

    private async void Start()
    {
        await InitializeLeaderboardServicesAsync();
        await RefreshLeaderboardAsync();
    }

    private void Update()
    {
        timer += Time.unscaledDeltaTime;
        if (timer >= leaderboardUpdateTime)
        {
            timer = 0f;
            _ = RefreshLeaderboardAsync();
        }

        if (uiNeedsUpdate)
        {
            UpdateUI();
            uiNeedsUpdate = false;
        }
    }

    /*
     * Initializes Unity Services + anonymous auth
     */
    public static async Task InitializeLeaderboardServicesAsync()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    /*
     * Fetch full leaderboard and mark UI for redraw
     */
    public async Task RefreshLeaderboardAsync()
    {
        if (refreshInProgress) return;
        refreshInProgress = true;
        try
        {
            leaderboard = await GetLeaderboardAsync();
            uiNeedsUpdate = true;
        }
        catch (LeaderboardsException e)
        {
            Debug.LogWarning($"[Leaderboard] Refresh failed: {e.Reason}");
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            refreshInProgress = false;
        }
    }

    /*
     * Pull entries page by page; keep highest score on duplicate names
     */
    public async Task<Dictionary<string, int>> GetLeaderboardAsync()
    {
        var result = new Dictionary<string, int>();

        try
        {
            int limit = 100;
            int offset = 0;

            while (true)
            {
                var page = await LeaderboardsService.Instance.GetScoresAsync(
                    leaderboardID,
                    new GetScoresOptions { Limit = limit, Offset = offset }
                );

                if (page?.Results == null || page.Results.Count == 0)
                    break;

                foreach (var entry in page.Results)
                {
                    string playerName = entry.PlayerName ?? "None";
                    int hashIndex = playerName.IndexOf('#');
                    if (hashIndex > 0) playerName = playerName.Substring(0, hashIndex);

                    int score = (int)entry.Score;

                    if (result.TryGetValue(playerName, out var oldScore))
                        result[playerName] = Mathf.Max(oldScore, score);
                    else
                        result[playerName] = score;
                }

                offset += page.Results.Count;
                if (page.Results.Count < limit) break;
            }
        }
        catch (LeaderboardsException e)
        {
            Debug.LogWarning($"[Leaderboard] Fetch error: {e.Reason}");
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }

        return result;
    }

    /*
     * Submit current player's high score
     */
    public static async Task SetNewEntry()
    {
#if !UNITY_EDITOR
	try
		{
			await InitializeLeaderboardServicesAsync();

			PlayerData playerData = SaveManager.LoadGame();
			string name = SaveManager.GetName();
			if (string.IsNullOrWhiteSpace(name))
				name = $"Player_{UnityEngine.Random.Range(1000, 9999)}";

			await AuthenticationService.Instance.UpdatePlayerNameAsync(name);

			int score = Mathf.FloorToInt(playerData.GetHighScore());
			await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardID, score);
		}
		catch (LeaderboardsException e)
		{
			Debug.LogWarning($"[Leaderboard] SetNewEntry failed: {e.Reason}");
		}
		catch (System.Exception e)
		{
			Debug.LogException(e);
		}
#endif
    }

    /*
     * Check if a name exists in the board
     */
    public async Task<bool> CheckForNameAsync(string name)
    {
        var table = await GetLeaderboardAsync();
        return table != null && table.ContainsKey(name);
    }

    /*
     * Redraw UI from the cached leaderboard table
     */
    private void UpdateUI()
    {
        if (leaderboard == null || leaderboard.Count == 0)
        {
            for (int j = 0; j < nameFields.Count; j++)
            {
                if (nameFields[j]) { nameFields[j].text = string.Empty; nameFields[j].color = defaultNameColor; }
            }
            for (int j = 0; j < scoreFields.Count; j++)
            {
                if (scoreFields[j]) scoreFields[j].text = string.Empty;
            }
            if (playerRankField && playerRankField.transform?.parent)
                playerRankField.transform.parent.gameObject.SetActive(false);
            return;
        }

        string playerName = SaveManager.GetName();

        var entries = leaderboard
            .OrderByDescending(kv => kv.Value)
            .ThenBy(kv => kv.Key);

        int displayCount = Mathf.Min(leaderboard.Count,
                                     nameFields != null ? nameFields.Count : 0,
                                     scoreFields != null ? scoreFields.Count : 0);

        for (int j = 0; j < nameFields.Count; j++)
        {
            if (nameFields[j]) { nameFields[j].text = string.Empty; nameFields[j].color = defaultNameColor; }
        }
        for (int j = 0; j < scoreFields.Count; j++)
        {
            if (scoreFields[j]) scoreFields[j].text = string.Empty;
        }

        int idx = 0;
        int rank = 0;
        bool playerFound = false;
        bool playerInRange = false;

        foreach (var kvp in entries)
        {
            rank++;

            if (idx < displayCount)
            {
                if (nameFields[idx])
                {
                    nameFields[idx].text = kvp.Key;
                    nameFields[idx].color = (kvp.Key == playerName) ? playerColor : defaultNameColor;
                }
                if (scoreFields[idx])
                {
                    scoreFields[idx].text = kvp.Value.ToString();
                }
            }

            if (kvp.Key == playerName)
            {
                playerFound = true;
                if (idx < displayCount)
                {
                    playerInRange = true;
                }
                else
                {
                    if (playerRankField) playerRankField.text = rank.ToString();
                    if (playerNameField)
                    {
                        playerNameField.color = playerColor;
                        playerNameField.text = kvp.Key;
                    }
                    if (playerScoreField) playerScoreField.text = kvp.Value.ToString();
                }
            }

            idx++;
        }

        if (playerRankField && playerRankField.transform?.parent)
            playerRankField.transform.parent.gameObject.SetActive(playerFound && !playerInRange);
    }
}

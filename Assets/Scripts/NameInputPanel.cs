using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NameInputPanel : MonoBehaviour
{
    [Header("References")]
    private Animator anim;
    private MainMenuManager mainMenuManager;
    private LeaderboardManager leaderboardManager;

    [Header("UI")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button submitButton;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        
        mainMenuManager = FindFirstObjectByType<MainMenuManager>();
        leaderboardManager = FindFirstObjectByType<LeaderboardManager>();
    }

    private void Update()
    {
        if (inputField.text.IndexOfAny(mainMenuManager.turkishChars) >= 0 || inputField.text.Length < 3)
            submitButton.interactable = false;
        else
            submitButton.interactable = true;
    }

    /*
     * desc: saves player's selected name and default score to the leaderboard
     */
    public async void OnSubmitClicked()
    {
        try
        {
            string name = inputField.text;

            bool containsSameName = await leaderboardManager.CheckForNameAsync(inputField.text);
            if (containsSameName)
                name += "_";

            SaveManager.SaveGame(name);
            Exit();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Submit Name Error: " + e.Message);
        }
    }

    /*
     * desc: brings the name input panel to the scene
     */
    public void Enter()
    {
        anim.SetBool("Open", true);
        mainMenuManager.soundButtonActive = false;
    }

    /*
     * desc: removes the name input panel from the scene
     */
    public void Exit()
    {
        anim.SetBool("Open", false);
        mainMenuManager.soundButtonActive = true;
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform mainMenuContainer;

    [Header("Sound Button")]
    [SerializeField] private Image soundButton;
    [SerializeField] private Sprite noSound;
    [SerializeField] private Sprite lowSound;
    [SerializeField] private Sprite midSound;
    [SerializeField] private Sprite highSound;
    internal bool soundButtonActive = true;

    [Header("Sliders")]
    [SerializeField] private Slider masterVolumeSlider;

    [Header("Panels")]
    [SerializeField] private Animator leaderbaordPanel;
    [SerializeField] private Button leaderboardClose;
    [SerializeField] private NameInputPanel nameInputPanel;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private float moveDistance = 1500f;

    private Vector2 containerDefaultPos;
    private Vector2 buttonDefaultPos;

    public char[] turkishChars = { 'ç', 'ð', 'ý', 'ö', 'þ', 'ü' };
    private bool isTransitioning = false;

    private void Start()
    {
        leaderboardClose.gameObject.SetActive(false);
        SetSoundButtonSprite(masterVolumeSlider.value);

        containerDefaultPos = mainMenuContainer.anchoredPosition;
        buttonDefaultPos = soundButton.rectTransform.anchoredPosition;

        StartCoroutine(AnimateIn());
    }

    private void OnEnable()
    {
        masterVolumeSlider.onValueChanged.AddListener(SetSoundButtonSprite);
    }

    private void OnDisable()
    {
        masterVolumeSlider.onValueChanged.RemoveListener(SetSoundButtonSprite);
    }

    private IEnumerator AnimateIn()
    {
        isTransitioning = true;

        Vector2 containerStart = containerDefaultPos - Vector2.up * moveDistance;
        Vector2 buttonStart = buttonDefaultPos - Vector2.up * moveDistance;

        Vector2 containerTarget = containerDefaultPos;
        Vector2 buttonTarget = buttonDefaultPos;

        mainMenuContainer.anchoredPosition = containerStart;
        soundButton.rectTransform.anchoredPosition = buttonStart;

        float timer = 0f;
        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float t = timer / animationDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            mainMenuContainer.anchoredPosition = Vector2.Lerp(containerStart, containerTarget, smoothT);
            soundButton.rectTransform.anchoredPosition = Vector2.Lerp(buttonStart, buttonTarget, smoothT);

            yield return null;
        }

        mainMenuContainer.anchoredPosition = containerTarget;
        soundButton.rectTransform.anchoredPosition = buttonTarget;

        isTransitioning = false;

        CheckNameInput();
    }

    private void CheckNameInput()
    {
        if (SaveManager.GetName() == "None" || SaveManager.GetName() == "")
            nameInputPanel.Enter();
    }

    private void SetSoundButtonSprite(float volume)
    {
        if (volume <= 0f) soundButton.sprite = noSound;
        else if (volume < 0.35f) soundButton.sprite = lowSound;
        else if (volume < 0.65f) soundButton.sprite = midSound;
        else soundButton.sprite = highSound;
    }

    public void StartGame()
    {
        if (!isTransitioning)
        {
            StartCoroutine(AnimateOutAndLoad("Gameplay"));
        }
    }

    private IEnumerator AnimateOutAndLoad(string sceneName)
    {
        isTransitioning = true;

        Vector2 containerStart = mainMenuContainer.anchoredPosition;
        Vector2 buttonStart = soundButton.rectTransform.anchoredPosition;

        Vector2 containerTarget = containerStart + Vector2.up * moveDistance;
        Vector2 buttonTarget = buttonStart + Vector2.up * moveDistance;

        float timer = 0f;
        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float t = timer / animationDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            mainMenuContainer.anchoredPosition = Vector2.Lerp(containerStart, containerTarget, smoothT);
            soundButton.rectTransform.anchoredPosition = Vector2.Lerp(buttonStart, buttonTarget, smoothT);

            yield return null;
        }

        SceneManager.LoadScene(sceneName);
    }

    public void OpenLeaderboard()
    {
        soundButtonActive = false;
        leaderboardClose.gameObject.SetActive(true);

        leaderbaordPanel.SetBool("Open", true);
    }

    public void CloseLeaderboard()
    {
        soundButtonActive = true;
        leaderboardClose.gameObject.SetActive(false);

        leaderbaordPanel.SetBool("Open", false);
    }
}
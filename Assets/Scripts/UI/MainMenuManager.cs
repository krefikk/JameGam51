using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using TMPro;

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

    [Header("Audio")]
    [SerializeField] private AudioMixer mixer;

    [Header("Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Panels")]
    [SerializeField] private Animator leaderbaordPanel;
    [SerializeField] private Button leaderboardClose;
    [SerializeField] private NameInputPanel nameInputPanel;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private float moveDistance = 1500f;
    [SerializeField] private float startupDelay = 0.5f;

    private Vector2 containerDefaultPos;
    private Vector2 buttonDefaultPos;

    private Vector2 containerStartPos;
    private Vector2 buttonStartPos;

    public char[] turkishChars = { 'ç', 'ð', 'ý', 'ö', 'þ', 'ü' };
    private bool isTransitioning = false;

    private void Awake()
    {
        Time.timeScale = 1f;

        masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
    }

    private void Start()
    {
        SetVisibility(mainMenuContainer, false);
        SetVisibility(soundButton.rectTransform, false);

        containerDefaultPos = mainMenuContainer.anchoredPosition;
        buttonDefaultPos = soundButton.rectTransform.anchoredPosition;

        containerStartPos = containerDefaultPos - Vector2.up * moveDistance;
        buttonStartPos = buttonDefaultPos - Vector2.up * moveDistance;

        mainMenuContainer.anchoredPosition = containerStartPos;
        soundButton.rectTransform.anchoredPosition = buttonStartPos;

        leaderboardClose.gameObject.SetActive(false);
        SetSoundButtonSprite(masterVolumeSlider.value);

        OnSliderValueChange(masterVolumeSlider);
        OnSliderValueChange(musicVolumeSlider);
        OnSliderValueChange(sfxVolumeSlider);

        SetVisibility(mainMenuContainer, true);
        SetVisibility(soundButton.rectTransform, true);

        StartCoroutine(WaitForAndStartAnimateIn(startupDelay));

        CheckNameInput();
    }

    private void SetVisibility(RectTransform uiElement, bool status)
    {
        float targetAlpha = status ? 1f : 0f;

        Image image = uiElement.GetComponent<Image>();
        if (image != null)
        {
            Color newColor = image.color;
            newColor.a = targetAlpha;
            image.color = newColor;
        }

        TextMeshProUGUI text = uiElement.GetComponent<TextMeshProUGUI>();
        if (text != null)
        {
            Color newColor2 = text.color;
            newColor2.a = targetAlpha;
            text.color = newColor2;
        }

        Image[] images = uiElement.GetComponentsInChildren<Image>(true);
        foreach (Image img in images)
        {
            Color newColor = img.color;
            newColor.a = targetAlpha;
            img.color = newColor;
        }

        TextMeshProUGUI[] texts = uiElement.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI txt in texts)
        {
            Color newColor = txt.color;
            newColor.a = targetAlpha;
            txt.color = newColor;
        }
    }

    private IEnumerator WaitForAndStartAnimateIn(float delay)
    {
        yield return null;
        yield return null;
        yield return new WaitForSeconds(delay);
        //StartCoroutine(AnimateIn());
    }

    private void OnEnable()
    {
        masterVolumeSlider.onValueChanged.AddListener(SetSoundButtonSprite);
    }

    private void OnDisable()
    {
        masterVolumeSlider.onValueChanged.RemoveListener(SetSoundButtonSprite);
    }

    public void OnSliderValueChange(Slider slider)
    {
        if (slider == masterVolumeSlider)
        {
            PlayerPrefs.SetFloat("MasterVolume", slider.value);
            ApplyVolume("MasterVolume", slider.value);
        }
        else if (slider == musicVolumeSlider)
        {
            PlayerPrefs.SetFloat("MusicVolume", slider.value);
            ApplyVolume("MusicVolume", slider.value);
        }
        else if (slider == sfxVolumeSlider)
        {
            PlayerPrefs.SetFloat("SFXVolume", slider.value);
            ApplyVolume("SFXVolume", slider.value);
        }

        PlayerPrefs.Save();
    }

    private void ApplyVolume(string exposedParam, float value)
    {
        float dB = value <= 0f ? -80f : Mathf.Log10(value) * 20f;
        mixer.SetFloat(exposedParam, dB);
    }

    private IEnumerator AnimateIn()
    {
        if (isTransitioning)
            yield break;

        isTransitioning = true;

        float timer = 0f;
        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float t = timer / animationDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            mainMenuContainer.anchoredPosition = Vector2.Lerp(containerStartPos, containerDefaultPos, smoothT);
            soundButton.rectTransform.anchoredPosition = Vector2.Lerp(buttonStartPos, buttonDefaultPos, smoothT);

            yield return null;
        }

        mainMenuContainer.anchoredPosition = containerDefaultPos;
        soundButton.rectTransform.anchoredPosition = buttonDefaultPos;

        isTransitioning = false;      
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
            StartCoroutine(AnimateOutAndLoadAsync("Gameplay"));
        }
    }

    private IEnumerator AnimateOutAndLoadAsync(string sceneName)
    {
        isTransitioning = true;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

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

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;
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
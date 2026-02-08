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

    [Header("Sliders")]
    [SerializeField] private Slider masterVolumeSlider;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private float moveDistance = 1500f;

    private bool isTransitioning = false;

    private void Start()
    {
        SetSoundButtonSprite(masterVolumeSlider.value);
    }

    private void OnEnable()
    {
        masterVolumeSlider.onValueChanged.AddListener(SetSoundButtonSprite);
    }

    private void OnDisable()
    {
        masterVolumeSlider.onValueChanged.RemoveListener(SetSoundButtonSprite);
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

        Vector2 containerStartPos = mainMenuContainer.anchoredPosition;
        Vector2 buttonStartPos = soundButton.rectTransform.anchoredPosition;

        Vector2 containerTargetPos = containerStartPos + Vector2.up * moveDistance;
        Vector2 buttonTargetPos = buttonStartPos + Vector2.up * moveDistance;

        float timer = 0f;

        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float t = timer / animationDuration;

            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            mainMenuContainer.anchoredPosition = Vector2.Lerp(containerStartPos, containerTargetPos, smoothT);
            soundButton.rectTransform.anchoredPosition = Vector2.Lerp(buttonStartPos, buttonTargetPos, smoothT);

            yield return null;
        }

        SceneManager.LoadScene(sceneName);
    }
}
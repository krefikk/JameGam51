using UnityEngine;
using UnityEngine.UI;

public class SmartAudioMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform mainMenuContainer;
    [SerializeField] private RectTransform sidePanelContainer;
    [SerializeField] private RectTransform soundButtonTrigger;
    [SerializeField] private RectTransform visualButton;

    [Header("Settings")]
    [SerializeField] private float hoverDelay = 0.5f;
    [SerializeField] private float animationSpeed = 10f;

    [Header("Positions")]
    [SerializeField] private float menuShiftAmount = 200f;
    [SerializeField] private float sidePanelOpenX = 0f;
    [SerializeField] private float sidePanelClosedX = 400f;
    [SerializeField] private float buttonSlideUpAmount = 150f;

    [Header("Main Menu Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button leaderboardButton;
    [SerializeField] private Button exitButton;

    private bool isMenuOpen = false;
    private float hoverTimer = 0f;

    private Vector2 menuTargetPos;
    private Vector2 sideTargetPos;
    private Vector2 buttonTargetPos;

    private Vector2 menuDefaultPos;
    private Vector2 buttonDefaultPos;

    private Camera uiCamera;

    private void Start()
    {
        menuDefaultPos = mainMenuContainer.anchoredPosition;
        buttonDefaultPos = visualButton.anchoredPosition;

        menuTargetPos = menuDefaultPos;
        buttonTargetPos = buttonDefaultPos;
        sideTargetPos = new Vector2(sidePanelClosedX, sidePanelContainer.anchoredPosition.y);

        sidePanelContainer.anchoredPosition = sideTargetPos;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = canvas.worldCamera;
    }

    private void Update()
    {
        bool mouseOnButton = IsMouseOverUI(soundButtonTrigger);
        bool mouseOnPanel = IsMouseOverUI(sidePanelContainer);

        if (!isMenuOpen)
        {
            if (mouseOnButton)
            {
                hoverTimer += Time.deltaTime;
                if (hoverTimer >= hoverDelay)
                {
                    OpenMenu();
                }
            }
            else
            {
                hoverTimer = 0f;
            }
        }
        else
        {
            if (!mouseOnButton && !mouseOnPanel)
            {
                CloseMenu();
            }
        }

        mainMenuContainer.anchoredPosition = Vector2.Lerp(mainMenuContainer.anchoredPosition, menuTargetPos, Time.deltaTime * animationSpeed);
        sidePanelContainer.anchoredPosition = Vector2.Lerp(sidePanelContainer.anchoredPosition, sideTargetPos, Time.deltaTime * animationSpeed);
        visualButton.anchoredPosition = Vector2.Lerp(visualButton.anchoredPosition, buttonTargetPos, Time.deltaTime * animationSpeed);
    }

    private void OpenMenu()
    {
        isMenuOpen = true;

        startButton.interactable = false;
        leaderboardButton.interactable = false;
        exitButton.interactable = false;

        menuTargetPos = new Vector2(menuDefaultPos.x - menuShiftAmount, menuDefaultPos.y);
        sideTargetPos = new Vector2(sidePanelOpenX, sidePanelContainer.anchoredPosition.y);
        buttonTargetPos = new Vector2(buttonDefaultPos.x, buttonDefaultPos.y + buttonSlideUpAmount);
    }

    private void CloseMenu()
    {
        isMenuOpen = false;
        hoverTimer = 0f;

        startButton.interactable = true;
        leaderboardButton.interactable = true;
        exitButton.interactable = true;

        menuTargetPos = menuDefaultPos;
        sideTargetPos = new Vector2(sidePanelClosedX, sidePanelContainer.anchoredPosition.y);
        buttonTargetPos = buttonDefaultPos;
    }

    private bool IsMouseOverUI(RectTransform target)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(target, Input.mousePosition, uiCamera);
    }
}
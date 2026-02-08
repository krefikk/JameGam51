using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;

public class MainMenuAudio : MonoBehaviour
{
    [Header("MX")]
    [SerializeField] private AudioClip mainMenuMX;
    [SerializeField] private float crossfadeDuration = 2f;


    [Header("UI SFX")]
    [SerializeField] private AudioClip buttonHover;
    [SerializeField] private AudioClip buttonClick;

    private void Start()
    {
        if (AudioManager.master != null && AudioManager.master.SFX != null)
        {
            AudioManager.master.SFX.StopAll();
        }

    StartCoroutine(PlayMX());
    SetupButtons();
    }

    private IEnumerator PlayMX()
    {
        yield return null;

        if (mainMenuMX != null)
        {
            AudioManager.master.MX.PlayWithCrossfade(mainMenuMX, crossfadeDuration);
        }
    }

    private void SetupButtons()
{
    UnityEngine.UI.Button[] buttons = FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None);

    foreach (var button in buttons)
    {
        Hover(button.gameObject);

        if (buttonClick != null)
        {
            button.onClick.AddListener(() => PlayClickSound());
        }
    }
}

    private void Hover(GameObject buttonObject)
    {
        EventTrigger trigger = buttonObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = buttonObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((data) => { PlayHoverSound(); });
        trigger.triggers.Add(entry);
    }

    private void PlayHoverSound()
    {
        if (buttonHover != null && AudioManager.master != null)
        {
            AudioManager.master.SFX.Play(buttonHover, AudioManager.master.UIMixer);
        }
    }

    private void PlayClickSound()
    {
        if (buttonClick != null && AudioManager.master!= null)
        {
            AudioManager.master.SFX.Play(buttonClick, AudioManager.master.UIMixer );
        }
    }
}
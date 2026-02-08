using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;


public class GameplayAudio : MonoBehaviour
{
    [Header("MX")]
    [SerializeField] private AudioClip gameplayMX;

    [Header("Crossfade Settings")]
    [SerializeField] private float crossfadeDuration = 2f;

    [Header("SFX")]
    [SerializeField] private AudioClip bG;
    [SerializeField] private AudioClip playerSwim;
    [SerializeField] private AudioClip playerDead;
    [SerializeField] private AudioClip[] playerDamage;
    [SerializeField] private AudioClip fishSwim;
    [SerializeField] private AudioClip[] bubbleCollect;
    [SerializeField] private AudioClip bomb;
    [SerializeField] private AudioClip heartbeat;

    [Header("UI SFX")]
    [SerializeField] private AudioClip buttonHover;
    [SerializeField] private AudioClip buttonClick;

    private Dictionary<Fish, AudioSource> fishAudioSources = new Dictionary<Fish, AudioSource>();
    private int lastDamageIndex = -1; // ← AGREGAR

    private void Start()
    {
        StartCoroutine(PlayMX());
        AudioManager.master.SFX.PlayLoop(bG);
        SetupButtons();
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

    private IEnumerator PlayMX()
    {
        yield return null;

        if (gameplayMX != null && AudioManager.master != null)
        {
            AudioManager.master.MX.PlayWithCrossfade(gameplayMX, crossfadeDuration);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (AudioManager.master.SFX.pausableSource == null)
            {
                AudioManager.master.SFX.PlayPausable(playerSwim);
                AudioManager.master.SFX.UnPausePausable();
            }
            else
            {
                AudioManager.master.SFX.UnPausePausable();
            }
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            AudioManager.master.SFX.PausePausable();
        }
    }

    public void PlayPlayerDamage()
    {
        if (playerDamage == null || playerDamage.Length == 0) return;
        if (AudioManager.master == null) return;

        int randomIndex;

        if (playerDamage.Length > 1)
        {
            do
            {
                randomIndex = Random.Range(0, playerDamage.Length);
            }
            while (randomIndex == lastDamageIndex);

            lastDamageIndex = randomIndex;
        }
        else
        {
            randomIndex = 0;
        }

        AudioManager.master.SFX.Play(playerDamage[randomIndex]);
    }
}
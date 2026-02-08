using NaughtyAttributes;
using System;
using System.Collections;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("References")]
    private PlayerState state;
    private SpriteRenderer sr;
    private CameraShake mainCam;

    [Header("Health")]
    [SerializeField][Range(1, 10)] public int maxHealth;
    [ShowNonSerializedField] internal int health;
    public int Health => health;

    [Header("Oxygen")]
    [SerializeField] [Range(0.05f, 1f)] private float oxygenConsumptionRate = 0.2f;
    [SerializeField] private float noOxygenDamageTime = 1.2f;
    [SerializeField][Range(1, 100)] public float maxOxygen;
    [ShowNonSerializedField] internal float oxygen;
    private float noOxygenDamageTimer = 0f;
    public float Oxygen => oxygen;

    [Header("Invincibility")]
    [SerializeField] private float invincibilityTime = 2f;
    [SerializeField] private float hitFlashSpeed = 0.1f;
    private float invincibilityTimer;

    [Header("Time Scale")]
    [SerializeField][Range(0f, 1f)] private float slowMotionScale = 0f;
    [SerializeField] private float restoreTimeSpeed = 5f;
    private bool restoreTime;

    [Header("Speaker")]
    [SerializeField] public Transform speakerPosition;

    [Header("Actions")]
    public Action onDiverDie;
    public Action<bool> onDiverTakeDamage; // drown damage: true, hit damage: false
    public Action onDiverGetBubble;
    public Action onDiverExplodedMine;

    ///
    private GameplayAudio gameplayAudio;
    ///

    private void Awake()
    {
        state = GetComponent<PlayerState>();
        sr = GetComponent<SpriteRenderer>();
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraShake>();

        health = maxHealth;
        oxygen = maxOxygen;
    }

    private void Update()
    {
        RestoreTimeScale();
        if (state.dead)
            return;

        oxygen -= Time.deltaTime * oxygenConsumptionRate;
        if (oxygen <= 0)
        {
            noOxygenDamageTimer += Time.deltaTime;
            if (noOxygenDamageTimer >= noOxygenDamageTime)
            {
                TakeDamage(1, true);
                noOxygenDamageTimer = 0f;
            }    
        }
        else
            noOxygenDamageTimer = 0f;

        UIManager.Instance.UpdateOxygenBar(maxOxygen, oxygen);
        
        FlashWhileInvincible();

        if (state.invinsible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
            {
                state.invinsible = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Fish"))
        {
            TakeDamage();
        }

        if (collision.CompareTag("AirBubble"))
        {
            onDiverGetBubble?.Invoke();
            AirBubble bubble = collision.GetComponent<AirBubble>();

            if (!bubble.lostOxygen)
            {
                Popup.Create(transform.position, bubble.GetOxygenAmount(), bubble.GetOxygenAmount() >= 25 ? true : false);
                AddOxygen(bubble.GetOxygenAmount());
                bubble.lostOxygen = true;
            } 
            bubble.OnCollideWithDiver();
        }
    }

    public void TakeDamage(int amount = 1, bool drowning = false)
    {
        if (state.invinsible && !drowning)
            return;

        state.invinsible = true;
        invincibilityTimer = invincibilityTime;

        UIManager.Instance.DecreaseHealthBar(amount);

        mainCam.Shake();
        Popup.Create(transform.position, -1 * amount, false);

        health -= amount;
        onDiverTakeDamage?.Invoke(drowning);

        if (!drowning && health >= 0)
            HitStopTime(0.5f);

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        state.dead = true;
        health = 0;
        onDiverDie?.Invoke();

        //
        GameplayAudio gameplayAudio = FindFirstObjectByType<GameplayAudio>();
        if (gameplayAudio != null)
        {
            gameplayAudio.PlayPlayerDead();
        }
        //

    }

    private void HitStopTime(float delay)
    {
        Time.timeScale = slowMotionScale;

        if (delay > 0)
        {
            StopCoroutine(StartTimeAgain(delay));
            StartCoroutine(StartTimeAgain(delay));
        }
        else
            restoreTime = true;
    }

    private IEnumerator StartTimeAgain(float delay)
    {
        restoreTime = true;
        yield return new WaitForSeconds(delay);
    }

    private void RestoreTimeScale()
    {
        if (restoreTime)
        {
            if (Time.timeScale < 1)
                Time.timeScale += Time.deltaTime * restoreTimeSpeed;
            else
            {
                Time.timeScale = 1f;
                restoreTime = false;
            }  
        }
    }

    private void FlashWhileInvincible()
    {
        sr.material.color = state.invinsible ? Color.Lerp(Color.white, Color.black, Mathf.PingPong(Time.time * hitFlashSpeed, 1.0f)) : Color.white;
    }

    public void AddOxygen(float amount)
    {
        oxygen += amount;
        if (oxygen > maxOxygen)
            oxygen = maxOxygen;
    }
}

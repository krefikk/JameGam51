using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class Collectible : MonoBehaviour
{
    [Header("References")]
    protected Animator anim;

    [Header("General Properties")]
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float waveFrequency = 2f;
    [SerializeField] protected float waveAmplitude = 1f;
    [SerializeField] protected Vector2 direction; // For Y: 1 for upwards, -1 for downwards

    protected float waveTimer;

    protected void Awake()
    {
        anim = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        waveTimer = Random.Range(0f, 100f);
        direction = direction.normalized;
    }

    protected void Update()
    {
        Move();
    }

    public void Move()
    {
        waveTimer += Time.deltaTime * waveFrequency;

        Vector2 perpendicularDir = new Vector2(-direction.y, direction.x);
        Vector2 waveOffset = perpendicularDir * Mathf.Sin(waveTimer) * waveAmplitude;
        Vector3 moveVector = (direction + waveOffset).normalized;

        transform.position += moveVector * moveSpeed * Time.deltaTime;
    }

    protected IEnumerator DestroyCollectible(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    protected void ResetMoveSpeed(float deceleration = 2f)
    {
        StopCoroutine(ResetMoveSpeedCoroutine(deceleration));
        StartCoroutine(ResetMoveSpeedCoroutine(deceleration));
    }

    private IEnumerator ResetMoveSpeedCoroutine(float deceleration)
    {
        while (moveSpeed > 0f)
        {
            moveSpeed = Mathf.MoveTowards(moveSpeed, 0f, deceleration * Time.deltaTime);
            yield return null;
        }

        moveSpeed = 0f;
    }

    public abstract void OnCollideWithDiver();

    public abstract void OnDie();
}

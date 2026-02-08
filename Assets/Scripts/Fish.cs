using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.Processors;

public class Fish : MonoBehaviour
{
    [Header("References")]
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;
    private PlayerController player;

    [Header("Flags")]
    private bool dead = false;
    private bool canMove = false;

    [Header("Properties")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float turnSpeed = 2.5f;
    [SerializeField] private float targetDistanceThreshold = 4f;

    [Header("Constraints")]
    [Range(0, 180)][SerializeField] private float maxTurnAngle = 90f;

    [Header("Swimming")]
    [SerializeField] public float waveFrequency = 2f;
    [SerializeField] public float waveAmplitude = 1f;
    [SerializeField] private Vector3 direction = Vector3.left;

    [Header("Effects")]
    [SerializeField] private ParticleSystem bloodParticles;
    [SerializeField] private int bloodAmount = 5;
    
    private Vector3 startDirection;
    private float waveTimer;
    private Vector3? targetPosition = null;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        player = FindFirstObjectByType<PlayerController>();
    }

    void Start()
    {
        waveTimer = Random.Range(0f, 100f);

        direction = direction.normalized;
        startDirection = direction;

        if (direction != Vector3.zero)
        {
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(targetAngle, Vector3.forward);

            if (Mathf.Abs(targetAngle) > 90)
                sr.flipY = true;
            else
                sr.flipY = false;
        }
    }

    void Update()
    {
        if (dead || !canMove)
            return;

        if (targetPosition.HasValue)
        {
            ChaseState();
        }
        else
        {
            Vector3? heardSound = NoiseManager.Instance.CheckForNoise(transform.position);

            if (heardSound.HasValue)
            {
                targetPosition = heardSound.Value;
            }

            IdleState();
        }

        if (Vector2.Distance(player.transform.position, transform.position) <= targetDistanceThreshold)
            anim.SetBool("NearTarget", true);
        else
            anim.SetBool("NearTarget", false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            anim.SetTrigger("Eat");
            anim.SetBool("Ate", true);
            bloodParticles.Emit(bloodAmount);
            // eat sound
        }
    }

    void ChaseState()
    {
        Vector3 directionToTarget = (targetPosition.Value - transform.position).normalized;
        RotateTowards(directionToTarget);

        transform.position += transform.right * moveSpeed * Time.deltaTime;

        float distance = Vector3.Distance(transform.position, targetPosition.Value);
        if (distance < 0.5f)
        {
            StopChasing();
            return;
        }

        float angleDifference = Vector3.Angle(startDirection, transform.right);

        if (angleDifference > maxTurnAngle)
        {
            StopChasing();
        }
    }

    void IdleState()
    {
        waveTimer += Time.deltaTime * waveFrequency;

        Vector3 perpendicularDir = new Vector3(-direction.y, direction.x, 0);
        Vector3 waveOffset = perpendicularDir * Mathf.Sin(waveTimer) * waveAmplitude;

        Vector3 targetDir = (direction + waveOffset).normalized;

        RotateTowards(targetDir);

        transform.position += transform.right * moveSpeed * Time.deltaTime;
    }

    void StopChasing()
    {
        direction = transform.right;
        targetPosition = null;
    }

    void RotateTowards(Vector3 dir)
    {
        if (dir == Vector3.zero) return;

        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(targetAngle, Vector3.forward);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
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

    public void Die(float damageAmount = 1)
    {
        if (dead)
            return;

        dead = true;
        ResetMoveSpeed();
        Popup.Create(transform.position, -1, false);
        rb.gravityScale = 0.5f;
        anim.SetBool("Die", true);
        DestroyFish(0.5f);
    }

    private IEnumerator DestroyFish(float delay, float duration = 2f)
    {
        yield return new WaitForSeconds(delay);

        Color startColor = sr.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float newAlpha = Mathf.Lerp(startColor.a, 0f, elapsed / duration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);

            yield return null;
        }

        sr.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        Destroy(gameObject);
    }

    public void StartMove()
    {
        canMove = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, targetDistanceThreshold);
    }
}
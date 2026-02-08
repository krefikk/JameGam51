using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    private Rigidbody2D rb;
    private PlayerState state;
    private PlayerStats stats;

    [Header("Input")]
    [SerializeField] private InputActionReference thrustInput;

    [Header("Force")]
    [SerializeField] private float thrustForce = 5f;
    [SerializeField][Range(0.01f, 1f)] private float frictionCoef = 0.9f;

    [Header("Rotation")]
    [SerializeField] private float maxRotateSpeed = 150f;
    [SerializeField] private float rotationAcceleration = 300f; // Durma ve baþlama hýzý
    private float currentRotateSpeed = 0f;
    private float targetRotateSpeed = 0f;
    private int rotationDirection = 1; // Sabit yön (Saat yönü tersi için 1, saat yönü için -1 yapabilirsin)

    [Header("Noise Generation")]
    [SerializeField] private float minNoiseRadius = 2f;
    [SerializeField] private float maxNoiseRadius = 15f;
    [SerializeField] private float noiseChargeSpeed = 5f;
    [SerializeField] private float noiseDuration = 0.1f;
    private float currentNoiseRadius;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        state = GetComponent<PlayerState>();
        stats = GetComponent<PlayerStats>();
    }

    private void OnEnable()
    {
        stats.onDiverTakeDamage += HandleDamage;
        stats.onDiverDie += OnDie;
    }

    private void OnDisable()
    {
        stats.onDiverTakeDamage -= HandleDamage;
        stats.onDiverDie -= OnDie;
    }

    private void Start()
    {
        rotationDirection = Random.Range(0, 2) == 0 ? 1 : -1;

        currentNoiseRadius = minNoiseRadius;
        targetRotateSpeed = maxRotateSpeed * rotationDirection;
    }

    private void Update()
    {
        HandleInputAndRotation();
    }

    private void FixedUpdate()
    {
        if (!state.thrusting)
        {
            rb.linearVelocity = rb.linearVelocity * (1 - frictionCoef);
            if (rb.linearVelocity.magnitude < 0.1f)
                rb.linearVelocity = Vector2.zero;
        }

        Rotate();
    }

    private void HandleInputAndRotation()
    {
        currentRotateSpeed = Mathf.MoveTowards(currentRotateSpeed, targetRotateSpeed, rotationAcceleration * Time.deltaTime);

        if (state.dead)
            return;

        if (thrustInput.action.IsPressed())
        {
            state.thrusting = true;
            targetRotateSpeed = 0f;
            ApplyForce();
            GenerateContinuousNoise();
        }
        else
        {
            state.thrusting = false;
            currentNoiseRadius = minNoiseRadius;
            targetRotateSpeed = maxRotateSpeed * rotationDirection;
        }
    }

    private void GenerateContinuousNoise()
    {
        currentNoiseRadius += noiseChargeSpeed * Time.deltaTime;
        currentNoiseRadius = Mathf.Clamp(currentNoiseRadius, minNoiseRadius, maxNoiseRadius);
        NoiseManager.Instance.RegisterNoise(transform.position, currentNoiseRadius, noiseDuration);
    }

    private void ApplyForce()
    {
        rb.AddForce(-1 * transform.right * thrustForce, ForceMode2D.Impulse);
    }

    private void Rotate()
    {
        rb.angularVelocity = 0;
        transform.Rotate(0, 0, currentRotateSpeed * Time.fixedDeltaTime);
    }

    private void ChangeRotationDirection()
    {
        rotationDirection *= -1;
        targetRotateSpeed = maxRotateSpeed * rotationDirection;
    }

    private void HandleDamage(bool isDrowning)
    {
        if (!isDrowning)
            ChangeRotationDirection();
    }

    private void OnDie()
    {
        targetRotateSpeed = 0f;
    }
}
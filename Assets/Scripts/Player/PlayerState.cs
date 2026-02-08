using UnityEngine;
using NaughtyAttributes;

public class PlayerState : MonoBehaviour
{
    [Header("References")]
    private Animator anim;
    private InGameManager inGameManager;

    [ShowNonSerializedField] internal bool thrusting = false;
    [ShowNonSerializedField] internal bool invinsible = false;
    [ShowNonSerializedField] internal bool dead = false;

    [ShowNonSerializedField] internal int bubblesPopped = 0;
    [ShowNonSerializedField] internal int minesExploded = 0;

    public int BubblesPopped => bubblesPopped;
    public int MinesExploded => minesExploded;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        inGameManager = FindFirstObjectByType<InGameManager>();
    }

    private void OnEnable()
    {
        GetComponent<PlayerStats>().onDiverExplodedMine += IncreaseMinesExploded;
        GetComponent<PlayerStats>().onDiverGetBubble += IncreaseBubblesPopped;
        GetComponent<PlayerStats>().onDiverDie += DieAnimation;
    }

    private void OnDisable()
    {
        GetComponent<PlayerStats>().onDiverExplodedMine -= IncreaseMinesExploded;
        GetComponent<PlayerStats>().onDiverGetBubble -= IncreaseBubblesPopped;
        GetComponent<PlayerStats>().onDiverDie -= DieAnimation;
    }

    private void Update()
    {
        if (dead || inGameManager.paused)
            return;

        anim.SetBool("Thrust", thrusting);
    }

    private void IncreaseBubblesPopped()
    {
        if (dead) return;

        bubblesPopped++;
    }

    private void IncreaseMinesExploded()
    {
        if (dead) return;

        minesExploded++;
    }

    private void DieAnimation()
    {
        anim.SetBool("Die", true);

        GameObject speakerPrefab = Resources.Load<GameObject>("Speaker");
        GameObject speaker = Instantiate(speakerPrefab, GetComponent<PlayerStats>().speakerPosition.position, transform.rotation);
        speaker.GetComponent<Rigidbody2D>().AddForce(new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)) * 3f);

        GetComponent<Rigidbody2D>().gravityScale = 0.15f;
    }
}

using UnityEngine;
using NaughtyAttributes;

public class PlayerState : MonoBehaviour
{
    [Header("References")]
    private Animator anim;

    [ShowNonSerializedField] internal bool thrusting = false;
    [ShowNonSerializedField] internal bool invinsible = false;

    [ShowNonSerializedField] internal int bubblesPopped = 0;
    [ShowNonSerializedField] internal int minesExploded = 0;

    public int BubblesPopped => bubblesPopped;
    public int MinesExploded => minesExploded;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        GetComponent<PlayerStats>().onDiverExplodedMine += IncreaseMinesExploded;
        GetComponent<PlayerStats>().onDiverGetBubble += IncreaseBubblesPopped;
    }

    private void OnDisable()
    {
        GetComponent<PlayerStats>().onDiverExplodedMine -= IncreaseMinesExploded;
        GetComponent<PlayerStats>().onDiverGetBubble -= IncreaseBubblesPopped;
    }

    private void Update()
    {
        anim.SetBool("Thrust", thrusting);
    }

    private void IncreaseBubblesPopped()
    {
        bubblesPopped++;
    }

    private void IncreaseMinesExploded()
    {
        minesExploded++;
    }
}

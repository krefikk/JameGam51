using System.Collections;
using UnityEngine;

public class Mine : Collectible
{
    [Header("Mine Properties")]
    [SerializeField] protected int damage = 1;
    [SerializeField] protected float explosionTime = 2f;
    [SerializeField] protected float explosionRadius = 1f;

    protected float explosionTimer;
    internal bool explosionStarted = false;
    internal bool exploded = false;

    protected override void Start()
    {
        base.Start();
        if (damage >= 2)
            anim.SetBool("Dangerous", true);
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Fish"))
        {
            if (!explosionStarted)
            {
                ResetMoveSpeed();
                StartCoroutine(StartExplosion());
            } 
        }

        if (collision.CompareTag("Player"))
        {
            OnCollideWithDiver();
        }
    }

    public override void OnCollideWithDiver()
    {
        if (!explosionStarted)
        {
            ResetMoveSpeed();
            StartCoroutine(StartExplosion());
        }
    }

    public override void OnDie()
    {
        StartCoroutine(DestroyCollectible(1f));
    }

    private IEnumerator StartExplosion()
    {
        explosionStarted = true;
        anim.SetBool("StartExplosion", true);
        explosionTimer = explosionTime;

        while (explosionTimer > 0)
        {
            explosionTimer -= Time.deltaTime;
            yield return null;
        }

        exploded = true;
        anim.SetTrigger("Explode");
        yield return new WaitForSeconds(0.3f);

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D hit in hitColliders)
        {
            if (hit.TryGetComponent(out PlayerStats playerStats))
            {
                playerStats.TakeDamage(damage);
                playerStats.onDiverExplodedMine?.Invoke();
            } 

            else if (hit.TryGetComponent(out Fish fish))
                fish.Die(damage);

            else if (hit.TryGetComponent(out AirBubble bubble))
                bubble.Die();
        }

        OnDie();
    }

    public void SetDamage(int value)
    {
        damage = value;
        if (damage >= 2)
            anim.SetBool("Dangerous", true);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}

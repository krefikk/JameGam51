using System.Collections;
using UnityEngine;

public class AirBubble : Collectible
{
    [Header("Air Bubble Properties")]
    [SerializeField] protected float oxygenAmount = 10;
    internal bool lostOxygen = false;

    protected override void Start()
    {
        base.Start();
        if (oxygenAmount >= 25)
            anim.SetBool("Gold", true);
    }

    public override void OnCollideWithDiver()
    {
        Die();
        OnDie();
    }

    public override void OnDie()
    {
        StartCoroutine(DestroyCollectible(2f));
    }

    public void Die()
    {
        ResetMoveSpeed();
        anim.SetBool("Die", true);
        // Sound effect
    }

    public float GetOxygenAmount()
    {
        return oxygenAmount;
    }

    public void SetOxygenAmount(float value)
    {
        oxygenAmount = value;
        if (oxygenAmount >= 25)
            anim.SetBool("Gold", true);
    }
}

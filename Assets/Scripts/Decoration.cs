using System.Collections;
using UnityEngine;

public class Decoration : MonoBehaviour
{
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        float random = Random.Range(0.25f, 2f);
        StartCoroutine(WaitAndResetAnimation(random));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Fish") || collision.CompareTag("Player"))
            anim.SetTrigger("Collided");
    }

    private IEnumerator WaitAndResetAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);
        anim.Play(anim.GetCurrentAnimatorStateInfo(0).fullPathHash, -1, 0f);
    }
}

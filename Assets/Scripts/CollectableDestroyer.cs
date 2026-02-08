using UnityEngine;

public class CollectableDestroyer : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Collectible>() != null)
            Destroy(collision.gameObject);
    }
}

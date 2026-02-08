using UnityEngine;

public class FishDestroyer : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Fish"))
            Destroy(collision.gameObject);
    }
}

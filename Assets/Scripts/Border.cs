using UnityEngine;

public class Border : MonoBehaviour
{
    private CameraShake mainCam;
    private float shakeTime = 5f;
    private float shakeTimer = 0f;
    private bool shaked = false;

    private void Awake()
    {
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraShake>();
    }

    private void Update()
    {
        if (shaked)
            shakeTimer += Time.deltaTime;

        if (shakeTimer >= shakeTime)
            shaked = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (shaked)
            return;

        mainCam.Shake(0.1f, 0.05f, true, true);
        shaked = true;
    }
}

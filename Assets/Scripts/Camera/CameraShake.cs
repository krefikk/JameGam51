using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Vector3 originalPosition;
    private bool shaking = false;

    [Header("Properties")]
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeSeverity = 1f;
    [SerializeField] private bool verticalShaking = true;
    [SerializeField] private bool horizontalShaking = true;

    public void Shake()
    {
        if (shaking)
            return;

        StartCoroutine(ShakeCamera(shakeDuration, shakeSeverity, verticalShaking, horizontalShaking));
    }

    public void Shake(float duration, float severity, bool vertical, bool horizontal)
    {
        if (shaking)
            return;

        StartCoroutine(ShakeCamera(duration, severity, vertical, horizontal));
    }

    private IEnumerator ShakeCamera(float duration, float severity, bool vertical, bool horizontal)
    {
        shaking = true;
        originalPosition = transform.localPosition;
        float time = duration;

        while (time > 0)
        {
            Vector3 shakeOffset = Vector3.zero;

            if (vertical)
                shakeOffset.y = Random.Range(-1f, 1f) * severity;
            if (horizontal)
                shakeOffset.x = Random.Range(-1f, 1f) * severity;

            transform.localPosition = originalPosition + shakeOffset;
            time -= Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
        shaking = false;
    }
}

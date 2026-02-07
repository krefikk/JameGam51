using UnityEngine;
using TMPro;

public class Popup : MonoBehaviour
{
    private static Popup prefab;

    private TextMeshPro textMesh;
    private float disappearTimer;
    private Color textColor;
    private Vector3 moveVector;

    private const float DISAPPEAR_TIMER_MAX = 1f;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    public static Popup Create(Vector3 position, float amount, bool isCritical)
    {
        if (prefab == null)
            prefab = Resources.Load<Popup>("Popup");

        Vector3 spawnPosition = position + new Vector3(-0.5f, 0.8f, 0);
        spawnPosition += new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.1f, 0.1f), 0);

        Transform popupTransform = Instantiate(prefab.transform, spawnPosition, Quaternion.identity);

        Popup popup = popupTransform.GetComponent<Popup>();
        popup.Setup(amount, isCritical);

        return popup;
    }

    public void Setup(float amount, bool isCritical)
    {
        textMesh.text = "";

        if (amount > 0)
            textMesh.text += "+";
        textMesh.text += amount.ToString("0.#");

        if (amount <= 0)
        {
            if (!isCritical)
            {
                // Normal (Red)
                textMesh.fontSize = 3;
                textColor = new Color(0.533f, 0.03f, 0.03f, 1f);
            }
            else
            {
                // Critical
                textMesh.fontSize = 5;
                textColor = new Color(1f, 0.8f, 0.2f, 1f);
                textMesh.text += "!";
            }
        }
        else
        {
            if (!isCritical)
            {
                // Gray
                textMesh.fontSize = 3;
                textColor = new Color(0.91f, 0.91f, 0.91f, 1f);
            }
            else
            {
                // Gold
                textMesh.fontSize = 5;
                textColor = new Color(1f, 0.84f, 0f, 1f);
                textMesh.text += "!";
            }
        }

        textMesh.color = textColor;
        disappearTimer = DISAPPEAR_TIMER_MAX;

        moveVector = new Vector3(0.5f, 1f) * 5f;
    }

    private void Update()
    {
        transform.position += moveVector * Time.deltaTime;
        moveVector -= moveVector * 8f * Time.deltaTime;

        if (disappearTimer > DISAPPEAR_TIMER_MAX * 0.5f)
        {
            float increaseScaleAmount = 1f;
            transform.localScale += Vector3.one * increaseScaleAmount * Time.deltaTime;
        }
        else
        {
            float decreaseScaleAmount = 1f;
            transform.localScale -= Vector3.one * decreaseScaleAmount * Time.deltaTime;
        }

        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            float disappearSpeed = 3f;
            textColor.a -= disappearSpeed * Time.deltaTime;
            textMesh.color = textColor;

            if (textColor.a < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
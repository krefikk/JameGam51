using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class NoiseManager : MonoBehaviour
{
    public static NoiseManager Instance;

    public class SoundBubble
    {
        public Vector3 center;
        public float radius;
        public float duration;
        public float createdAt;
        public bool IsExpired => Time.time > (createdAt + duration);

        public bool IsPositionInside(Vector3 position)
        {
            return Vector3.Distance(position, center) <= radius;
        }
    }

    private List<SoundBubble> activeSounds = new List<SoundBubble>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        for (int i = activeSounds.Count - 1; i >= 0; i--)
        {
            if (activeSounds[i].IsExpired)
            {
                activeSounds.RemoveAt(i);
            }
        }
    }

    public void RegisterNoise(Vector3 pos, float radius, float time)
    {
        var bubble = new SoundBubble
        {
            center = pos,
            radius = radius,
            duration = time,
            createdAt = Time.time
        };
        activeSounds.Add(bubble);
    }

    // Returns loudest or nearest noise given listener can hear
    public Vector3? CheckForNoise(Vector3 listenerPos)
    {
        var audibleSounds = activeSounds.Where(s => s.IsPositionInside(listenerPos)).ToList();

        if (audibleSounds.Count == 0) return null;
        var nearestSound = audibleSounds.OrderBy(s => Vector3.Distance(listenerPos, s.center)).First();

        return nearestSound.center;
    }

    private void OnDrawGizmos()
    {
        if (activeSounds == null) return;
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        foreach (var s in activeSounds)
        {
            Gizmos.DrawSphere(s.center, s.radius);
        }
    }
}
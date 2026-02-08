using UnityEngine;
using UnityEngine.Audio;

public class SFXManager : MonoBehaviour
{
    private AudioSource[] audioSourcePool;
    private int currentPoolIndex = 0;
    private bool isInitialized = false;

    public AudioSource pausableSource;
    public AudioClip pausableClip;


    private void InitializePool()
    {
        if (isInitialized) return;

        int poolSize = AudioManager.master.PoolSize;
        audioSourcePool = new AudioSource[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            GameObject sourceObject = new GameObject($"PooledAudioSource_SFX_{i}");
            sourceObject.transform.SetParent(transform);
            audioSourcePool[i] = sourceObject.AddComponent<AudioSource>();
            audioSourcePool[i].playOnAwake = false;
        }

        isInitialized = true;
    }

    private AudioSource GetAvailableSource()
    {
        AudioSource source = audioSourcePool[currentPoolIndex];
        currentPoolIndex = (currentPoolIndex + 1) % audioSourcePool.Length;
        return source;
    }


    public void Play(AudioClip clip)
    {
        Play(clip, AudioManager.master.SFXMixer);
    }

    public void Play(AudioClip clip, AudioMixerGroup mixer)
    {
        if (clip == null) return;
        if (!isInitialized) InitializePool();

        AudioSource source = GetAvailableSource();
        source.outputAudioMixerGroup = mixer;
        source.clip = clip;
        source.loop = false;
        source.spatialBlend = 0f; // 2D
        source.Play();
    }

    public void Play(AudioClip clip, Vector3 position)
    {
        Play(clip, position, AudioManager.master.SFXMixer);
    }

    public void Play(AudioClip clip, Vector3 position, AudioMixerGroup mixer)
    {
        if (clip == null) return;
        if (!isInitialized) InitializePool();

        AudioSource source = GetAvailableSource();
        source.outputAudioMixerGroup = mixer;
        source.clip = clip;
        source.loop = false;
        source.transform.position = position;
        source.spatialBlend = 1f; // 3D
        source.Play();
    }


    public void PlayLoop(AudioClip clip)
    {
        if (clip == null) return;
        if (!isInitialized) InitializePool();

        AudioSource source = GetAvailableSource();
        source.outputAudioMixerGroup = AudioManager.master.SFXMixer;
        source.clip = clip;
        source.loop = true;
        source.spatialBlend = 0f; // 2D
        source.Play();
    }

    public AudioSource PlayLoop3D(AudioClip clip, Transform followTransform, float minDist = 5f, float maxDist = 20f)
    {
        if (clip == null) return null;

        GameObject go = new GameObject($"SFX3D_{clip.name}");
        go.transform.SetParent(followTransform);
        go.transform.localPosition = Vector3.zero;

        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.outputAudioMixerGroup = AudioManager.master.SFXMixer;
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 1f; // 3D
        source.volume = 0.6f;
        source.minDistance = minDist;
        source.maxDistance = maxDist;
        source.rolloffMode = AudioRolloffMode.Linear;

        source.Play();
        return source;
    }

    public void PlayPausable(AudioClip clip, AudioMixerGroup mixer)
    {
        if (clip == null) return;
        if (!isInitialized) InitializePool();

        if (pausableSource == null)
        {
        GameObject pausableObject = new GameObject("PausableAudioSource");
        pausableObject.transform.SetParent(transform);
        pausableSource = pausableObject.AddComponent<AudioSource>();
        pausableSource.loop = true;
        pausableSource.spatialBlend = 0f;
        pausableSource.playOnAwake = false;
        pausableSource.outputAudioMixerGroup = mixer;

    }

    if (pausableSource.clip != clip)
    {
        pausableClip = clip;
        pausableSource.clip = clip;
    }

    if (!pausableSource.isPlaying)
    {
        pausableSource.Play();
    }

    pausableSource.Pause();
    }

    public void PausePausable()
    {
        if (pausableSource != null)
            pausableSource.Pause();
    }

    public void UnPausePausable()
    {
        if (pausableSource != null)
            pausableSource.UnPause();
    }

    public void StopAll()
    {
        if (audioSourcePool == null) return;

        foreach (AudioSource source in audioSourcePool)
        {
            if (source != null)
            {
                source.Stop();
                source.loop = false;
            }
        }

        if (pausableSource != null)
        {
        pausableSource.Stop();
        pausableClip = null;
        }
    }
}
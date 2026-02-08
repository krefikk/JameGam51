using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

public class MXManager : MonoBehaviour
{
private AudioSource mxSource;
    private bool isInitialized = false;
    private Coroutine crossfadeCoroutine;

    private void InitializeMusicSource()
    {
        if (isInitialized) return;

        GameObject mxObject = new GameObject("MXSource");
        mxObject.transform.SetParent(transform);

        mxSource = mxObject.AddComponent<AudioSource>();
        mxSource.loop = true;
        mxSource.playOnAwake = false;
        mxSource.outputAudioMixerGroup = AudioManager.master.MXMixer;

        isInitialized = true;
    }

    public void Play(AudioClip clip)
    {
        if (clip == null) return;
        if (!isInitialized) InitializeMusicSource();

        if (crossfadeCoroutine != null)
        {
            StopCoroutine(crossfadeCoroutine);
            crossfadeCoroutine = null;
        }

        mxSource.clip = clip;
        mxSource.volume = 1f;
        mxSource.Play();
    }

    public void PlayWithCrossfade(AudioClip clip, float fadeDuration = 2f)
    {
        if (clip == null) return;
        if (!isInitialized) InitializeMusicSource();

        if (!mxSource.isPlaying)
        {
            Play(clip);
            return;
        }

        if (crossfadeCoroutine != null)
        {
            StopCoroutine(crossfadeCoroutine);
        }

        crossfadeCoroutine = StartCoroutine(CrossfadeRoutine(clip, fadeDuration));
    }

    private IEnumerator CrossfadeRoutine(AudioClip newClip, float duration)
    {
        GameObject tempObject = new GameObject("TempMXSource");
        tempObject.transform.SetParent(transform);
        AudioSource tempSource = tempObject.AddComponent<AudioSource>();

        tempSource.clip = newClip;
        tempSource.loop = true;
        tempSource.volume = 0f;
        tempSource.outputAudioMixerGroup = AudioManager.master.MXMixer;
        tempSource.Play();

        float elapsed = 0f;
        float startVolume = mxSource.volume;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            mxSource.volume = Mathf.Lerp(startVolume, 0f, t);
            tempSource.volume = Mathf.Lerp(0f, 1f, t);

            yield return null;
        }

        mxSource.Stop();
        mxSource.clip = newClip;
        mxSource.timeSamples = tempSource.timeSamples;
        mxSource.volume = 1f;
        mxSource.Play();

        Destroy(tempObject);

        crossfadeCoroutine = null;
    }

    public void Stop()
    {
        if (crossfadeCoroutine != null)
        {
            StopCoroutine(crossfadeCoroutine);
            crossfadeCoroutine = null;
        }

        if (mxSource != null) mxSource.Stop();
    }
}




using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;

public class AudioManager : MonoBehaviour
{
public static AudioManager master {get; private set;}
public MXManager MX { get; private set; }
public SFXManager SFX { get; private set; }


[Header("Mixers")]
[SerializeField] private AudioMixerGroup mXMixer;
public AudioMixerGroup MXMixer => mXMixer;
[SerializeField] private AudioMixerGroup sFXMixer;
public AudioMixerGroup SFXMixer => sFXMixer;
[SerializeField] private AudioMixerGroup uIMixer;
public AudioMixerGroup UIMixer => uIMixer;



[Header("Audio Source Pool")]
[SerializeField] private int poolSize = 10;
public int PoolSize => poolSize;


    private void Awake()
    {
        if (master != null && master != this)
        {
            Destroy(gameObject);
            return;
        }
        master = this;
        DontDestroyOnLoad(gameObject);

        MX = GetComponentInChildren<MXManager>();
        SFX = GetComponentInChildren<SFXManager>();
    }

}
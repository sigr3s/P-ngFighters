using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SoundManager : MonoBehaviour 
{
    #region Static Variables
    public static SoundManager Instance;
    #endregion

    #region Public Variables
    [Header("General settings"), Range(0, 1)] public float soundVolume = 1f;		
    [Header("Sound effects list"), SerializeField] public List<AudioClip> soundsEffectsList = new List<AudioClip>();
    #endregion

    #region Private Variables
    List<AudioSource> audioSources = new List<AudioSource>();
    #endregion

    #region Unity lifecycle
    void Awake() 
    {
        DontDestroyOnLoad(this);
        if (Instance == null) 
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    #endregion
    
    #region Public Methods
    public void PlaySound(int id, float volume = 1.0f) 
    {
        if (id >= soundsEffectsList.Count || id < 0){ return; }
        InternalPlaySound(soundsEffectsList[id], volume);
    }

    public void PlaySound(AudioClip audioClip, float volume = 1.0f) 
    {
        InternalPlaySound(audioClip, volume);
    }

    public void PlayUniqueSound(int id, float volume = 1.0f) 
    {
        if (id >= soundsEffectsList.Count || id < 0){ return; }

        bool found = false;
        for (int i = 0; i < audioSources.Count; i++)
        {
            if(audioSources[i].clip == soundsEffectsList[id])
            {
                found = true;
                if(!audioSources[i].isPlaying)
                {
                    audioSources[i].Play();
                }
            }
        }

        if(!found)
        {
            InternalPlaySound(soundsEffectsList[id], volume);
        }
    }    
    #endregion

    #region Private Functions
    void InternalPlaySound(AudioClip audioClip, float volume = 1.0f) 
    {
        AudioSource audioSource = GetAudioSourceFromPool();

        audioSource.volume = (volume * soundVolume);
        audioSource.clip = audioClip;
        audioSource.Play();     
    }

    AudioSource GetAudioSourceFromPool()
    {
        AudioSource target = null;
        for (int i = 0; i < audioSources.Count; i++)
        {
            if(audioSources[i].clip == null)
            {
                target = audioSources[i];
                continue;
            }
        }

        if(target == null)
        {
            target = this.gameObject.AddComponent<AudioSource>();
            audioSources.Add(target);
        }
        return target;
    }    
    #endregion
}


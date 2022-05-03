using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance = null;
    public Sound[] sounds;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        for (int i = 0; i < sounds.Length; i++)
        {
            sounds[i].source                = gameObject.AddComponent<AudioSource>();
            sounds[i].source.clip           = sounds[i].audioClip;
            sounds[i].source.volume         = sounds[i].volume;
            sounds[i].source.loop           = sounds[i].loop;
            sounds[i].source.playOnAwake    = sounds[i].playOnAwake;
        }
    }

    private void Start()
    {
        for (int i = 0; i < sounds.Length; i++)
            if (sounds[i].playOnAwake)
                Play(sounds[i].name);
    }

    public void Play(string clipName)
    {
        Play(clipName, 1f);
    }

    public void Play(string clipName, float pitch)
    {
        Sound s = Array.Find(sounds, x => x.name == clipName);

        if (s == null)
        {
            Debug.Log(clipName + "- Sound Not Found");
            return;
        }

        s.source.pitch = pitch;
        s.source.Play();
    }
}

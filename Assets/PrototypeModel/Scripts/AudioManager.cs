using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Audio[] Sounds;

    private void Awake()
    {
        foreach (Audio a in Sounds)
        {
            a.source = gameObject.AddComponent<AudioSource>();
            a.source.clip = a.clip;
            a.source.volume = a.volume;
            a.source.pitch = a.Pitch;
            a.source.loop = a.Loop;
        }
    }
    
    public void Play(string name)
    {
        Audio a = Array.Find(Sounds,Audio => Audio.Name == name);
        a.source.Play();
    }
    public void Stop(string name)
    {
        Audio a = Array.Find(Sounds, Audio => Audio.Name == name);
        a.source.Stop();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

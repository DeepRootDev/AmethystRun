using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip triggerClip;
    [SerializeField] private bool playOnce = true;

    private AudioSource audioSource;
    private bool hasPlayed = false;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = triggerClip;
        audioSource.playOnAwake = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasPlayed || !playOnce)
        {
            if (other.CompareTag("Player"))
            {
                audioSource.Play();
                hasPlayed = true;
            }
        }
    }
}

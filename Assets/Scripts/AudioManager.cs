using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    AudioSource audioSource;
    public static AudioManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;

        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    public void PlaySFX(AudioClip sfx)
    {
        audioSource.PlayOneShot(sfx);
    }
}

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundPlayer : MonoBehaviour
{
    public float pitchRange = 0.1f;

    private void Awake()
    {
        GetComponent<AudioSource>().pitch += Random.Range(-pitchRange, pitchRange);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSoundPlayer : MonoBehaviour
{
    [SerializeField] private List<AudioClip> _clips;
    [SerializeField] private AudioSource _source;

    public void PlaySound()
    {
        int index = Random.Range(0, _clips.Count);
        _source.PlayOneShot(_clips[index]);
    }
}

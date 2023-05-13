using UnityEngine;

public class IncreasingPitchSound : MonoBehaviour
{
    [SerializeField] private AudioSource _source;
    [SerializeField] private float _pitchStep;
    [SerializeField] private float _maxPitch;

    private float _defaultPitch;

    private void Awake()
    {
        _defaultPitch = _source.pitch;
    }

    public void PlaySound()
    {
        _source.Play();
        if (_source.pitch < _maxPitch)
        {
            _source.pitch += _pitchStep;
        }
    }

    public void ResetPitch()
    {
        _source.pitch = _defaultPitch;
    }

}

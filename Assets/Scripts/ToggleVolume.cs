using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class ToggleVolume : MonoBehaviour
{
    [SerializeField] private AudioMixer _mixer;

    [Space]
    [SerializeField] private string _playerPrefsTag;
    [SerializeField] private string _volumeVariableTag;

    [Space]
    [SerializeField] private UnityEvent _onToggleOn;
    [SerializeField] private UnityEvent _onToggleOff;

    private bool _isOn;


    private void Start()
    {
        _isOn = Convert.ToBoolean(PlayerPrefs.GetInt(_playerPrefsTag, 1));

        OnToggled();
    }

    public void OnToggled()
    {
        if (_isOn)
        {
            _mixer.SetFloat(_volumeVariableTag, 0);
            _onToggleOn?.Invoke();
        }
        else
        {
            _mixer.SetFloat(_volumeVariableTag, -80);
            _onToggleOff?.Invoke();
        }
    }

    public void Toggle()
    {
        _isOn = !_isOn;

        PlayerPrefs.SetInt(_playerPrefsTag, Convert.ToInt32(_isOn));

        OnToggled();
    }
}

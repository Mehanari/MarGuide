using UnityEngine;

public class AstronautSoundPlayer : MonoBehaviour
{
    [SerializeField] private RandomSoundPlayer _walkingSounds;
    [SerializeField] private RandomSoundPlayer _deathSounds;
    [SerializeField] private RandomSoundPlayer _fallSounds;

    public void PlayWalkSound()
    {
        _walkingSounds.PlaySound();
    }

    public void PlayDeathSound()
    {
        _deathSounds.PlaySound();
    }

    public void PlayFallSound()
    {
        _fallSounds.PlaySound();
    }
}

using UnityEngine;

public class AstronautSoundPlayer : MonoBehaviour
{
    [SerializeField] private Footsteps _walkingSounds;
    [SerializeField] private Footsteps _deathSounds;

    public void PlayWalkSound()
    {
        _walkingSounds.PlaySound();
    }

    public void PlayDeathSound()
    {
        _deathSounds.PlaySound();
    }
}

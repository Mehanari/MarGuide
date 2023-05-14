using UnityEngine;
using UnityEngine.Events;

public class Oxygen : MonoBehaviour
{
    [SerializeField] private UnityEvent _onOxygenCollected;
    private int _amount;

    public void SetAmount(int amount)
    {
        _amount = amount;
    }

    public void CollectOxygen(Unit astronaut)
    {
        astronaut.AddOxygen(_amount);
        _onOxygenCollected?.Invoke();
    }
}

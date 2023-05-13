using UnityEngine;
using UnityEngine.Events;

public class Oxygen : MonoBehaviour
{
    [SerializeField] private int _amount;
    [SerializeField] private UnityEvent _onOxygenCollected;

    private void OnTriggerEnter(Collider other)
    {
        var unit = other.gameObject.GetComponent<Unit>();
        if (unit != null)
        {
            unit.AddOxygen(_amount);
            _onOxygenCollected?.Invoke();
        }
    }
}

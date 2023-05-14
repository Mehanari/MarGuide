using UnityEngine;
using UnityEngine.Events;

public class EnterTriggerComponent : MonoBehaviour
{
    [SerializeField] private string _tag;
    [SerializeField] private UnityEvent _onTriggerEnter;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_tag))
        {
            _onTriggerEnter?.Invoke();
        }
    }
}

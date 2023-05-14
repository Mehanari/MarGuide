using UnityEngine;
using UnityEngine.Events;

public class AwakeAction : MonoBehaviour
{
    [SerializeField] private UnityEvent _action;

    private void Awake()
    {
        _action?.Invoke();
    }
}

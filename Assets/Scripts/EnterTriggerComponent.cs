using UnityEngine;
using UnityEngine.Events;

public class EnterTriggerComponent : MonoBehaviour
{
    [SerializeField] private string _tag;
    [SerializeField] private UnityEvent _onTriggerEnter;
    [SerializeField] private AstronautEvent _onAstronautEnter;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_tag))
        {
            _onTriggerEnter?.Invoke();
            var astronaut = other.gameObject.GetComponent<Unit>();
            if (astronaut != null)
            {
                _onAstronautEnter?.Invoke(astronaut);
            }
        }
    }

    [System.Serializable]
    public class AstronautEvent : UnityEvent<Unit> { }
}

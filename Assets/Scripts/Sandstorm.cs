using UnityEngine;

public class Sandstorm : MonoBehaviour
{
    private Vector3 _speed;

    public void SetSpeed(Vector3 speed)
    {
        _speed = speed;
    }

    private void Update()
    {
        var position = transform.position;
        position += _speed * Time.deltaTime;
        transform.position = position;
    }

    private void OnTriggerEnter(Collider other)
    {
        var unit = other.gameObject.GetComponent<Unit>();
        if (unit != null)
        {
            Debug.Log("Killing astronaut");
            unit.Die();
        }
    }
}

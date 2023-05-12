using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Unit : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private float _destinationTreashold;

    private GameTile _tileTo;
    private bool _isMoving;

    public bool IsMoving => _isMoving;

    private Queue<GameTile> _route = new Queue<GameTile>();

    public UnityEvent OnRouteComplete;

    public void SpawnOn(GameTile tile)
    {
        var position = transform.localPosition;
        position.x = tile.transform.localPosition.x;
        position.z = tile.transform.localPosition.z;
        transform.localPosition = position;
    }

    public void SetDestination(GameTile destination)
    {
        _tileTo = destination;
    }

    public void SetRoute(List<GameTile> route)
    {
        for (int i = 0; i < route.Count; i++)
        {
            _route.Enqueue(route[i]);
        }
    }

    private void Update()
    {
        if (_tileTo == null)
        {
            if (_route.Count == 0)
            {
                _isMoving = false;
                return;
            }
            else
            {
                _tileTo = _route.Dequeue();
            }
        }
        else
        {
            _isMoving = true;
            Vector3 moveDir = (_tileTo.transform.position - transform.position).normalized;
            transform.position += moveDir * _speed * Time.deltaTime;
            if (ReachedTile(_tileTo))
            {
                if (_route.Count > 0)
                {
                    _tileTo = _route.Dequeue();
                }
                else
                {
                    OnRouteComplete?.Invoke();
                    _tileTo = null;
                }
            }
        }
    }

    private bool ReachedTile(GameTile tile)
    {
        bool reachedX = transform.position.x > _tileTo.transform.position.x - _destinationTreashold &&
                        transform.position.x < _tileTo.transform.position.x + _destinationTreashold;
        bool reachedZ = transform.position.z > _tileTo.transform.position.z - _destinationTreashold &&
                transform.position.z < _tileTo.transform.position.z + _destinationTreashold;
        return reachedX && reachedZ;
    }
}

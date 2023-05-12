using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Unit : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private float _rotationSpeed;
    [SerializeField] private float _destinationTreashold;
    [SerializeField] private Animator _animator;
    [SerializeField] private string _walkAnimatorTag;
    [SerializeField] private string _deathTriggerTag;

    private Vector3 _rotationFrom;
    private Vector3 _rotationTo;
    private float _rotationProgress;

    private GameTile _tileTo;
    private bool _isMoving;

    public bool IsMoving => _isMoving;

    private Queue<GameTile> _path = new Queue<GameTile>();

    public UnityEvent OnPathComplete;
    public TileEvent OnNewTileSet;

    public void SpawnOn(GameTile tile)
    {
        var position = transform.localPosition;
        position.x = tile.transform.localPosition.x;
        position.z = tile.transform.localPosition.z;
        transform.localPosition = position;
    }

    public void SetPath(List<GameTile> route)
    {
        for (int i = 0; i < route.Count; i++)
        {
            _path.Enqueue(route[i]);
        }
    }

    private void Update()
    {
        if (_tileTo == null)
        {
            if (_path.Count == 0)
            {
                _isMoving = false;
                _animator.SetBool(_walkAnimatorTag, false);
                return;
            }
            else
            {
                _animator.SetBool(_walkAnimatorTag, true);
                _tileTo = _path.Dequeue();
            }
        }
        else
        {
            _isMoving = true;
            Vector3 moveDir = (_tileTo.transform.position - transform.position).normalized;
            transform.position += moveDir * _speed * Time.deltaTime;
            transform.LookAt(_tileTo.transform);
            if (_rotationProgress < 1)
            {
                _rotationProgress += _rotationSpeed * Time.deltaTime;
                transform.eulerAngles = Vector3.LerpUnclamped(_rotationFrom, _rotationTo, _rotationProgress);
            }
            if (ReachedTile(_tileTo))
            {
                if (_path.Count > 0)
                {
                    _tileTo = _path.Dequeue();
                    OnNewTileSet?.Invoke(_tileTo);
                    _rotationFrom = transform.eulerAngles;
                    transform.LookAt(_tileTo.transform);
                    _rotationTo = transform.eulerAngles;
                    if (_rotationTo.y > 180)
                    {
                        _rotationTo.y = -360 + _rotationTo.y;
                    }
                    transform.eulerAngles = _rotationFrom;
                    _rotationProgress = 0f;
                }
                else
                {
                    OnPathComplete?.Invoke();
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

    public void Die()
    {
        _tileTo = null;
        _isMoving = false;
        _animator.SetTrigger(_deathTriggerTag);
    }

    public class TileEvent : UnityEvent<GameTile> { }
}

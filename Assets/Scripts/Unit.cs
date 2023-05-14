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
    private bool _isDead;

    public bool IsMoving => _isMoving;

    private Queue<GameTile> _path = new Queue<GameTile>();

    public UnityEvent OnPathComplete;
    public UnityEvent OnDie;
    public TileEvent OnNewTileSet = new TileEvent();
    public OxygenEvent OnGetOxygen = new OxygenEvent();

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
        if (_isDead)
        {
            return;
        }
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
                if (Mathf.Abs(_rotationFrom.y - _rotationTo.y) > 200)
                {
                    _rotationFrom.y = -_rotationFrom.y;
                }
                _rotationProgress += _rotationSpeed * Time.deltaTime;
                transform.eulerAngles = Vector3.LerpUnclamped(_rotationFrom, _rotationTo, _rotationProgress);
            }
            if (ReachedTile(_tileTo))
            {
                if (_path.Count > 0)
                {
                    _tileTo = _path.Dequeue();
                    OnNewTileSet?.Invoke(_tileTo);
                    _rotationFrom = ValidateRotation(transform.eulerAngles);
                    transform.LookAt(_tileTo.transform);
                    _rotationTo = ValidateRotation(transform.eulerAngles);
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

    private Vector3 ValidateRotation(Vector3 rotation)
    {
        if (rotation.y > 180)
        {
            rotation.y = -360 + rotation.y;
        }
        return rotation;
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
        _isDead = true;
        _isMoving = false;
        _animator.SetTrigger(_deathTriggerTag);
        OnDie?.Invoke();
    }

    public void AddOxygen(int amount)
    {
        OnGetOxygen?.Invoke(amount);
    }

    public class TileEvent : UnityEvent<GameTile> { }

    public class OxygenEvent : UnityEvent<int> { }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Game : MonoBehaviour
{
    [Header("Map options")]
    [SerializeField] private GameBoard _board;
    [SerializeField] private Vector2Int _boardSize;
    [SerializeField] private Vector2Int _unitSpawnPoint;
    [SerializeField] private int _startPartLength;
    [SerializeField] private int _endPartLength;
    [SerializeField] private int _mapBorderWidth;
    [SerializeField] private MapGenerator _mapGenerator;
    [SerializeField] private TileViewFactory _viewFactory;
    [SerializeField] private int _oxygenChunkSize;

    [Header("Player options")]
    [SerializeField] private Camera _camera;
    [SerializeField] private Unit _unitPrefab;
    [SerializeField] private int _oxygen;
    [SerializeField] private float _cameraSpeed;

    [Header("Tile selection options")]
    [SerializeField] private Color _validPathColor;
    [SerializeField] private Color _violatedPathColor;
    [SerializeField] private Color _busyPathColor;
    [SerializeField] private UnityEvent _onTileSelected;
    [SerializeField] private UnityEvent _onPathSelected;
    [SerializeField] private UnityEvent _onPathApproved;
    [SerializeField] private UnityEvent _onPathDenied;

    private int _maxZReached;
    private GameTile _lastTileOnPath;
    private bool _selectingNewPath;
    private List<GameTile> _currentPath = new List<GameTile>();
    private List<GameTile> _busyPath;
    private bool _currentPathIsViolated;
    private Unit _unit;

    private List<List<GameTile>> _createdPathes = new List<List<GameTile>>();

    private Ray TouchRay => _camera.ScreenPointToRay(Input.mousePosition);

    private void Start()
    {
        _board.InitializeBoard(_boardSize, _startPartLength, _endPartLength, _mapBorderWidth, _mapGenerator, _oxygenChunkSize, _viewFactory);
        _lastTileOnPath = _board.GetTile(_unitSpawnPoint.x, _unitSpawnPoint.y);
        _unit = Instantiate(_unitPrefab);
        _unit.SpawnOn(_lastTileOnPath);
        _unit.OnPathComplete.AddListener(OnUnitCompletedPath);
        _unit.OnNewTileSet.AddListener(UnitWentOnNewTile);
        _unit.OnGetOxygen.AddListener(UnitCollectedOxygen);
        _maxZReached = (int)_lastTileOnPath.transform.position.z;
    }

    private void UnitCollectedOxygen(int amount)
    {
        _oxygen += amount;
    }

    private void UnitWentOnNewTile(GameTile tile)
    {
        if (tile.Type == TileType.Ground)
        {
            _oxygen -= 1;
        }
        if (tile.Type == TileType.Mountain)
        {
            _oxygen -= 2;
            tile.Type = TileType.Ground;
            tile.SetView(_viewFactory.GetView(TileType.Ground), 0);
            tile.SetColor(_busyPathColor);
        }
        if (_oxygen <= 0)
        {
            _unit.Die();
        }

        var tileZ = (int)tile.transform.position.z;
        if (tileZ > _maxZReached)
        {
            _maxZReached = tileZ;
        }
    }

    private void OnUnitCompletedPath()
    {
        for (int i = 0; i < _busyPath.Count - 1; i++)
        {
            _busyPath[i].SetDefaultColor();
        }
        _busyPath = null;
        if (_createdPathes.Count > 0)
        {
            var lastPath = _createdPathes[0];
            SetPathToUnit(lastPath);
        }
        ValidateCreatedPathColors();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartPathSelection();
        }
        if (Input.GetMouseButtonUp(0))
        {
            EndPathSelection();
        }
        if (Input.GetMouseButtonDown(1))
        {
            DeleteLastPath();
        }

        if (_selectingNewPath)
        {
            GrowCurrentPath();
        }

        ProcessCameraMovement();
    }

    private void StartPathSelection()
    {
        var tile = _board.GetTile(TouchRay);
        if (tile != null)
        {
            if (tile == _lastTileOnPath)
            {
                _selectingNewPath = true;
            }
        }
    }

    private void EndPathSelection()
    {
        _selectingNewPath = false;
        _onPathSelected?.Invoke();
        if (PathIsValid())
        {
            _lastTileOnPath = _currentPath[_currentPath.Count - 1];
            _createdPathes.Add(_currentPath);
            if (_busyPath == null)
            {
                SetPathToUnit(_currentPath);
            }
            _currentPath = new List<GameTile>();
            _onPathApproved?.Invoke();
        }
        else
        {
            foreach (var tile in _currentPath)
            {
                tile.SetDefaultColor();
            }
            _lastTileOnPath.SetColor(_validPathColor);
            _currentPath.Clear();
            _onPathDenied?.Invoke();
            ValidateCreatedPathColors();
        }
    }

    private bool PathIsValid()
    {
        if (_currentPath.Count <= 1)
        {
            return false;
        }
        if (_currentPathIsViolated)
        {
            _currentPathIsViolated = false;
            return false;
        }

        var previousTileOnPath = _currentPath[0];
        for (int i = 1; i < _currentPath.Count; i++)
        {
            var currentTile = _currentPath[i];
            var distance = Vector3.Distance(currentTile.transform.position, previousTileOnPath.transform.position);
            if (distance > 1) 
            {
                return false;
            }
            if (currentTile.Type == TileType.Acid || currentTile.Type == TileType.Border)
            {
                return false;
            }
            previousTileOnPath = currentTile;
        }
        return true;
    }



    private void DeleteLastPath()
    {
        if (_createdPathes.Count == 0 || _selectingNewPath)
        {
            return;
        }
        var lastPath = _createdPathes[_createdPathes.Count - 1];
        for (int i = 0; i < lastPath.Count; i++)
        {
            lastPath[i].SetDefaultColor();
        }
        _lastTileOnPath = lastPath[0];
        _lastTileOnPath.SetColor(_validPathColor);
        _createdPathes.Remove(lastPath);
        ValidateCreatedPathColors();
    }

    private void GrowCurrentPath()
    {
        var tile = _board.GetTile(TouchRay);
        if (tile != null)
        {
            if (TileTooFarFromEndOfPath(tile) || tile.Type == TileType.Acid || tile.Type == TileType.Border)
            {
                ViolateCurrentPath();
            }
            if (!_currentPath.Contains(tile))
            {
                _currentPath.Add(tile);
                _onTileSelected?.Invoke();
            }
            if (_currentPathIsViolated)
            {
                tile.SetColor(_violatedPathColor);
            }
            else
            {
                tile.SetColor(_validPathColor);
            }
        }
    }

    private bool TileTooFarFromEndOfPath(GameTile tile)
    {
        if (_currentPath.Count == 0)
        {
            return false;
        }
        var lastTileOnPath = _currentPath[_currentPath.Count - 1];
        var distance = Vector3.Distance(tile.transform.position, lastTileOnPath.transform.position);
        return distance > 1;

    }

    private void ViolateCurrentPath()
    {
        _currentPathIsViolated = true;
        for (int i = 0; i < _currentPath.Count; i++)
        {
            _currentPath[i].SetColor(_violatedPathColor);
        }
    }

    private void SetPathToUnit(List<GameTile> path)
    {
        _busyPath = path;
        for (int i = 0; i < _busyPath.Count; i++)
        {
            _busyPath[i].SetColor(_busyPathColor);
        }
        _unit.SetPath(_busyPath);
        _createdPathes.Remove(_busyPath);
    }

    private void ValidateCreatedPathColors()
    {
        for (int i = 0; i < _createdPathes.Count; i++)
        {
            var path = _createdPathes[i];
            for (int j = 0; j < path.Count; j++)
            {
                path[j].SetColor(_validPathColor);
            }
        }
        for (int i = 0; i < _currentPath.Count; i++)
        {
            if (_currentPathIsViolated)
            {
                _currentPath[i].SetColor(_violatedPathColor);
            }
            else
            {
                _currentPath[i].SetColor(_validPathColor);
            }
        }
        if (_busyPath == null)
        {
            return;
        }
        for (int i = 0; i < _busyPath.Count; i++)
        {
            _busyPath[i].SetColor(_busyPathColor);
        }
    }

    private void ProcessCameraMovement()
    {
        if ((int)_camera.transform.position.z < _maxZReached)
        {
            var cameraPosition = _camera.transform.position;
            cameraPosition.z += _cameraSpeed * Time.deltaTime;
            _camera.transform.position = cameraPosition;
        }
    }
}

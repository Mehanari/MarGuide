using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Game : MonoBehaviour
{
    [Header("Difficulty options")]
    [SerializeField] private DifficultyConfig _difficultyConfig;
    [SerializeField] private int _startDifficulty;

    [Header("Map options")]
    [SerializeField] private GameBoard _board;
    [SerializeField] private Vector2Int _unitSpawnPoint;
    [SerializeField] private TileViewFactory _viewFactory;

    [Header("Player options")]
    [SerializeField] private Camera _camera;
    [SerializeField] private Unit _unitPrefab;
    [SerializeField] private float _cameraSpeed;
    private int _oxygen;

    [Header("Tile selection options")]
    [SerializeField] private Color _validPathColor;
    [SerializeField] private Color _violatedPathColor;
    [SerializeField] private Color _busyPathColor;
    [SerializeField] private UnityEvent _onTileSelected;
    [SerializeField] private UnityEvent _onPathSelected;
    [SerializeField] private UnityEvent _onPathApproved;
    [SerializeField] private UnityEvent _onPathDenied;

    [Header("Sandstorm options")]
    [SerializeField] private Sandstorm _sandstorm;

    [Header("Game flow options")]
    [SerializeField] private UnityEvent _onWin;
    [SerializeField] private UnityEvent _onLose;

    [Header("Base options")]
    [SerializeField] private GameObject _baseObject;

    [Header("UI options")]
    [SerializeField] private TextMeshProUGUI _oxygenCountTextMesh;

    private int _maxZReached;
    private GameTile _lastTileOnPath;
    private bool _selectingNewPath;
    private List<GameTile> _currentPath = new List<GameTile>();
    private List<GameTile> _busyPath;
    private bool _currentPathIsViolated;
    private Unit _unit;
    private bool _stopGame;
    private Vector2Int _generatedMapSize;

    private List<List<GameTile>> _createdPathes = new List<List<GameTile>>();

    private Ray TouchRay => _camera.ScreenPointToRay(Input.mousePosition);

    private void Start()
    {
        int difficulty = GlobalDifficulty.Difficulty + _startDifficulty;
        var diffConfig = _difficultyConfig.GetDifficulty(difficulty);
        var mapGenerator = new MapGenerator(diffConfig.GenerationParameters);
        _generatedMapSize = diffConfig.GenerationParameters.GeneratedMapSize;

        _board.InitializeBoard(mapGenerator, diffConfig.OxygenChunkSize, diffConfig.OxygenTankAmount,  _viewFactory);
        _lastTileOnPath = _board.GetTile(_unitSpawnPoint.x, _unitSpawnPoint.y);
        SetUpUnit();
        PlaceBase(diffConfig.GenerationParameters);
        _maxZReached = (int)_lastTileOnPath.transform.position.z;
        _sandstorm.SetSpeed(diffConfig.SandstormSpeed);
        _sandstorm.transform.position = diffConfig.SandstormSpawnPosition;

        UpdateOxygen(diffConfig.StartOxygen);
    }

    private void SetUpUnit()
    {
        _unit = Instantiate(_unitPrefab);
        _unit.SpawnOn(_lastTileOnPath);
        _unit.OnPathComplete.AddListener(OnUnitCompletedPath);
        _unit.OnNewTileSet.AddListener(UnitWentOnNewTile);
        _unit.OnGetOxygen.AddListener(UpdateOxygen);
        _unit.OnDie.AddListener(Lose);
    }

    private void PlaceBase(GenerationParameters parameters)
    {
        int startPartLength = parameters.StartPartLength;
        int endPartLength = parameters.EndPartLength;
        int borderWidth = parameters.BorderWidth;
        var baseTile = _board.GetTile(startPartLength + _generatedMapSize.x + endPartLength, borderWidth + _generatedMapSize.y/2);
        _baseObject.transform.position = baseTile.transform.position;
    }

    public void IncreaseDifficulty()
    {
        GlobalDifficulty.IncreaseDifficulty();
    }

    public void Lose()
    {
        _sandstorm.SetSpeed(Vector3.zero);
        _onLose?.Invoke();
        _stopGame = true;
    }

    public void Win()
    {
        _sandstorm.SetSpeed(Vector3.zero);
        _onWin?.Invoke();
        _stopGame = true;
    }

    private void UnitWentOnNewTile(GameTile tile)
    {
        if (tile.Type == TileType.Ground)
        {
            UpdateOxygen(-1);
        }
        if (tile.Type == TileType.Mountain)
        {
            UpdateOxygen(-2);
            tile.Type = TileType.Ground;
            tile.SetView(_viewFactory.GetView(TileType.Ground), 0);
            tile.SetColor(_busyPathColor);
        }


        var tileZ = (int)tile.transform.position.z;
        if (tileZ > _maxZReached)
        {
            _maxZReached = tileZ;
        }
    }

    private void UpdateOxygen(int amount)
    {
        _oxygen += amount;
        if (_oxygen <= 0)
        {
            _unit.Die();
            _oxygen = 0;
        }
        _oxygenCountTextMesh.text = _oxygen.ToString();

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
        if (_stopGame) return;
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

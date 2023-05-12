using System.Collections.Generic;
using System.Linq;
using TMPro.EditorUtilities;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private GameBoard _board;
    [SerializeField] private Vector2Int _boardSize;
    [SerializeField] private MapGenerator _mapGenerator;
    [SerializeField] private int _oxygenChunkSize;
    [SerializeField] private Camera _camera;

    private GameTile _lastTileOnPath;
    private bool _selectingNewPath;
    private List<GameTile> _currentPath = new List<GameTile>();

    private Ray TouchRay => _camera.ScreenPointToRay(Input.mousePosition);

    private void Start()
    {
        _board.InitializeBoard(_boardSize, _mapGenerator, _oxygenChunkSize);
        _lastTileOnPath = _board.GetTile(0, 0);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
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
        if (Input.GetMouseButtonUp(0))
        {
            EndPathSelection();
            _selectingNewPath = false;
        }

        if (_selectingNewPath)
        {
            var tile = _board.GetTile(TouchRay);
            if (tile != null)
            {
                if (!_currentPath.Contains(tile))
                {
                    _currentPath.Add(tile);
                }
                tile.SetColor(Color.green);
            }
        }
    }

    private void EndPathSelection()
    {
        if (PathIsValid())
        {
            _lastTileOnPath = _currentPath[_currentPath.Count - 1];
        }
        else
        {
            foreach (var tile in _currentPath)
            {
                tile.SetDefaultColor();
            }
            _lastTileOnPath.SetColor(Color.green);
        }
        _currentPath.Clear();
    }

    private bool PathIsValid()
    {
        if (_currentPath.Count <= 1)
        {
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
            if (currentTile.Type == TileType.Acid)
            {
                return false;
            }
            previousTileOnPath = currentTile;
        }
        return true;
    }
}

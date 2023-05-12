using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    [SerializeField] private GameTile _tilePrefab;
    [SerializeField] private GameTileView _groundPrefab;
    [SerializeField] private GameTileView _mountainPrefab;
    [SerializeField] private GameTileView _acidPrefab;
    [SerializeField] private GameObject _oxygenTankPrefab;
    [SerializeField] private LayerMask _tilesMask;


    private MapGenerator _generator;
    private Vector2Int _boardSize;
    private TileType[,] _typeMap;
    private GameTile[,] _tiles;

    public void InitializeBoard(Vector2Int size, MapGenerator generator, int oxygenChunkSize)
    {
        _boardSize = size;
        _generator = generator;
        _tiles = new GameTile[_boardSize.x, _boardSize.y];
        _typeMap = _generator.GenerateMap(size.x, size.y);
        GenerateTiles();
        PlaceOxygen(oxygenChunkSize);
    }

    private void GenerateTiles()
    {
        for (int i = 0; i < _boardSize.x; i++)
        {
            for (int j = 0; j < _boardSize.y; j++)
            {
                GenerateTile(i, j);
            }
        }
    }

    private void GenerateTile(int x, int y)
    {
        _tiles[x, y] = Instantiate(_tilePrefab);
        _tiles[x, y].transform.localPosition = new Vector3(y, 0, x);
        _tiles[x, y].Type = _typeMap[x, y];
        _tiles[x, y].Coordinates = new Vector2Int(x, y);
        switch (_tiles[x, y].Type)
        {
            case TileType.Mountain:
                _tiles[x, y].SetView(_mountainPrefab, 0);
                break;
            case TileType.Ground:
                _tiles[x, y].SetView(_groundPrefab, 0);
                break;
            case TileType.Acid:
                _tiles[x, y].SetView(_acidPrefab, 0);
                break;
            default:
                _tiles[x, y].SetView(_groundPrefab, 0);
                break;
        }
    }

    private void PlaceOxygen(int chunkSize)
    {
        int startX = 0;
        int startY = 0;
        while (startX < _boardSize.x)
        {
            while (startY < _boardSize.y)
            {
                var groundTiles = GetGroundTilesBetweenPoints(startX, startY, startX + chunkSize, startY + chunkSize);
                if (groundTiles.Count > 0)
                {
                    int tileIndex = Random.Range(0, groundTiles.Count);
                    PlaceOxygenOnTile(groundTiles[tileIndex]);

                }
                startY += chunkSize;
            }
            startY = 0;
            startX += chunkSize;
        }
    }

    private List<GameTile> GetGroundTilesBetweenPoints(int startX, int startY, int endX, int endY)
    {
        List<GameTile> groundTiles = new List<GameTile>();
        for (int i = startX; i < endX; i++)
        {
            for (int j = startY; j < endY; j++)
            {
                if (i < _boardSize.x && j < _boardSize.y)
                {
                    if (_tiles[i, j].Type == TileType.Ground)
                    {
                        groundTiles.Add(_tiles[i, j]);
                    }
                }
            }
        }
        return groundTiles;
    }

    private void PlaceOxygenOnTile(GameTile tile)
    {
        var oxygen = Instantiate(_oxygenTankPrefab);
        oxygen.transform.localPosition = tile.transform.localPosition;
    }

    public GameTile GetTile(Ray ray)
    {
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, float.MaxValue, _tilesMask))
        {
            int y = Round(hit.point.x);
            int x = Round(hit.point.z);
            if (x >= 0 && x < _boardSize.x && y >= 0 && y < _boardSize.y)
            {
                return _tiles[x, y];
            }
        }
        return null;
    }

    public GameTile GetTile(int x, int y)
    {
        if (x >= 0 && x < _boardSize.x && y >= 0 && y < _boardSize.y)
        {
            return _tiles[x, y];
        }
        return null;
    }

    private int Round(float value)
    {
        int rounded = (int)value;
        if (value - rounded >= 0.5)
        {
            rounded++;
        }
        return rounded;
    }
}

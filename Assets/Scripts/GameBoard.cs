using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    [SerializeField] private GameTile _tilePrefab;
    [SerializeField] private Oxygen _oxygenTankPrefab;
    [SerializeField] private LayerMask _tilesMask;

    private TileViewFactory _viewFactory;
    private MapGenerator _generator;
    private Vector2Int _boardSize;
    private TileType[,] _typeMap;
    private GameTile[,] _tiles;

    public void InitializeBoard(MapGenerator generator, int oxygenChunkSize, int oxygenTankAount, TileViewFactory factory)
    {
        _generator = generator;
        _typeMap = _generator.GenerateMap();
        _boardSize = new Vector2Int(_typeMap.GetLength(0), _typeMap.GetLength(1));
        _viewFactory = factory;
        _tiles = new GameTile[_boardSize.x, _boardSize.y];
        GenerateTiles();
        PlaceOxygen(oxygenChunkSize, oxygenTankAount);
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
        int rotation = Random.Range(0, 4) * 90;
        _tiles[x, y].SetView(_viewFactory.GetView(_tiles[x, y].Type), rotation);
    }

    private void PlaceOxygen(int chunkSize, int tankAmount)
    {
        int startX = 0;
        int startY = 0;
        while (startX < _boardSize.x - chunkSize)
        {
            while (startY < _boardSize.y)
            {
                var groundTiles = GetGroundTilesBetweenPoints(startX, startY, startX + chunkSize, startY + chunkSize);
                if (groundTiles.Count > 0)
                {
                    int tileIndex = Random.Range(0, groundTiles.Count);
                    var oxygen = PlaceOxygenOnTile(groundTiles[tileIndex]);
                    oxygen.SetAmount(tankAmount);

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

    private Oxygen PlaceOxygenOnTile(GameTile tile)
    {
        var oxygen = Instantiate(_oxygenTankPrefab);
        oxygen.transform.localPosition = tile.transform.localPosition;
        return oxygen;
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

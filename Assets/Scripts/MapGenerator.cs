using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private int _acidLakesDensity;
    [SerializeField] private int _acidLakesGenerationIterationsCount = 2;
    [SerializeField] private int _pathPartMaxLength = 3;
    [SerializeField] private int _canyonsWidth = 2;
    [SerializeField] private int _verticalCanyonsPeriod = 10;
    [SerializeField] private int _horizontalCanyonsPeriod = 6;

    private int _mapHeight;
    private int _mapWidth;



    public TileType[,] GenerateMap(int mapHeight, int mapWidth, int startPartLength, int endPartLength, int mapBorderWidth)
    {
        _mapHeight = mapHeight;
        _mapWidth = mapWidth;

        TileType[,] map = new TileType[_mapHeight + startPartLength + endPartLength + mapBorderWidth*2, _mapWidth + mapBorderWidth*2];
        for (int x = mapBorderWidth; x < mapBorderWidth + startPartLength; x++)
        {
            for (int y = mapBorderWidth; y < map.GetLength(1) - mapBorderWidth; y++)
            {
                map[x, y] = TileType.Ground;
            }
        }
        for (int x = map.GetLength(0) - mapBorderWidth - endPartLength; x < map.GetLength(0) - mapBorderWidth; x++)
        {
            for (int y = mapBorderWidth; y < map.GetLength(1) - mapBorderWidth; y++)
            {
                map[x, y] = TileType.Ground;
            }
        }
        TileType[,] generatedMap = new TileType[_mapHeight, _mapWidth];
        GenerateWallsAndLakes(generatedMap);
        SurroundLakesWithFloor(generatedMap);
        for (int i = 1; i < _mapWidth; i += _verticalCanyonsPeriod)
        {
            GenerateRandomVerticalPath(generatedMap, i, _canyonsWidth);
        }
        for (int i = 0; i < _mapHeight; i += _horizontalCanyonsPeriod)
        {
            GenerateRandomHorizontalPath(generatedMap, i, _canyonsWidth);
        }
        for (int x = 0; x < _mapHeight; x++)
        {
            for (int y = 0; y < _mapWidth; y++)
            {
                map[x + startPartLength + mapBorderWidth, y + mapBorderWidth] = generatedMap[x, y];
            }
        }
        return map;
    }

    private void GenerateWallsAndLakes(TileType[,] map)
    {
        var binaryMap = Noise.GenerateBinaryNoiseGrid(_mapHeight, _mapWidth, 100 - _acidLakesDensity);
        CellularAutomatonAlgorithm.Apply(binaryMap, _acidLakesGenerationIterationsCount);
        for (int i = 0; i < _mapHeight; i++)
        {
            for (int j = 0; j < _mapWidth; j++)
            {
                if (binaryMap[i, j])
                {
                    map[i, j] = TileType.Mountain;
                }
                else
                {
                    map[i, j] = TileType.Acid;
                    if (j == _mapWidth - 1 || j == 0)
                    {
                        map[i, j] = TileType.Ground;
                    }
                }
            }
        }
    }

    private void SurroundLakesWithFloor(TileType[,] map)
    {
        for (int x = 0; x < _mapWidth; x++)
        {
            for (int y = 0; y < _mapHeight; y++)
            {
                if (map[y, x] == TileType.Acid)
                {
                    TurnMountainsToFloorAroundPoint(map, x, y);
                }
            }
        }
    }

    private void TurnMountainsToFloorAroundPoint(TileType[,] map, int x, int y)
    {
        for (int neighbourX = x - 1; neighbourX <= x + 1; neighbourX++)
        {
            for (int neighbourY = y - 1; neighbourY <= y + 1; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < _mapWidth && neighbourY >= 0 && neighbourY < _mapHeight)
                {
                    if (map[neighbourY, neighbourX] == TileType.Mountain)
                    {
                        map[neighbourY, neighbourX] = TileType.Ground;
                    }
                }
            }
        }
    }

    private void GenerateRandomVerticalPath(TileType[,] map, int startX, int width)
    {
        int currentX = startX;
        int currentY = 0;

        while (currentY < _mapHeight)
        {
            int lineLength = Random.Range(1, _pathPartMaxLength + 1);
            for (int i = 0; i < lineLength; i++)
            {
                if (currentY >= _mapHeight)
                {
                    break;
                }
                for (int x = currentX; x < currentX + width; x++)
                {
                    if (x >= _mapWidth)
                    {
                        break;
                    }
                    if (map[currentY, x] != TileType.Acid)
                    {
                        map[currentY, x] = TileType.Ground;
                    }
                }
                currentY++;
            }
            bool moveXRigth = Random.Range(0, 2) == 1;
            if (moveXRigth)
            {
                if (currentX + 1 < _mapWidth)
                {
                    currentX++;
                }
                else
                {
                    currentX--;
                }
            }
            else
            {
                if (currentX - 1 > 0)
                {
                    currentX--;
                }
                else
                {
                    currentX++;
                }
            }
            for (int x = currentX; x < currentX + width; x++)
            {
                if (x >= _mapWidth)
                {
                    break;
                }
                if (map[currentY - 1, x] != TileType.Acid)
                {
                    map[currentY - 1, x] = TileType.Ground;
                }
            }
        }
    }

    private void GenerateRandomHorizontalPath(TileType[,] map, int startX, int width)
    {
        int currentX = startX;
        int currentY = 0;

        while (currentY < _mapWidth)
        {
            int lineLength = Random.Range(1, _pathPartMaxLength + 1);
            for (int i = 0; i < lineLength; i++)
            {
                if (currentY >= _mapWidth)
                {
                    break;
                }
                for (int x = currentX; x < currentX + width; x++)
                {
                    if (x >= _mapHeight)
                    {
                        break;
                    }
                    if (map[x, currentY] != TileType.Acid)
                    {
                        map[x, currentY] = TileType.Ground;
                    }
                }
                currentY++;
            }
            bool moveXRigth = Random.Range(0, 2) == 1;
            if (moveXRigth)
            {
                if (currentX + 1 < _mapHeight)
                {
                    currentX++;
                }
                else
                {
                    currentX--;
                }
            }
            else
            {
                if (currentX - 1 > 0)
                {
                    currentX--;
                }
                else
                {
                    currentX++;
                }
            }
            for (int x = currentX; x < currentX + width; x++)
            {
                if (x >= _mapHeight)
                {
                    break;
                }
                if (map[x, currentY - 1] != TileType.Acid)
                {
                    map[x, currentY - 1] = TileType.Ground;
                }
            }
        }
    }
}

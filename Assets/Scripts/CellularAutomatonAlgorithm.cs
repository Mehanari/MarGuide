public static class CellularAutomatonAlgorithm
{
    public static void Apply(bool[,] map, int iterationsCount)
    {
        for (int i = 0; i < iterationsCount; i++)
        {
            SmoothMap(map);
        }
    }

    private static void SmoothMap(bool[,] map)
    {
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                int neighbourWallTiles = GetSurroundingCount(map, x, y);

                map[x, y] = neighbourWallTiles > 4;
            }
        }
    }

    private static int GetSurroundingCount(bool[,] map, int x, int y)
    {
        int wallCount = 0;
        for (int neighbourX = x - 1; neighbourX <= x + 1; neighbourX++)
        {
            for (int neighbourY = y - 1; neighbourY <= y + 1; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < map.GetLength(0) && neighbourY >= 0 && neighbourY < map.GetLength(1))
                {
                    if (map[neighbourX, neighbourY])
                    {
                        wallCount++;
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }
        return wallCount;
    }
}

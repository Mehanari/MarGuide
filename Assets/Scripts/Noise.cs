public static class Noise
{
    public static bool[,] GenerateBinaryNoiseGrid(int gridHeight, int gridWidth, int noiseDencity)
    {
        bool[,] noiseGrid = new bool[gridHeight, gridWidth];
        for (int i = 0; i < gridHeight; i++)
        {
            for (int j = 0; j < gridWidth; j++)
            {
                int randomValue = UnityEngine.Random.Range(0, 101);
                noiseGrid[i, j] = randomValue < noiseDencity;
            }
        }
        return noiseGrid;
    }
}

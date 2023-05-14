public static class GlobalDifficulty 
{
    private static int _currentDifficulty;

    public static int Difficulty => _currentDifficulty;

    public static void IncreaseDifficulty()
    {
        _currentDifficulty++;
    }

    public static void ResetDifficulty(){
        _currentDifficulty = 0;
    }
}

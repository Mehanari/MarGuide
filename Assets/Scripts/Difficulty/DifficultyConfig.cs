using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Difficulty config", menuName = "Difficulty/Difficulty config")]
public class DifficultyConfig : ScriptableObject
{
    [SerializeField] private List<DifficultyLevel> _difficultyLevels;

    public DifficultyLevel GetDifficulty(int difficulty)
    {
        if (difficulty < _difficultyLevels.Count)
        {
            return _difficultyLevels[difficulty];
        }

        return _difficultyLevels[_difficultyLevels.Count - 1];
    }
}


[System.Serializable]
public class DifficultyLevel
{
    public GenerationParameters GenerationParameters;
    public int OxygenChunkSize;
    public int StartOxygen;
    public int OxygenTankAmount;
    public Vector3 SandstormSpeed;
}

[System.Serializable]
public class GenerationParameters
{
    public int AcidLakesDensity;
    public int AcidLakesGenerationIterationsCount = 2;
    public int PathPartMaxLength = 3;
    public int CanyonsWidth = 2;
    public int VerticalCanyonsPeriod = 10;
    public int HorizontalCanyonsPeriod = 6;
    public int StartPartLength = 10;
    public int EndPartLength = 10;
    public int BorderWidth = 5;
    public Vector2Int GeneratedMapSize;
}

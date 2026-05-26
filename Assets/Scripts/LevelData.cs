using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class LevelData
{
    public int stars = 0;
    public bool isCompleted = false;
    public int bestMoves = 0;

    public int levelID;
}

[Serializable]
public class LevelsDataWrapper
{
    public List<LevelData> levelsData;
    public LevelsDataWrapper(List<LevelData> levelsData)
    {
        this.levelsData = levelsData;
    }
}
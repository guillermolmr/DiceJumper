using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public List<Level> levels = new List<Level>();
    public List<LevelData> levelsData = new List<LevelData>();
    public Material diceBackground;
    public Level currentLevel { get; private set; }
    [SerializeField]int currentLevelIndex;

    [SerializeField] bool testMode;


    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;


        int lastLevel = PlayerPrefs.GetInt("LastLevel", 0);
        string jsonData= PlayerPrefs.GetString("LevelsData", "");
#if UNITY_EDITOR
        Debug.Log("Loaded level data: " + jsonData);
#endif
        if (string.IsNullOrEmpty(jsonData))
        {
            for(int i = 0; i < levels.Count; i++)
            {
                LevelData levelData = new LevelData
                {
                    levelID = i,
                    isCompleted = i <= lastLevel,
                    bestMoves = 0,
                    stars = 0
                };
                levelsData.Add(levelData);

            }
        }
        else
        {
            levelsData= JsonUtility.FromJson<LevelsDataWrapper>(jsonData).levelsData;
            if(levelsData.Count<levels.Count)
            {
                for(int i = levelsData.Count; i < levels.Count; i++)
                {
                    LevelData levelData = new LevelData
                    {
                        levelID = i,
                        isCompleted = i <= lastLevel,
                        bestMoves = 0,
                        stars = 0
                    };
                    levelsData.Add(levelData);
                }
            }
        }


            DontDestroyOnLoad(gameObject);
        for (int i = 0; i < levels.Count; i++)
        {
            levels[i].levelID = i;
            

            
        }
    }

    private void Start()
    {
        if (diceBackground != null)
        {
            diceBackground.SetFloat("_MaxLevel", levels.Count - 1);
        }
    }

    public void LoadLevel(int v)
    {
        if (diceBackground!=null)
        {
            diceBackground.SetFloat("_Level", v);
        }
        currentLevel= levels[v];
        currentLevelIndex = v;
        currentLevel.DrawBoard();
        UIManager.Instance.SetLevelName(currentLevel.name);
#if UNITY_EDITOR
        Debug.Log(JsonUtility.ToJson(levels[v]));
#endif
        int lastLevel= PlayerPrefs.GetInt("LastLevel", 0);
        
        PlayerPrefs.Save();
    }


    public void LoadCurrentLevel()
    {
        if (testMode)
        {
            if(currentLevelIndex==-1)
                LoadLevel(levels.Count-1);
            else
                LoadLevel(currentLevelIndex);
            return;
        }
        int levelIndex = PlayerPrefs.GetInt("CurrentLevel", 0); // 0 = default si no existe
        Debug.Log("Current level loaded as: " + levelIndex);
        LoadLevel(levelIndex);
    }

    public void SaveLevelData(LevelData levelData)
    {
        levelsData[levelData.levelID]= levelData;
        
        string jsonData = JsonUtility.ToJson(new LevelsDataWrapper( levelsData));

        Debug.Log("Saving level data: " + jsonData);
        PlayerPrefs.SetString("LevelsData", jsonData);
        PlayerPrefs.Save();
    }
    public void LoadNextLevel()
    {

        
        if (currentLevelIndex + 1 < levels.Count)
        {
            LoadLevel(currentLevelIndex + 1);
        }
        else
        {
            PlayerPrefs.SetInt("LastLevel", levels.Count-1);
            
            LoadLevel(0);
            Debug.Log("No more levels to load!");
        }
    }
    public void ResetLevel()
    {
        LoadLevel(currentLevelIndex);
    }

    public Level GetLevel(int levelIndex)
    {
        return levels[levelIndex];
    }
}

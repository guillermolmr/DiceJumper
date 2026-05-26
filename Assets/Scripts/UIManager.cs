using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    public static UIManager Instance { get; private set; }

    [Header("MenuButtons")]
    public Sprite[] DiceSprites = new Sprite[6];


    [Header("Gameplay UI")]
    [SerializeField] GameObject gameplayUI;
    [SerializeField] GameObject cameraGameplay;
    [SerializeField] TextMeshProUGUI moves;
    [SerializeField] TextMeshProUGUI level;
    [SerializeField] GameObject gameplayBackground;

    

    [Header("Level Completed")]
    [SerializeField] GameObject LevelCompleted;
    [SerializeField] GameObject[] starGameObject=new GameObject[3];
    [SerializeField] TextMeshProUGUI results;
    [SerializeField] TextMeshProUGUI LevelCompletedText;


    [Header("Level selection")]
    [SerializeField] GameObject levelSelectionUI;

    [Header("Main Menu")]
    [SerializeField] GameObject mainMenuUI;
    [SerializeField] GameObject PlayButton;
    [SerializeField] GameObject ContinueButton;
    [SerializeField] GameObject LevelsButton;
    [SerializeField] GameObject TittleGO;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void DisplayLevelCompleted(int moves, int stars, int bestMoves,int designedMoves,bool isGameCompleted)
    {
        LevelCompleted.SetActive(true);
        for (int i = 0; i < starGameObject.Length; i++)
        {
            starGameObject[i].SetActive(i < stars);
        }


        if (isGameCompleted)
        {
            LevelCompletedText.text = "Congratulations! You've completed the game!";
        }
        else
        {
            LevelCompletedText.text = "Level Completed!";
        }
            results.text= $"Moves: {moves}\nBest Moves: {(bestMoves == 0 ? "N/A" : bestMoves.ToString())}\nDesigned Moves: {(designedMoves == 0 ? "N/A" : designedMoves.ToString())}";
    }

    public void ResetLevel()
    {
        LevelCompleted.SetActive(false);
        LevelManager.instance.ResetLevel();
    }
    public void LoadNextLevel()
    {
        LevelManager.instance.LoadNextLevel();
        LevelCompleted.SetActive(false);
    }

    public void SetMoves(int value)
    {
        moves.text = value.ToString();
    }
    public void SetLevelName(string levelName)
    {
        level.text = levelName;
    }
    public void ShowLevelSelection()
    {
        levelSelectionUI.SetActive(true);
        mainMenuUI.SetActive(false);
        TittleGO.SetActive(false);
        gameplayUI.SetActive(false);
        cameraGameplay.SetActive(false);


    }
    public void ReturnMainMenu()
    {
        mainMenuUI.SetActive(true);
        TittleGO.SetActive(true);
        gameplayUI.SetActive(false);
        LevelCompleted.SetActive(false);
        cameraGameplay.SetActive(false);
        cameraGameplay.SetActive(false);
        levelSelectionUI.SetActive(false);
    }

    public void CheckIfLevelSavedToContinue()
    {
        int levelIndex = PlayerPrefs.GetInt("CurrentLevel", -1);

        ContinueButton.SetActive(levelIndex != -1);
    }

    public void StartGame()
    {
        mainMenuUI.SetActive(false);
        TittleGO.SetActive(false);
        levelSelectionUI.SetActive(false);
        gameplayUI.SetActive(true);
        cameraGameplay.SetActive(true);

        LevelManager.instance.LoadLevel(0);
    }
    public void ContinueGame()
    {
        mainMenuUI.SetActive(false);
        TittleGO.SetActive(false);
        levelSelectionUI.SetActive(false);
        gameplayUI.SetActive(true);
        cameraGameplay.SetActive(true);

        LevelManager.instance.LoadCurrentLevel();
    }

    public void LoadLevel(int levelIndex)
    {
        mainMenuUI.SetActive(false);
        TittleGO.SetActive(false);
        levelSelectionUI.SetActive(false);
        gameplayUI.SetActive(true);
        cameraGameplay.SetActive(true);

        LevelManager.instance.LoadLevel(levelIndex);
    }
}

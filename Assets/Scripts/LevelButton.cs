
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [SerializeField] GameObject lockImage;
    [SerializeField] GameObject starsParent;
    [SerializeField] GameObject[] stars= new GameObject[3];
    [SerializeField] TextMeshProUGUI levelNameText;
    [SerializeField] TextMeshProUGUI bestMoves;
    [SerializeField] Button mybutton;

    private void Awake()
    {
        mybutton = GetComponent<Button>();
    }

    [SerializeField] int levelIndex;
    public void Init(LevelData levelData)
    {
        this.levelIndex = levelData.levelID;
        gameObject.SetActive(true);
        if(levelData.isCompleted || levelData.levelID == 0 || PlayerPrefs.GetInt("LastLevel", 0) == levelData.levelID)
        {
#if !UNITY_EDITOR
            mybutton.interactable = true;
            
#endif
            lockImage.SetActive(false);
            starsParent.SetActive(true);
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].SetActive(false);
            }
            for (int i = 0; i < levelData.stars; i++)
            {
                stars[i].SetActive(true);
            }
            bestMoves.text = levelData.bestMoves.ToString() + " Moves";
            
        }
        else
        {

#if !UNITY_EDITOR
            
            mybutton.interactable= false;
#endif
            lockImage.SetActive(true);
            starsParent.SetActive(false);
            bestMoves.text = "";


        }

        levelNameText.text = LevelManager.instance.levels[levelData.levelID].name;

    }


    public void OnClick()
    {

        UIManager.Instance.LoadLevel(levelIndex);
    }
}

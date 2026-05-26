
using UnityEngine;

public class LevelSelectorMenu : MonoBehaviour
{
    [SerializeField] GameObject LevelButton;
    [SerializeField] Transform content;
    
    private void OnEnable()
    {
        for(int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }
        var levels = LevelManager.instance.levelsData;
        for (int i = 0; i < levels.Count; i++)
        {
            GameObject button = Instantiate(LevelButton, content);
            button.GetComponent<LevelButton>().Init(levels[i]);
        }
    }
}

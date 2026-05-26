using UnityEngine;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour
{
    [SerializeField] Image diceImage;

    private void Start()
    {
        int index = transform.GetSiblingIndex();
        diceImage.sprite = UIManager.Instance.DiceSprites[index%6];
        
    }
}

using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : SpecialTile
{
    public List<Vector2Int> yellowTilePositions=new List<Vector2Int>();
    public List<GameObject> yellowTileGameObjects=new List<GameObject>();
    public bool state=false;
    public override void TriggerTilePower()
    {
        base.TriggerTilePower();
        SwitchTiles();

    }
    //TODO: Animate this 
    public void SwitchTiles()
    {
        state = !state;
        
        if (state)
        {
            ActivateTiles();

        }
        else
        {
            DeactivateTiles();
        }

        DiceManager.instance.DisplayValidTiles();
    }

    public void ActivateTiles()
    {
        BoardManager boardManager = BoardManager.instance;
        for (int i = 0; i < yellowTilePositions.Count; i++)
        {

            if (!boardManager.validTiles.Add(yellowTilePositions[i]))
            {
                Debug.Log("Couldn't add valid tile " + yellowTilePositions[i].ToString());
            }
            yellowTileGameObjects[i].SetActive(true);
        }
        
    }

    public void DeactivateTiles()
    {
        BoardManager boardManager = BoardManager.instance;
        for (int i = 0; i < yellowTilePositions.Count; i++)
        {
            if (!boardManager.validTiles.Remove(yellowTilePositions[i]))
            {
                //Debug.Log("Couldn't remove valid tile "+ yellowTilePositions[i].ToString());
            }
            yellowTileGameObjects[i].SetActive(false);
        }
    }
}

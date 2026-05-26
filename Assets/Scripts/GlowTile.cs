using UnityEngine;

public class GlowTile : MonoBehaviour
{
    public Vector2Int position;

    bool isDice= false;

    
    private void OnMouseDown()
    {
        //Debug.Log("Tile selected: " + position); 
        BoardManager.instance.OnTileSelected(position);
    }
}

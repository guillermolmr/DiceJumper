using UnityEngine;

public class SpecialTile : MonoBehaviour
{

    public virtual void TriggerTilePower()
    {
        Debug.Log("Trigger " + name);
        
    }

    public virtual void TriggerOnLift()
    {
        Debug.Log("Left " + name);
    }
}

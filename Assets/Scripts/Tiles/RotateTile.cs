using System.Collections;
using UnityEngine;

public class RotateTile : SpecialTile
{
    public bool rotateRight;
    public float animationDuration = 1f;
    public override void TriggerTilePower()
    {
        base.TriggerTilePower();

        if(rotateRight)
        {
            Rotate(90f);
        }
        else
        {
            Rotate(-90f);
        }

        
    }
    //TODO: Animate this
    public void Rotate(float angle)
    {
        /*
        Transform dice = DiceManager.instance.transform;
        dice.Rotate(Vector3.up, angle, Space.World);
        DiceManager.instance.DisplayValidTiles();
        Debug.Log("Rotating dice "+angle.ToString());
        */

        StartCoroutine(RotationAnimation(angle));
    }
    
    IEnumerator RotationAnimation(float angle)
    {
        float cd = 0f;

        float segment = animationDuration / 3f;
        float iTime = 1f / segment;

        Transform dt= DiceManager.instance.transform;
        Vector3 origin = dt.position;
        Vector3 target = origin + Vector3.up;
        while (cd < 1f)
        {
            cd += iTime * Time.deltaTime;
            dt.position = Vector3.LerpUnclamped(origin, target, GLM.EaseOutBack(cd));
            yield return null;
        }

        Quaternion initialRotation = dt.rotation;
        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.up) * initialRotation;
        cd = 0f;
        while (cd < 1f)
        {
            cd += iTime * Time.deltaTime;
            dt.rotation = Quaternion.LerpUnclamped(initialRotation, targetRotation, GLM.EaseOutBack(cd));
            yield return null;
        }
        dt.rotation = targetRotation;
        cd = 1f;
        while (cd > 0f)
        {
            cd -= iTime * Time.deltaTime;
            dt.position = Vector3.LerpUnclamped(origin, target, GLM.EaseOutBack(cd));
            yield return null;
        }
        dt.position = origin;

        DiceManager.instance.DisplayValidTiles();
    }
     
    
}

using System.Collections;
using UnityEngine;

public class GlassTile : SpecialTile
{

    public override void TriggerOnLift()
    {
        base.TriggerOnLift();
        Crumble();
    }

    public override void TriggerTilePower()
    {
        base.TriggerTilePower();
        DiceManager.instance.DisplayValidTiles();
    }

    public void Crumble()
    {
        
        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        BoardManager.instance.validTiles.Remove(pos);
        StartCoroutine(CrumbleAnim());
    }

    IEnumerator CrumbleAnim()
    {
        yield return new WaitForSeconds(0.5f);

        gameObject.SetActive(false);
    }
}

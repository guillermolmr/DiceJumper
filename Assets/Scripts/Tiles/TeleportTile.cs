using System.Collections;
using UnityEngine;

public class TeleportTile : SpecialTile
{
    public Vector3 target;
    public float durationAnimation;
    public override void TriggerTilePower()
    {
        base.TriggerTilePower();

        DoTeleportation();

    }


    //TODO: Animate this
    void DoTeleportation()
    {
        //DiceManager.instance.transform.position = target;
        //DiceManager.instance.DisplayValidTiles();

        StartCoroutine(TeleportAnimation());
    }


    IEnumerator TeleportAnimation()
    {
        float segment = durationAnimation / 4f;
        Transform dt=DiceManager.instance.transform;
        float cd = 0f;
        Transform st = DiceManager.instance.portalEffectSphere;
        Material effect = DiceManager.instance.portalEffectMaterial;
        Vector3 initialScale = dt.localScale;
        Vector3 targetScale = Vector3.one * 0.01f;
        float itime = 1f / segment;

        effect.SetFloat("_Strength", 0f);
        st.gameObject.SetActive(true);
        
        while (cd < 1f)
        {
            cd += Time.deltaTime * itime;
            effect.SetFloat("_Strength", cd);
            
            yield return null;
        }
        effect.SetFloat("_Strength", 1f);
        cd = 0f;
        while (cd < 1f)
        {
            cd += Time.deltaTime * itime;
            dt.localScale = Vector3.Lerp(initialScale, targetScale, cd);

            yield return null;
        }
        cd = 0f;
        dt.position = target;
        while (cd < 1f)
        {
            cd += Time.deltaTime * itime;
            dt.localScale = Vector3.Lerp( targetScale, initialScale, cd);

            yield return null;
        }
        cd = 1f;
        while (cd > 0f)
        {
            cd -= Time.deltaTime * itime;
            effect.SetFloat("_Strength", cd);

            yield return null;
        }
        effect.SetFloat("_Strength", 0f);
        st.gameObject.SetActive(false);
        DiceManager.instance.DisplayValidTiles();
    }
}

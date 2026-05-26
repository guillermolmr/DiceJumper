using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class DiceManager : MonoBehaviour
{

    public static DiceManager instance;
    public float moveDistance = 1f;
    public float duration = 0.2f;

    [SerializeField] int numMoves;

    [SerializeField] List<int> steps = new List<int>();

    public Transform portalEffectSphere;
    public Material portalEffectMaterial;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        if(portalEffectSphere == null && portalEffectSphere != null)
        {
            portalEffectMaterial = portalEffectSphere.gameObject.GetComponent<MeshRenderer>().material;

        }

        gameObject.SetActive(false);
        
    }
    


    private static readonly (Vector3 axis, int face)[] FaceAxes = new[]
    {
        (Vector3.forward, 1),
        (Vector3.right,   2),
        (Vector3.up,      4),
        (Vector3.down,    3),
        (Vector3.left,    5),
        (Vector3.back,    6),
    };

    
    public static Vector3 GetAxisOfDots(int dots)
    {
        Vector3 up= -FaceAxes[dots-1].axis;
        return up;
    }
    public int GetTopFace()
    {
        int topFace = -1;
        float maxDot = float.MinValue;

        foreach (var (localAxis, face) in FaceAxes)
        {
            Vector3 worldAxis = transform.TransformDirection(localAxis);
            float dot = Vector3.Dot(worldAxis, Vector3.up);

            if (dot > maxDot)
            {
                maxDot = dot;
                topFace = face;
            }
        }
        return topFace;
    }

    public static int GetTopFace(Transform tileTransform)
    {
        int topFace = -1;
        float maxDot = float.MinValue;

        foreach (var (localAxis, face) in FaceAxes)
        {
            Vector3 worldAxis = tileTransform.TransformDirection(localAxis);
            float dot = Vector3.Dot(worldAxis, Vector3.up);

            if (dot > maxDot)
            {
                maxDot = dot;
                topFace = face;
            }
        }
        return topFace;
    }

    public void MoveTo(Vector3 targetPosition)
    {
        BoardManager.instance.DisableGlowTiles();
        StartCoroutine(RollTo(targetPosition));
        numMoves++;
    }

    public void SetPositionAndDots(Vector3 position, int dots, bool resetMoves=true)
    {

        transform.position = position;
        transform.rotation = Quaternion.identity;
        transform.up= GetAxisOfDots(dots);
        if (resetMoves)
        {
            numMoves = 0;
            UIManager.Instance.SetMoves(numMoves);
            steps.Clear();

        }
        //gameObject.SetActive(true);
    }



    IEnumerator RollTo(Vector3 targetPosition)
    {
        {
            SpecialTile specialTile = BoardManager.instance.GetSpecialTile(transform.position);
            if (specialTile != null)
            {
                specialTile.TriggerOnLift();
            }
        }
        

        Vector3 dir= SnapToCardinal((targetPosition - transform.position).normalized);

        Vector3 startPos = transform.position;

        Vector3 relativeDir= transform.InverseTransformDirection(dir);

        // Eje de rotación
        Vector3 axis = Vector3.Cross(Vector3.up, dir);

        
        float cd = 0f;
        float itime= 1f / duration;
        Quaternion currentRotation = transform.rotation;

        Quaternion targetRotation = Quaternion.AngleAxis(90f,axis)* currentRotation;

        while (cd<1f)
        {
            cd+= itime * Time.deltaTime;
            float step = 90f *cd;
            
            //transform.eulerAngles = currentRotation + r;

            transform.rotation=Quaternion.Lerp(currentRotation, targetRotation, cd);
            transform.position = Vector3.Lerp(startPos, targetPosition, cd);

            yield return null;
        }
        

        Vector3 rot = transform.eulerAngles;
        rot.x = Mathf.Round(rot.x / 90f) * 90f;
        rot.y = Mathf.Round(rot.y / 90f) * 90f;
        rot.z = Mathf.Round(rot.z / 90f) * 90f;

        transform.eulerAngles = rot;
        transform.position = targetPosition;
        int dots = GetTopFace();
        steps.Add(dots);
        UIManager.Instance.SetMoves(numMoves);

        if (BoardManager.instance.CheckLevelCompleted(transform.position, dots))
        {
            LevelData levelData = LevelManager.instance.levelsData[LevelManager.instance.currentLevel.levelID];
            //int bestMoves = LevelManager.instance.currentLevel.bestMoves;

#if UNITY_EDITOR
            if (LevelManager.instance.currentLevel.designedMoves == 0 || numMoves <= LevelManager.instance.currentLevel.designedMoves)
            {
                LevelManager.instance.currentLevel.bestSteps = new List<int>(steps);
                LevelManager.instance.currentLevel.designedMoves = numMoves;
            }
            EditorUtility.SetDirty(LevelManager.instance.currentLevel);
#endif
            int designedMoves = LevelManager.instance.currentLevel.designedMoves;
            int stars = 1;
            if (designedMoves == 0 || numMoves <= designedMoves)
            {
                stars = 3;
            }
            else if (numMoves <= designedMoves + designedMoves)
            {
                stars = 2;
            }

            if (levelData.bestMoves == 0 || numMoves < levelData.bestMoves)
                levelData.bestMoves = numMoves;

            levelData.isCompleted = true;
            if(stars > levelData.stars)
                levelData.stars = stars;
            if (levelData.bestMoves == 0 || numMoves < levelData.bestMoves)
                levelData.bestMoves = numMoves;

            LevelManager.instance.SaveLevelData(levelData);
            UIManager.Instance.DisplayLevelCompleted(numMoves, stars, levelData.bestMoves, designedMoves, levelData.levelID == LevelManager.instance.levels.Count - 1);
            if(levelData.levelID<LevelManager.instance.levels.Count-1)
                PlayerPrefs.SetInt("CurrentLevel", levelData.levelID + 1);
            
        }
        else
        {
            SpecialTile specialTile = BoardManager.instance.GetSpecialTile(transform.position);
            if (specialTile != null)
            {
                //Debug.Log("SpecialTile: " + specialTile.ToString());
                specialTile.TriggerTilePower();
            }
            else
            {
                //Debug.Log("Not a special tile: ");
                
                BoardManager.instance.DisplayValidTiles(transform.position, dots);
            }
        }
            

    }


    public void DisplayValidTiles()
    {
        int dots = GetTopFace();
        BoardManager.instance.DisplayValidTiles(transform.position, dots);
    }
    public static Vector3 SnapToCardinal(Vector3 direction)
    {
        Vector3[] cardinals = {
        Vector3.right, Vector3.left,
        Vector3.up,    Vector3.down,
        Vector3.forward, Vector3.back,
    };

        Vector3 best = cardinals[0];
        float maxDot = float.MinValue;

        foreach (Vector3 cardinal in cardinals)
        {
            float dot = Vector3.Dot(direction.normalized, cardinal);
            if (dot > maxDot)
            {
                maxDot = dot;
                best = cardinal;
            }
        }

        return best;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
        

    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using static CustomLevel;

public class BoardManager : MonoBehaviour
{

    public static BoardManager instance;
    [Header("Animations")]
    public Vector3 offset = new Vector3(0, 10f, 0);
    public float durationBuildBoard = 1f;


    [Header("Tile prefabs")]
    [SerializeField] GameObject blackTile;
    [SerializeField] GameObject whiteTile;
    [SerializeField] GameObject yellowTile;
    [SerializeField] GameObject teleportTile;
    [SerializeField] GameObject presurePlateTile;
    [SerializeField] GameObject teleportTargetTile;
    [SerializeField] GameObject rotateRightTile;
    [SerializeField] GameObject rotateLeftTile;
    [SerializeField] GameObject glassTile;


    public GameObject FreeGoal;
    public GameObject DotsGoal;
    public GameObject WrongDots;

    public HashSet<Vector2Int> validTiles;
    [Header("Valid tiles")]
    [SerializeField] public GlowTile[] glowTiles=new GlowTile[4];
    
    
    public bool isBoardReady { get; private set; }



    [SerializeField] bool editMode;

    Vector2Int goalPosition;
    int goalDots;

    public Level testLevel;

    Dictionary<Vector2Int, SpecialTile> specialTiles;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        validTiles = new HashSet<Vector2Int>();
        specialTiles = new Dictionary<Vector2Int, SpecialTile>();



    }


    
    public void DeleteBoard()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        DiceManager.instance.gameObject.SetActive(false);
        validTiles.Clear();
        specialTiles.Clear();
        DisableGlowTiles();
        FreeGoal.SetActive(false);
        DotsGoal.SetActive(false);
        WrongDots.SetActive(false);
    }

    public void ExplodeBoard()
    {
        //TODO: Add explosion effect here
    }
    public void MakeBoard(int width, int height)
    {
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                
                CreateTile(new Vector2Int(x, y), (TileType)(((x + y) % 2))+1);
            }
        }
    }




    public void CreateTile(Vector2Int position, TileType tileType, bool force=false)
    {
        

        if (!validTiles.Add(position))
        {
            if(!force)
                return;

        }
            GameObject tileToSpawn = null;
        switch (tileType)
        {
            case TileType.Black:
                tileToSpawn = blackTile;
                break;
            case TileType.White:
                tileToSpawn = whiteTile;
                break;
            case TileType.TargetTeleport:
                tileToSpawn = teleportTargetTile;
                break;
            case TileType.RotateLeft:
                tileToSpawn = rotateLeftTile;
                break;
            case TileType.RotateRight:
                tileToSpawn = rotateRightTile;
                break;

                
        }
        if(tileToSpawn == null)
        {
            Debug.LogError("No Tile to spawn: "+ tileType.ToString());
            return;
        }
       
        Instantiate(tileToSpawn, new Vector3(position.x, 0, position.y)+offset, Quaternion.identity,transform);

        
        

    }

    public void CreateTile(Vector2Int position, CustomLevel.SpecialTile specialTile)
    {
        
        

        if (!validTiles.Add(position))
        {
            Debug.LogError($"There is already a tile at pos{position.ToString()}. Can't create {specialTile.ToString()}");
            return;
        }

        switch (specialTile.type)
        {
            
            case TileType.Yellow:
                validTiles.Remove(position);
                break;
            case TileType.Teleport:
                SpawnTeleport(position, specialTile); 
                break;
            case TileType.PressurePlate:
                SpawnPressurePlate(position,specialTile);
                break;
            
            case TileType.RotateLeft:
                SpawnRotateTile(position, false);
                break;
            case TileType.RotateRight:
                SpawnRotateTile(position, true);
                break;
            case TileType.Glass:
                SpawnGlass(position);
                break;
            default:
                CreateTile(position, specialTile.type,true);
                break;
        }
        

    }
    void SpawnRotateTile(Vector2Int position, bool isRight)
    {
        GameObject tile = Instantiate(isRight? rotateRightTile:rotateLeftTile, new Vector3(position.x, 0, position.y) + offset, Quaternion.identity, transform);
        SpecialTile st = tile.GetComponent<SpecialTile>();

        if (st == null)
        {
            Debug.LogError("No SpecialTile component on RotateTile at " + position.ToString());
        }
        specialTiles.Add(position, st);
    }

    void SpawnGlass(Vector2Int position)
    {
        GameObject tile = Instantiate(glassTile, new Vector3(position.x, 0, position.y) + offset, Quaternion.identity, transform);
        SpecialTile st = tile.GetComponent<SpecialTile>();

        if (st == null)
        {
            Debug.LogError("No SpecialTile component on RotateTile at " + position.ToString());
        }
        specialTiles.Add(position, st);
    }
    
    void SpawnTeleport(Vector2Int position, CustomLevel.SpecialTile specialTile)
    {
        GameObject tile = Instantiate(teleportTile, new Vector3(position.x, 0, position.y) + offset, Quaternion.identity, transform);
        TeleportTile tt= tile.GetComponent<TeleportTile>();
        if(specialTile.targets != null && specialTile.targets.Count>0)
        {
            Vector2Int pos = specialTile.targets[0];
            tt.target= new Vector3(pos.x,1f, pos.y);
        }
        else
        {
            Debug.LogError("Teleport tile lacks target");
        }

        specialTiles.Add(position, tt);

    }
    void SpawnPressurePlate(Vector2Int position, CustomLevel.SpecialTile specialTile)
    {
        GameObject tile= Instantiate(presurePlateTile, new Vector3(position.x, 0, position.y) + offset, Quaternion.identity, transform);
        PressurePlate pressurePlate=tile.GetComponent<PressurePlate>();
        bool isActive = specialTile.value != 0;
        
        for(int i = 0; i<specialTile.targets.Count; i++)
        {
            GameObject yellow= Instantiate(yellowTile, new Vector3(specialTile.targets[i].x, 0, specialTile.targets[i].y) + offset, Quaternion.identity, transform);
            yellow.SetActive(false);
            pressurePlate.yellowTilePositions.Add(specialTile.targets[i]);
            pressurePlate.yellowTileGameObjects.Add(yellow);
        }
        if (isActive)
        {
            pressurePlate.ActivateTiles();
        }
        else
        {
            pressurePlate.DeactivateTiles();
        }

            specialTiles.Add(position, pressurePlate);
    }

    public List<Vector2Int> GetReachableTiles(Vector2Int origin, int maxSteps)
    {
        var reachable = new List<Vector2Int>();
        if (editMode)
        {
            foreach (Vector2Int dir in Directions)
            {
                reachable.Add(origin+ dir * maxSteps);
            }

            return reachable;
        }
        foreach (Vector2Int dir in Directions)
        {
            for (int i = 1; i <= maxSteps; i++)
            {
                Vector2Int candidate = origin + dir * i;

                if (!validTiles.Contains(candidate))
                    break;

                if (i == maxSteps)
                    reachable.Add(candidate);
            }
        }
        //Debug.Log(reachable.Count + " reachable tiles from " + origin + " with max steps " + maxSteps);
        return reachable;
    }

    private static readonly Vector2Int[] Directions = new[]
    {
    Vector2Int.up,
    Vector2Int.down,
    Vector2Int.left,
    Vector2Int.right,
};

    

    public void OnTileSelected(Vector2Int position)
    {

        if (editMode)
        {
            Vector3 dicePos=DiceManager.instance.transform.position; ;
            Vector2Int origin = new Vector2Int(Mathf.RoundToInt(dicePos.x), Mathf.RoundToInt(dicePos.z));
            int distance = Mathf.Abs(position.x - origin.x) + Mathf.Abs(position.y - origin.y);
            Vector2Int dir = position - origin;
            dir/= distance;
            Vector2Int last = origin;
            for (int i = 0; i <= distance; i++)
            {
                CreateTile(last, (TileType)(1+Mathf.Abs(last.x+last.y)%2));
                last += dir;
            }

        }
        DiceManager.instance.MoveTo(new Vector3(position.x, 1.0f, position.y));


    }


    public void DisableGlowTiles()
    {
        foreach(var glowTile in glowTiles)
        {
            glowTile.gameObject.SetActive(false);
        }
        WrongDots.SetActive(false);
    }
    public SpecialTile GetSpecialTile(Vector3 position)
    {
        SpecialTile special=null;

        Vector2Int origin = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
        if (specialTiles.ContainsKey(origin))
        {
            special = specialTiles[origin];
        }
        return special;
    }
    public void DisplayValidTiles(Vector3 position,int steps)
    {
        Vector2Int origin= new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
        var tiles = GetReachableTiles(origin, steps);
        Quaternion diceRotation= DiceManager.instance.transform.rotation;
        for (int i = 0; i<tiles.Count;i++)
        {
            glowTiles[i].transform.position = new Vector3(tiles[i].x, 0.2f, tiles[i].y);
            glowTiles[i].gameObject.SetActive(true);
            glowTiles[i].position = tiles[i];


            Vector3 dir = DiceManager.SnapToCardinal((glowTiles[i].transform.position - position).normalized);
            Vector3 axis = Vector3.Cross(Vector3.up, dir);
            glowTiles[i].transform.rotation= Quaternion.AngleAxis(90f, axis) * diceRotation;


            if (tiles[i] == goalPosition && goalDots!=-1)
            {
                
                if (goalDots == DiceManager.GetTopFace(glowTiles[i].transform))
                {
                    FreeGoal.transform.position= new Vector3(tiles[i].x, 1.1f, tiles[i].y);
                    FreeGoal.SetActive(true);
                }
                else
                {
                    WrongDots.transform.position = new Vector3(tiles[i].x, 1.1f, tiles[i].y);
                    WrongDots.SetActive(true);
                }
            }
        }
    }

    public void DrawGoal(Vector2Int position, int dots)
    {
        goalDots = dots;
        goalPosition = position;
        if (dots<=-1)
        {
            FreeGoal.transform.position = new Vector3(position.x, 1.01f, position.y);
            FreeGoal.SetActive(true);
        }
        else
        {
            DotsGoal.transform.position = new Vector3(position.x, 0.1f, position.y);
            DotsGoal.transform.up= DiceManager.GetAxisOfDots(dots);

            DotsGoal.SetActive(true);
        }
    }
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }

    }

    public bool CheckLevelCompleted(Vector3 position, int dots)
    {
        Vector2Int origin = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));

        if (origin == goalPosition)
        {
            if (goalDots == -1 || goalDots == dots)
            {
                return true;
            }
            else
            {
                WrongDots.transform.position = new Vector3(position.x, 1f, position.z);
                WrongDots.SetActive(true);
            }
        }


        return false;

    }

    public void LaunchBoardAnimation(Action OnAnimationEnd=null)
    {
        StartCoroutine(BoardAnimationEnter(OnAnimationEnd));
    }

    IEnumerator BoardAnimationEnter(Action OnAnimationEnd)
    {
        yield return null;


        Transform[] tts = new Transform[transform.childCount];
        
        Vector3[] initialPositions= new Vector3[transform.childCount];
        Vector3[] targetPositions= new Vector3[transform.childCount];
        float[] cds= new float[transform.childCount];


        for(int i=0;i<transform.childCount; i++)
        {
            Transform tc= transform.GetChild(i);
            tts[i] = tc;
            
        }
        tts=tts.OrderBy(v => Mathf.Round(v.position.x + v.position.z))
            .ThenBy(v => Mathf.Round(v.position.x))
            .ToArray();
        for(int i = 0; i < tts.Length; i++)
        {
            initialPositions[i] = tts[i].position;
            targetPositions[i] = tts[i].position;
            targetPositions[i].y = 0f;
        }

        float timeTile = durationBuildBoard;
        float waitTime = timeTile * 0.03f;
        
        float iTime = 1f / timeTile;
        float waitCD = 0f;
        int header = 0;
        for (int i = 0; header < tts.Length-1; )
        {
            for(int j = header; j <= i; j++)
            {
                
                cds[j] += Time.deltaTime * iTime;
                if (cds[j] >= 1f)
                {
                    header++;
                    cds[j] = 1f;
                }
                tts[j].position = Vector3.LerpUnclamped(initialPositions[j], targetPositions[j], GLM.EaseOutBack(cds[j]));
                //yield return null;
            }
            yield return null;
            waitCD += Time.deltaTime;
            if(waitCD > waitTime && i< tts.Length-1f)
            {
                waitTime = 0f;
                i++;

            }

        }

        OnAnimationEnd?.Invoke();


    }
}

using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static CustomLevel;
using static DiceSolver;
[CreateAssetMenu(fileName = "New Custom Level", menuName = "Levels/Custom Level")]
public class CustomLevel:Level
{

    public int width;
    public int height;
    public Vector2Int startPosition;
    public Vector2Int goalPosition;
    public int startingDots;
    public int goalDots;
    public bool[] floorTiles;
    public List<SpecialTile> specialTiles;
    
    public override void DrawBoard()
    {
        base.DrawBoard();

#if UNITY_EDITOR
        List<int> solution = DiceSolverSpecial.Solve(this, RectangularLevel.fromTopDots[startingDots-1], goalDots);
        if (solution != null)
        {

            string sol = "Solution: ";

            for (int i = 0; i < solution.Count; i++)
            {
                sol += solution[i].ToString();
                sol += ", ";
            }
            Debug.Log(sol);

        }
        else
        {
            Debug.Log("No solution found");
        }
#endif
        BoardManager boardManager = BoardManager.instance;
        DiceManager diceManager = DiceManager.instance;

        foreach (SpecialTile tile in specialTiles) {
            BoardManager.instance.CreateTile(tile.position, tile);
            
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (IsFloor(x, y))
                { 
                    BoardManager.instance.CreateTile(new Vector2Int(x, y), (TileType)(((x + y) % 2)) + 1);
                }   
            }
        }

        
        if (!IsFloor(goalPosition.x, goalPosition.y))
        {
            BoardManager.instance.CreateTile(goalPosition, (TileType)(((goalPosition.x + goalPosition.y) % 2)) + 1);
        }
        if (!IsFloor(startPosition.x, startPosition.y))
        {
            BoardManager.instance.CreateTile(startPosition, (TileType)(((startPosition.x + startPosition.y) % 2)) + 1);
        }

            diceManager.SetPositionAndDots(new Vector3(startPosition.x, 1f, startPosition.y), startingDots);
        boardManager.LaunchBoardAnimation(() =>
        {
            boardManager.DrawGoal(goalPosition, goalDots);
            diceManager.gameObject.SetActive(true);
            boardManager.DisplayValidTiles(diceManager.transform.position, startingDots);
            
        });

        //diceManager.SetPositionAndDots(new Vector3(startPosition.x, 1f, startPosition.y), startingDots);
        //diceManager.DisplayValidTiles();
    }

    public bool IsFloor( int x, int y)
    {
            int pos = y * width + x;

        return floorTiles != null && x >= 0&&y>=0 && floorTiles.Length > pos && floorTiles[pos];    
        
    }
    public void SetFloor(int x, int y, bool isFloor)
    {
        int pos = y * width + x;
        if (floorTiles == null || floorTiles.Length <= pos)
        {
            Debug.LogWarning($"Floor tiles array is too small for position ({x}, {y}). ");
            //Array.Resize(ref floorTiles, pos + 1);
        }
        else
        {
            floorTiles[pos] = isFloor;

        }
    }

    public void ReverseFloor(int x, int y)
    {
        int pos = y * width + x;
        if (floorTiles == null || floorTiles.Length <= pos)
        {
            Debug.LogWarning($"Floor tiles array is too small for position ({x}, {y}).");
            //Array.Resize(ref floorTiles, pos + 1);
        }
        else
        {
            floorTiles[pos] = !floorTiles[pos];

        }
    }

    public SpecialTile GetSpecialTileAtPosition(Vector2Int position)
    {
        return specialTiles.Find(tile => tile.position == position);
    }

    [Serializable]
    public class SpecialTile
    {
        public TileType type;
        public Vector2Int position;

        // For yellow tiles is the initial state
        public int value;
        
        public List<Vector2Int> targets;
    }
    
     
}

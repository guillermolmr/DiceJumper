using System.Collections.Generic;
using UnityEngine;
using static DiceSolver;

[CreateAssetMenu(fileName = "New Rectangular Level", menuName = "Levels/Rectangular Level")]
public class RectangularLevel : Level
{
    public int width;
    public int height;
    public Vector2Int startPosition;
    public Vector2Int goalPosition;
    public int startingDots;
    public int goalDots;

    public RectangularLevel(int width, int height, Vector2Int startPosition, Vector2Int goalPosition, int startingDots = -1, int goalDots = -1)
    {
        this.width = width;
        this.height = height;
        this.startingDots = startingDots;
        this.startPosition = startPosition;
        this.goalPosition = goalPosition;
        this.goalDots = goalDots;
    }

    public override void DrawBoard()
    {
        base.DrawBoard();
        int size = width * height;
        bool[] tiles= new bool[size];
        for(int i = 0; i < size; i++)
        {
            tiles[i]=true;
        }
#if UNITY_EDITOR
        List<int> solution = DiceSolver.Solve(
        tiles,
        width, height,
        startPosition.x, startPosition.y,
        goalPosition.x, goalPosition.y,
        fromTopDots[startingDots-1],
        goalDots
        );
        if (solution != null)
        {

            string sol = "Solution: ";

            for(int i=0; i < solution.Count; i++)
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

        boardManager.MakeBoard(width, height);
        diceManager.SetPositionAndDots(new Vector3(startPosition.x, 1f, startPosition.y), startingDots);
        boardManager.LaunchBoardAnimation(() =>
        {
            boardManager.DrawGoal(goalPosition, goalDots);
            
            diceManager.gameObject.SetActive(true);
            boardManager.DisplayValidTiles(diceManager.transform.position, startingDots);
        });
        


    }


    public static DiceOrientation[] fromTopDots =
        {
            new DiceOrientation // 1
            {
                Top = 1,
                Bottom = 6,
                North = 3,
                South = 4,
                East = 2,
                West = 5
            },

            new DiceOrientation // 2
            {
                Top = 2,
                Bottom = 5,
                North = 1,
                South = 6,
                East = 3,
                West = 4
            },

            new DiceOrientation // 3
            {
                Top = 3,
                Bottom = 4,
                North = 6,
                South = 1,
                East = 2,
                West = 5
            },

            new DiceOrientation // 4
            {
                Top = 4,
                Bottom = 3,
                North = 1,
                South = 6,
                East = 2,
                West = 5
            },
            new DiceOrientation // 5
            {
                Top = 5,
                Bottom = 2,
                North = 1,
                South = 6,
                East = 4,
                West = 3
            },

            new DiceOrientation // 5
            {
                Top = 6,
                Bottom = 1,
                North = 4,
                South = 3,
                East = 2,
                West = 5
            },
    };
}

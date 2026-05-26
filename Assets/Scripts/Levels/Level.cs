using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;


public class Level :ScriptableObject
{
    //public int stars=0;
    //public bool isCompleted=false;
    //public int bestMoves=0;
    public int designedMoves=0;

    public int levelID;
    public List<int> bestSteps = new List<int>();
    public virtual void DrawBoard()
    {
        BoardManager boardManager = BoardManager.instance;
        boardManager.DeleteBoard();
        

    }
   

}






using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TitleGenerator : MonoBehaviour
{
    [SerializeField] GameObject cameraTitle;
    [SerializeField] GameObject dice;

    [SerializeField] float spacing = 1f;
    [SerializeField] public Vector3 offsetPosition;
    List<Transform> diceList = new List<Transform>();

    [SerializeField] float animationDuration = 1f;
    
    int[][,] abc = new int[][,]
{
    new int[,] { {0,1,0},{1,0,1},{1,1,1},{1,0,1},{1,0,1} }, // A
    new int[,] { {1,1,0},{1,0,1},{1,1,0},{1,0,1},{1,1,0} }, // B
    new int[,] { {0,1,1},{1,0,0},{1,0,0},{1,0,0},{0,1,1} }, // C
    new int[,] { {1,1,0},{1,0,1},{1,0,1},{1,0,1},{1,1,0} }, // D
    new int[,] { {1,1,1},{1,0,0},{1,1,1},{1,0,0},{1,1,1} }, // E
    new int[,] { {1,1,1},{1,0,0},{1,1,1},{1,0,0},{1,0,0} }, // F
    new int[,] { {1,1,1},{1,0,0},{1,0,1},{1,0,1},{1,1,1} }, // G
    new int[,] { {1,0,1},{1,0,1},{1,1,1},{1,0,1},{1,0,1} }, // H
    new int[,] { {1,1,1},{0,1,0},{0,1,0},{0,1,0},{1,1,1} }, // I
    new int[,] { {1,1,1},{0,1,0},{0,1,0},{0,1,0},{1,0,0} }, // J
    new int[,] { {1,0,0},{1,0,1},{1,1,0},{1,1,0},{1,0,1} }, // K
    new int[,] { {1,0,0},{1,0,0},{1,0,0},{1,0,0},{1,1,1} }, // L
    new int[,] { {1,0,1},{1,1,1},{1,0,1},{1,0,1},{1,0,1} }, // M
    new int[,] { {1,1,1},{1,0,1},{1,0,1},{1,0,1},{1,0,1} }, // N
    new int[,] { {1,1,1},{1,0,1},{1,0,1},{1,0,1},{1,1,1} }, // O
    new int[,] { {1,1,0},{1,0,1},{1,1,0},{1,0,0},{1,0,0} }, // P
    new int[,] { {1,1,1},{1,0,1},{1,0,1},{1,1,1},{0,1,0} }, // Q
    new int[,] { {1,1,0},{1,0,1},{1,1,0},{1,0,1},{1,0,1} }, // R
    new int[,] { {0,1,1},{1,0,0},{0,1,0},{0,0,1},{1,1,0} }, // S
    new int[,] { {1,1,1},{0,1,0},{0,1,0},{0,1,0},{0,1,0} }, // T
    new int[,] { {1,0,1},{1,0,1},{1,0,1},{1,0,1},{1,1,1} }, // U
    new int[,] { {1,0,1},{1,0,1},{1,0,1},{1,0,1},{0,1,0} }, // V
    new int[,] { {1,0,1},{1,0,1},{1,0,1},{1,1,1},{1,0,1} }, // W
    new int[,] { {1,0,1},{1,0,1},{0,1,0},{1,0,1},{1,0,1} }, // X
    new int[,] { {1,0,1},{1,0,1},{0,1,0},{0,1,0},{0,1,0} }, // Y
    new int[,] { {1,1,1},{1,0,0},{0,1,0},{0,0,1},{0,1,0} }, // Z (*)
};

    public static readonly Dictionary<char, bool[,]> Letters = new()
    {
        ['A'] = new bool[,] {
            { false, true,  false },
            { true,  false, true  },
            { true,  true,  true  },
            { true,  false, true  },
        },
        ['B'] = new bool[,] {
            { true,  true,  false },
            { true,  true,  true  },
            { true,  false, true  },
            { true,  true,  false },
        },
        ['C'] = new bool[,] {
            { false, true,  true  },
            { true,  false, false },
            { true,  false, false },
            { false, true,  true  },
        },
        ['D'] = new bool[,] {
            { true,  true,  false },
            { true,  false, true  },
            { true,  false, true  },
            { true,  true,  false },
        },
        ['E'] = new bool[,] {
            { true,  true,  true  },
            { true,  true,  false },
            { true,  false, false },
            { true,  true,  true  },
        },
        ['F'] = new bool[,] {
            { true,  true,  true  },
            { true,  true,  false },
            { true,  false, false },
            { true,  false, false },
        },
        ['G'] = new bool[,] {
            { false, true,  true  },
            { true,  false, false },
            { true,  false, true  },
            { false, true,  true  },
        },
        ['H'] = new bool[,] {
            { true,  false, true  },
            { true,  true,  true  },
            { true,  false, true  },
            { true,  false, true  },
        },
        ['I'] = new bool[,] {
            { true,  true,  true  },
            { false, true,  false },
            { false, true,  false },
            { true,  true,  true  },
        },
        ['J'] = new bool[,] {
            { false, false, true  },
            { false, false, true  },
            { true,  false, true  },
            { false, true,  false },
        },
        ['K'] = new bool[,] {
            { true,  false, true  },
            { true,  true,  false },
            { true,  true,  false },
            { true,  false, true  },
        },
        ['L'] = new bool[,] {
            { true,  false, false },
            { true,  false, false },
            { true,  false, false },
            { true,  true,  true  },
        },
        ['M'] = new bool[,] {
            { true,  false, true  },
            { true,  true,  true  },
            { true,  false, true  },
            { true,  false, true  },
        },
        ['N'] = new bool[,] {
            { true,  false, true  },
            { true,  true,  true  },
            { true,  true,  true  },
            { true,  false, true  },
        },
        ['O'] = new bool[,] {
            { false, true,  false },
            { true,  false, true  },
            { true,  false, true  },
            { false, true,  false },
        },
        ['P'] = new bool[,] {
            { true,  true,  false },
            { true,  false, true  },
            { true,  true,  false },
            { true,  false, false },
        },
        ['Q'] = new bool[,] {
            { false, true,  false },
            { true,  false, true  },
            { true,  false, true  },
            { false, true,  true  },
        },
        ['R'] = new bool[,] {
            { true,  true,  false },
            { true,  false, true  },
            { true,  true,  false },
            { true,  false, true  },
        },
        ['S'] = new bool[,] {
            { false, true,  true  },
            { true,  false, false },
            { false, false, true  },
            { true,  true,  false },
        },
        ['T'] = new bool[,] {
            { true,  true,  true  },
            { false, true,  false },
            { false, true,  false },
            { false, true,  false },
        },
        ['U'] = new bool[,] {
            { true,  false, true  },
            { true,  false, true  },
            { true,  false, true  },
            { false, true,  false },
        },
        ['V'] = new bool[,] {
            { true,  false, true  },
            { true,  false, true  },
            { true,  false, true  },
            { false, true,  false },
        },
        ['W'] = new bool[,] {
            { true,  false, true  },
            { true,  false, true  },
            { true,  true,  true  },
            { true,  false, true  },
        },
        ['X'] = new bool[,] {
            { true,  false, true  },
            { false, true,  false },
            { false, true,  false },
            { true,  false, true  },
        },
        ['Y'] = new bool[,] {
            { true,  false, true  },
            { false, true,  false },
            { false, true,  false },
            { false, true,  false },
        },
        ['Z'] = new bool[,] {
            { true,  true,  true  },
            { false, true,  false },
            { true,  false, false },
            { true,  true,  true  },
        },
    };


    [SerializeField] string title = "DICE JUMPER";

    private void Awake()
    {
        cameraTitle.SetActive(false);
    }

    private void Start()
    {

        PrintText();
            StartCoroutine(FallTitleAnimation());
        cameraTitle.SetActive(true);
    }


    public void PrintText()
    {
        Vector3 pos = new Vector3();
        for (int i = 0; i < title.Length; i++)
        {
            char c = title[i];
            if (c >= 'A' && c <= 'Z')
            {
                int index = c - 'A';
                PrintLetter(pos+ offsetPosition, abc[index]);
                
            }

            pos.x += spacing * 4;
        }
    }

    private void PrintLetter(Vector3 pos, int[,] letter)
    {
        for(int i = 0; i < letter.GetLength(0); i++)
        {
            for (int j = 0; j < letter.GetLength(1); j++)
            {
                if (letter[i, j] == 1)
                {
                    Vector3 spawnPos = pos + new Vector3(j * spacing, -i * spacing, 0);
                    GameObject d=Instantiate(dice,spawnPos,Quaternion.identity,transform);
                    d.SetActive(true);
                    d.transform.forward = DiceManager.GetAxisOfDots(Random.Range(1, 7));
                    diceList.Add(d.transform);
                }
            }
        }
    }
    IEnumerator FallTitleAnimation()
    {
        int i=diceList.Count-1;
        float[] diceCD=new float[diceList.Count];
        float iTime = 1f / animationDuration;
        Vector3[] targetPosition= new Vector3[diceList.Count];
        for(int j=0; j<diceList.Count; j++)
        {
            targetPosition[j] = diceList[j].position-offsetPosition;
            diceCD[j] = 0f;
        }
        int k = diceList.Count - 1;
        while (diceCD[0]<1f)
        {
            for(int j= k; j>=i; j--)
            {
                diceCD[j] += iTime * Time.deltaTime;
                diceList[j].position = Vector3.Lerp(diceList[j].position, targetPosition[j], diceCD[j]);
            }
            if(diceCD[k]>=1f)
            {
                diceList[k].position=targetPosition[k];
                k--;
            }
            if(i>0)
            i--; 
            yield return null;
        }
    }
}


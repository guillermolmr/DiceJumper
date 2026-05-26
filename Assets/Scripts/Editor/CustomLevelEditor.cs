using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.MessageBox;

[CustomEditor(typeof(CustomLevel))]
public class CustomLevelEditor : Editor
{
    //  Paint mode 
    private enum PaintMode
    {
        Floor, Delete, None,
        White, Black, Yellow, Teleport, PressurePlate, Goal,Start, TargetTeleport,RotateLeft,RotateRight,
        Glass
    }

    private PaintMode currentMode = PaintMode.Floor;

    //  Selection 
    private CustomLevel.SpecialTile selectedTile = null;
    private bool isDragging = false;

    //  Foldouts 
    private bool showBasicProps = true;

    //  Scroll 
    private Vector2 gridScroll;

    //  Colors 
    private static readonly Color ColFloor = new Color(0.85f, 0.85f, 0.85f);
    private static readonly Color ColEmpty = new Color(0.20f, 0.20f, 0.20f);
    private static readonly Color ColStart = new Color(0.30f, 0.80f, 0.30f);
    private static readonly Color ColGoal = new Color(0.90f, 0.40f, 0.20f);
    private static readonly Color ColWhite = Color.white;
    private static readonly Color ColBlack = new Color(0.10f, 0.10f, 0.10f);
    private static readonly Color ColYellow = new Color(1f, 0.85f, 0.10f);
    private static readonly Color ColTeleport = new Color(0.4f, 0.20f, 0.90f);
    private static readonly Color ColTargetTeleport = new Color(0.2f, 0.45f, 0.90f);
    private static readonly Color ColRotateRight = new Color(0.45f, 0.45f, 0.90f);
    private static readonly Color ColRotateLeft = new Color(0.90f, 0.45f, 0.45f);
    private static readonly Color ColPressure = new Color(0.20f, 0.65f, 0.90f);
    private static readonly Color ColGoalTile = new Color(0.90f, 0.40f, 0.20f);
    private static readonly Color ColSelected = new Color(1f, 0.85f, 0f, 0.55f);
    private static readonly Color ColClear = new Color(0.6f, 0.6f, 0.6f);


    private static readonly Color ColGlass = Color.orange;
    

    // Toolbar button sizes
    private const float CELL = 28f;   // grid cell
    private const float BTN_W = 90f;
    private const float BTN_H = 26f;

    // 

    
    bool dragPaintValue=false;


    //Textures
    Texture[] tileTextures=null;
    Texture[] diceDots = null;
    Texture tileRightTex=null;
    Texture tileLeftTex=null;
    public override void OnInspectorGUI()
    {

        if (tileTextures == null)
        {
            tileTextures= new Texture[17];
            for (int i=0; i< tileTextures.Length; i++)
            {
                tileTextures[i] = Resources.Load($"TileEditor/tile{i.ToString()}") as Texture;
            }
            diceDots = new Texture[6];
            for (int i = 0; i < 6; i++) {
                diceDots[i] = tileTextures[8 + i];

            }

            tileRightTex = tileTextures[15];
            tileLeftTex = tileTextures[16];

        }
        serializedObject.Update();
        var lvl = (CustomLevel)target;

        //  Basic properties 
        showBasicProps = EditorGUILayout.Foldout(showBasicProps, "Level Properties", true);
        if (showBasicProps)
        {
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();

            int newW = EditorGUILayout.IntField("Width", lvl.width);
            int newH = EditorGUILayout.IntField("Height", lvl.height);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(lvl, "Resize Grid");
                ResizeGrid(lvl, newW, newH);
                EditorUtility.SetDirty(lvl);
            }

            lvl.startPosition = EditorGUILayout.Vector2IntField("Start Position", lvl.startPosition);
            lvl.goalPosition = EditorGUILayout.Vector2IntField("Goal Position", lvl.goalPosition);
            lvl.startingDots = EditorGUILayout.IntField("Starting Dots", lvl.startingDots);
            lvl.goalDots = EditorGUILayout.IntField("Goal Dots", lvl.goalDots);
            lvl.designedMoves = EditorGUILayout.IntField("Designed Moves", lvl.designedMoves);
            string bestSteps = "";
            for(int i = 0; i < lvl.bestSteps.Count; i++)
            {
                bestSteps += lvl.bestSteps[i].ToString() + ", ";
            }
            EditorGUILayout.LabelField("Designed moves: " + bestSteps);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(6);

        //  Paint mode toolbar 
        DrawToolbar(lvl);

        EditorGUILayout.Space(4);

        //  Grid 
        if (lvl.width > 0 && lvl.height > 0)
        {
            EnsureGrid(lvl);
            DrawGrid(lvl);
            DrawClearButton(lvl);
            DrawExpandButtons(lvl);

        }
        else
        {
            EditorGUILayout.HelpBox("Set Width and Height to start editing.", MessageType.Info);
        }

        //  Selected tile panel 
        if (selectedTile != null)
        {
            EditorGUILayout.Space(8);
            DrawTilePanel(lvl);
        }



        serializedObject.ApplyModifiedProperties();
    }
    private void DrawExpandButtons(CustomLevel lvl)
    {
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Expand Grid", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = new Color(0.4f, 0.75f, 1f);
        if (GUILayout.Button("Expand Left", GUILayout.Width(BTN_W * 1.4f), GUILayout.Height(BTN_H)))
            ExpandGrid(lvl, expandLeft: true, expandBottom: false);

        GUI.backgroundColor = new Color(0.4f, 1f, 0.7f);
        if (GUILayout.Button("Expand Bottom", GUILayout.Width(BTN_W * 1.4f), GUILayout.Height(BTN_H)))
            ExpandGrid(lvl, expandLeft: false, expandBottom: true);

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
    }
    // 
    // TOOLBAR
    // 
    private void DrawToolbar(CustomLevel lvl)
    {
        EditorGUILayout.LabelField("Paint Mode", EditorStyles.boldLabel);

        // Row 1: Floor + special tiles
        EditorGUILayout.BeginHorizontal();
        DrawModeButton("Floor", PaintMode.Floor, ColFloor);
        //DrawModeButton("White", PaintMode.White, ColWhite);
        //DrawModeButton("Black", PaintMode.Black, ColBlack);
        DrawModeButton("Pressure", PaintMode.PressurePlate, ColPressure);
        DrawModeButton("Yellow", PaintMode.Yellow, ColYellow);
        DrawModeButton("Teleport", PaintMode.Teleport, ColTeleport);
        DrawModeButton("TargetTeleport", PaintMode.TargetTeleport, ColTargetTeleport);
        DrawModeButton("RotateLeft", PaintMode.RotateLeft, ColRotateLeft);
        DrawModeButton("RotateRight", PaintMode.RotateRight, ColRotateRight);
        DrawModeButton("Glass", PaintMode.Glass, ColGlass);

        EditorGUILayout.EndHorizontal();

        // Row 2: Delete
        EditorGUILayout.BeginHorizontal();
        DrawModeButton(" Delete", PaintMode.Delete, new Color(0.85f, 0.25f, 0.25f));
        DrawModeButton("Start", PaintMode.Start, ColStart);
        DrawModeButton("Goal", PaintMode.Goal, ColGoal);
        EditorGUILayout.EndHorizontal();

        // Active mode label
        EditorGUILayout.LabelField($"Active: {currentMode}", EditorStyles.miniLabel);
    }


    private void DrawClearButton(CustomLevel lvl)
    {
       

        GUI.backgroundColor = ColClear;
        GUI.contentColor = (ColClear.grayscale < 0.5f) ? Color.white : Color.black;

        GUIStyle style = new GUIStyle(GUI.skin.button);

        if (GUILayout.Button("Clear", style, GUILayout.Width(BTN_W), GUILayout.Height(BTN_H)))
        {
            lvl.specialTiles.Clear();
            lvl.floorTiles = new bool[lvl.floorTiles.Length];
            lvl.designedMoves = 1000;
            lvl.bestSteps.Clear();
            EditorUtility.SetDirty(lvl);
        }

    }
    private void DrawModeButton(string label, PaintMode mode, Color col)
    {

        bool active = currentMode == mode;

        var prevBg = GUI.backgroundColor;
        var prevCol = GUI.contentColor;

        GUI.backgroundColor = active ? col : col * 0.6f;
        GUI.contentColor = (col.grayscale < 0.5f) ? Color.white : Color.black;

        GUIStyle style = new GUIStyle(GUI.skin.button);
        if (active)
        {
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = GUI.contentColor;
        }

        if (GUILayout.Button(label, style, GUILayout.Width(BTN_W), GUILayout.Height(BTN_H)))
        {
            currentMode = mode;
        }
            

        GUI.backgroundColor = prevBg;
        GUI.contentColor = prevCol;
    }

    // 
    // GRID
    // 
    private void DrawGrid(CustomLevel lvl)
    {
        float gridW = lvl.width * CELL;
        float gridH = lvl.height * CELL;

        // Scrollable area
        gridScroll = EditorGUILayout.BeginScrollView(gridScroll,
            GUILayout.Height(Mathf.Min(gridH + 4, 500)));

        // Reserve space for the grid
        Rect gridRect = GUILayoutUtility.GetRect(gridW, gridH);

        Event e = Event.current;

        

        // Draw cells
        for (int y = 0; y < lvl.height; y++)
        {
            for (int x = 0; x < lvl.width; x++)
            {
                Rect cell = new Rect(
                    gridRect.x + x * CELL,
                    gridRect.y + (lvl.height - 1 - y) * CELL, // flip Y so (0,0) is bottom-left
                    CELL - 1, CELL - 1);

                DrawCell(lvl, x, y, cell);

                // Input handling
                if (e.type == EventType.MouseDown && cell.Contains(e.mousePosition))
                {
                    isDragging = false;
                    HandleCellClick(lvl, x, y);
                    isDragging = true;
                    e.Use();
                    Repaint();
                }
                else if (e.type == EventType.MouseDrag && isDragging && cell.Contains(e.mousePosition))
                {
                    // Only drag for floor/delete modes
                    if (currentMode == PaintMode.Floor || currentMode == PaintMode.Delete || currentMode == PaintMode.Glass)
                    {
                        HandleCellClick(lvl, x, y);
                        e.Use();
                        Repaint();
                    }
                }
            }
        }

        if (e.type == EventType.MouseUp)
            isDragging = false;

        EditorGUILayout.EndScrollView();
    }

    
    private void DrawCell(CustomLevel lvl, int x, int y, Rect cell)
    {
        Vector2Int pos = new Vector2Int(x, y);
        bool isFloor = lvl.floorTiles != null && lvl.IsFloor(x, y);

        // Base color
        Color bg = isFloor ? ColFloor : ColEmpty;

        // Override with special tile color
        CustomLevel.SpecialTile special = GetSpecialAt(lvl, pos);
        Texture texture = null;

        
        if (special != null)
        {
            
            bg = GetTileColor(special.type);
            texture= GetTileTexture(special.type);
        }

        // Start / goal markers
        if (diceDots!=null && diceDots.Length == 6)
        {
            if (pos == lvl.startPosition )
            {
                if(lvl.startingDots > 0 && lvl.startingDots <= 6)
                    texture = diceDots[lvl.startingDots - 1];
                bg = ColStart;
            }
            if (pos == lvl.goalPosition)
            {
                if(lvl.goalDots > 0 && lvl.goalDots <= 6)
                    texture = diceDots[lvl.goalDots - 1];
                bg = ColGoal;
            }
        }
        
        

        // Draw background
        EditorGUI.DrawRect(cell, bg);

        // Selected overlay
        if (special != null && special == selectedTile)
        {
            
                EditorGUI.DrawRect(cell, ColSelected);

            
            
        }
        if (texture != null)
        {
            GUI.DrawTexture(cell, texture);
        }
        // Coordinates hint
        var labelStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            fontSize = 7,
            alignment = TextAnchor.LowerCenter,
            normal = { textColor = (bg.grayscale < 0.5f) ? Color.white * 0.5f : Color.black * 0.4f }
        };
        GUI.Label(cell, $"{x},{y}", labelStyle);

        // Special tile type initials
        if (special != null)
        {
            var typeStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 9,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = (bg.grayscale < 0.45f) ? Color.white : Color.black }
            };
            GUI.Label(cell, TileInitial(special.type), typeStyle);
        }
    }

    private void HandleCellClick(CustomLevel lvl, int x, int y)
    {
        Undo.RecordObject(lvl, "Edit Level Tile");
        Vector2Int pos = new Vector2Int(x, y);

        switch (currentMode)
        {
            case PaintMode.Floor:
                {
                    // Toggle floor; select special if present
                    CustomLevel.SpecialTile s = GetSpecialAt(lvl, pos);
                    if (s != null)
                        selectedTile = s;
                    else
                    {
                        if(!isDragging)
                            dragPaintValue = !lvl.IsFloor(x,y);
                        lvl.SetFloor(x, y,dragPaintValue);
                    }
                    break;
                }
            case PaintMode.Glass:
                {
                    // Toggle floor; select special if present
                    CustomLevel.SpecialTile s = GetSpecialAt(lvl, pos);
                    if (s ==null)
                    {
                        var newTile = new CustomLevel.SpecialTile
                        {
                            type = TileType.Glass,
                            position = pos,
                            value = 0,
                            targets = new List<Vector2Int>()
                        };
                        lvl.specialTiles.Add(newTile);

                    }
                    break;
                }
            case PaintMode.Delete:
                {
                    CustomLevel.SpecialTile s = GetSpecialAt(lvl, pos);
                    if (s != null)
                    {
                        if (selectedTile == s) selectedTile = null;
                        if(s.type == TileType.PressurePlate)
                        {
                            foreach (var target in s.targets)
                            {
                                lvl.specialTiles.RemoveAll(t => t.position == target);
                            }
                        }
                        else if (s.type == TileType.Yellow && s.targets != null && s.targets.Count > 0)
                        {
                            var presurePlate = GetSpecialAt(lvl, s.targets[0]);
                            presurePlate.targets.Remove(s.position);
                        }else if (s.type == TileType.Teleport && s.targets != null && s.targets.Count > 0)
                        {
                            Vector2Int target = s.targets[0];
                            lvl.specialTiles.RemoveAll(t => t.position == target );
                        }else if (s.type == TileType.TargetTeleport && s.targets != null && s.targets.Count > 0)
                        {
                            
                            var presurePlate = GetSpecialAt(lvl, s.targets[0]);
                            presurePlate.targets.Remove(s.position);
                        }
                        lvl.specialTiles.Remove(s);
                    }
                    else
                    {
                        lvl.SetFloor(x, y, false);
                    }
                    break;
                }
            case PaintMode.Start:
                {
                    lvl.startPosition = pos;
                }
                break;
            case PaintMode.Goal:
                {
                    lvl.goalPosition = pos;
                }
                break;

            case PaintMode.Yellow:
                {
                    
                    if (selectedTile == null || selectedTile.type != TileType.PressurePlate)
                    {
                        break;
                    }

                    if (selectedTile.targets == null)
                        selectedTile.targets = new List<Vector2Int>();

                    
                    if (selectedTile.targets.Contains(pos))
                    {
                        selectedTile.targets.Remove(pos);
                    }
                    else
                    {
                        selectedTile.targets.Add(pos);
                    }

                    TileType type = ModeToTileType(currentMode);

                    CustomLevel.SpecialTile existing = GetSpecialAt(lvl, pos);
                    if (existing != null)
                    {
                        Debug.Log("There is a special tile already");
                        break;
                    }

                    // Ensure floor is on under it
                    lvl.SetFloor(x,y,false) ;

                    var newTile = new CustomLevel.SpecialTile
                    {
                        type = type,
                        position = pos,
                        value = 0,
                        targets = new List<Vector2Int>()
                    };
                    newTile.targets.Add(selectedTile.position);
                    lvl.specialTiles.Add(newTile);






                    break;
                }
            case PaintMode.TargetTeleport:
                {

                    if (selectedTile == null || selectedTile.type != TileType.Teleport)
                    {
                        break;
                    }

                    if (selectedTile.targets == null)
                        selectedTile.targets = new List<Vector2Int>();


                    if (selectedTile.targets.Count>0)
                    {
                        selectedTile.targets[0] = pos;
                    }
                    else
                    {
                        selectedTile.targets.Add(pos);
                    }

                    TileType type = ModeToTileType(currentMode);

                    CustomLevel.SpecialTile existing = GetSpecialAt(lvl, pos);
                    if (existing != null)
                    {
                        Debug.Log("There is a special tile already");
                        break;
                    }

                    // Ensure floor is on under it
                    lvl.SetFloor(x,y, true);

                    var newTile = new CustomLevel.SpecialTile
                    {
                        type = type,
                        position = pos,
                        value = 0,
                        targets = new List<Vector2Int>()
                    };
                    newTile.targets.Add(selectedTile.position);
                    lvl.specialTiles.Add(newTile);






                    break;
                }
            default:
                {
                    // Place a special tile
                    TileType type = ModeToTileType(currentMode);

                    
                    CustomLevel.SpecialTile existing = GetSpecialAt(lvl, pos);
                    if (existing != null)
                    {
                        Debug.Log("There is a special tile already");
                        break;
                    }

                    // Ensure floor is on under it
                    lvl.SetFloor(x,y,true);

                    var newTile = new CustomLevel.SpecialTile
                    {
                        type = type,
                        position = pos,
                        value = 0,
                        targets = new List<Vector2Int>()
                    };
                    lvl.specialTiles.Add(newTile);
                    selectedTile = newTile;

                    // Return to floor mode after placing
                    currentMode = PaintMode.Floor;
                    break;
                }
        }

        EditorUtility.SetDirty(lvl);
    }

    // 
    // TILE DATA PANEL
    // 
    private void DrawTilePanel(CustomLevel lvl)
    {
        // Safety: make sure selectedTile still belongs to the list
        if (!lvl.specialTiles.Contains(selectedTile))
        {
            selectedTile = null;
            return;
        }

        Color panelCol = GetTileColor(selectedTile.type) * 0.35f;
        panelCol.a = 1f;

        var panelStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(10, 10, 8, 8)
        };

        var prevBg = GUI.backgroundColor;
        GUI.backgroundColor = panelCol;
        EditorGUILayout.BeginVertical(panelStyle);
        GUI.backgroundColor = prevBg;

        EditorGUILayout.LabelField(
            $"Selected Tile  {selectedTile.type} @ ({selectedTile.position.x}, {selectedTile.position.y})",
            EditorStyles.boldLabel);

        EditorGUILayout.Space(4);

        // Position (read-only hint)
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.Vector2IntField("Position", selectedTile.position);
        EditorGUI.EndDisabledGroup();

        // Type
        TileType newType = (TileType)EditorGUILayout.EnumPopup("Type", selectedTile.type);
        if (newType != selectedTile.type)
        {
            Undo.RecordObject(lvl, "Change Tile Type");
            selectedTile.type = newType;
            EditorUtility.SetDirty(lvl);
        }

        // Value (relevant for Yellow etc.)
        int newVal = EditorGUILayout.IntField("Value", selectedTile.value);
        if (newVal != selectedTile.value)
        {
            Undo.RecordObject(lvl, "Change Tile Value");
            selectedTile.value = newVal;
            EditorUtility.SetDirty(lvl);
        }

        // Targets list
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Targets", EditorStyles.boldLabel);

        if (selectedTile.targets == null)
            selectedTile.targets = new List<Vector2Int>();

        for (int i = 0; i < selectedTile.targets.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            Vector2Int newTarget = EditorGUILayout.Vector2IntField($"  [{i}]", selectedTile.targets[i]);
            if (newTarget != selectedTile.targets[i])
            {
                Undo.RecordObject(lvl, "Edit Target");
                selectedTile.targets[i] = newTarget;
                EditorUtility.SetDirty(lvl);
            }
            if (GUILayout.Button("", GUILayout.Width(22)))
            {
                Undo.RecordObject(lvl, "Remove Target");
                selectedTile.targets.RemoveAt(i);
                EditorUtility.SetDirty(lvl);
                break;
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("+ Add Target", GUILayout.Width(110)))
        {
            Undo.RecordObject(lvl, "Add Target");
            selectedTile.targets.Add(Vector2Int.zero);
            EditorUtility.SetDirty(lvl);
        }

        EditorGUILayout.Space(4);

        // Delete this tile button
        var prevBg2 = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.75f, 0.15f, 0.15f);
        if (GUILayout.Button("Delete This Tile"))
        {
            Undo.RecordObject(lvl, "Delete Special Tile");
            lvl.specialTiles.Remove(selectedTile);
            selectedTile = null;
            EditorUtility.SetDirty(lvl);
        }
        GUI.backgroundColor = prevBg2;

        EditorGUILayout.EndVertical();
    }

    // 
    // HELPERS
    // 
    private void EnsureGrid(CustomLevel lvl)
    {
        if (lvl.floorTiles == null ||
            lvl.floorTiles.GetLength(0) != lvl.width ||
            lvl.floorTiles.GetLength(1) != lvl.height)
        {
            ResizeGrid(lvl, lvl.width, lvl.height);
        }

        if (lvl.specialTiles == null)
            lvl.specialTiles = new List<CustomLevel.SpecialTile>();
    }

    private void ResizeGrid(CustomLevel lvl, int newW, int newH)
    {
        newW = Mathf.Max(1, newW);
        newH = Mathf.Max(1, newH);
        int newSize = newW * newH;
        bool[] old = lvl.floorTiles;
        
        bool[] next = new bool[newSize];

        if (old != null)
        {
            List<Vector2Int> oldFloors = new List<Vector2Int>();
            for(int i=0;i<old.Length; i++)
            {
                if (old[i])
                {
                    int x = i % lvl.width;
                    int y = i / lvl.width;
                    oldFloors.Add(new Vector2Int(x, y));
                }
            }
            foreach( var pos in oldFloors)
            {
                if(pos.x < newW && pos.y < newH)
                {
                    int newIndex = pos.y * newW + pos.x;
                    next[newIndex] = true;
                }
            }

        }

        lvl.width = newW;
        lvl.height = newH;
        lvl.floorTiles = next;

        // Remove out-of-bounds special tiles
        lvl.specialTiles?.RemoveAll(t =>
            t.position.x < 0 || t.position.x >= newW ||
            t.position.y < 0 || t.position.y >= newH);
    }

    private CustomLevel.SpecialTile GetSpecialAt(CustomLevel lvl, Vector2Int pos)
    {
        if (lvl.specialTiles == null) return null;
        return lvl.specialTiles.Find(t => t.position == pos);
    }

    private TileType ModeToTileType(PaintMode mode) => mode switch
    {
        PaintMode.White => TileType.White,
        PaintMode.Black => TileType.Black,
        PaintMode.Yellow => TileType.Yellow,
        PaintMode.Teleport => TileType.Teleport,
        PaintMode.PressurePlate => TileType.PressurePlate,
        PaintMode.Goal => TileType.Goal,
        PaintMode.RotateLeft => TileType.RotateLeft,
        PaintMode.RotateRight => TileType.RotateRight,
        PaintMode.TargetTeleport=> TileType.TargetTeleport,
        PaintMode.Glass => TileType.Glass,
        _ => TileType.None,
    };

    private Color GetTileColor(TileType type) => type switch
    {
        TileType.White => ColWhite,
        TileType.Black => ColBlack,
        TileType.Yellow => ColYellow,
        TileType.Teleport => ColTeleport,
        TileType.TargetTeleport => ColTeleport * 0.7f,
        TileType.PressurePlate => ColPressure,
        TileType.RotateLeft => ColRotateLeft,
        TileType.RotateRight => ColRotateRight,
        TileType.Goal => ColGoalTile,
        TileType.Glass =>ColGlass,
        _ => ColFloor,
    };


    private Texture GetTileTexture(TileType type) => type switch
    {
        TileType.RotateLeft => tileLeftTex,
        TileType.RotateRight => tileRightTex,
        _ => null,
    };
    private string TileInitial(TileType type) => type switch
    {
        TileType.White => "W",
        TileType.Black => "B",
        TileType.Yellow => "Y",
        TileType.Teleport => "T",
        TileType.TargetTeleport => "TT",
        TileType.PressurePlate => "PP",
        //TileType.RotateLeft=>"RL",
        //TileType.RotateRight=>"RR",
        TileType.Goal => "G",
        _ => "",
    };

    private void ExpandGrid(CustomLevel lvl, bool expandLeft, bool expandBottom)
    {
        Undo.RecordObject(lvl, expandLeft ? "Expand Left" : "Expand Bottom");

        int newW = lvl.width + (expandLeft ? 1 : 0);
        int newH = lvl.height + (expandBottom ? 1 : 0);
        int offsetX = expandLeft ? 1 : 0;
        int offsetY = expandBottom ? 1 : 0;

        // Rebuild floorTiles
        bool[] next = new bool[newW * newH];
        if (lvl.floorTiles != null)
        {
            for (int i = 0; i < lvl.floorTiles.Length; i++)
            {
                if (!lvl.floorTiles[i]) continue;
                int x = i % lvl.width;
                int y = i / lvl.width;
                int nx = x + offsetX;
                int ny = y + offsetY;
                next[ny * newW + nx] = true;
            }
        }

        // Shift special tiles
        if (lvl.specialTiles != null)
        {
            foreach (var t in lvl.specialTiles)
            {
                t.position += new Vector2Int(offsetX, offsetY);
                if (t.targets != null)
                    for (int i = 0; i < t.targets.Count; i++)
                        t.targets[i] += new Vector2Int(offsetX, offsetY);
            }
        }

        // Shift start/goal
        lvl.startPosition += new Vector2Int(offsetX, offsetY);
        lvl.goalPosition += new Vector2Int(offsetX, offsetY);

        lvl.width = newW;
        lvl.height = newH;
        lvl.floorTiles = next;

        EditorUtility.SetDirty(lvl);
    }
}
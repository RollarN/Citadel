#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

public class TileEditorTool : EditorWindow
{
    #region Members
    private TileType[,,] m_TileGridTypes = new TileType[1,1,1];

    private TileEditorData m_ToolData = null;

    [SerializeField] private int m_TileSize = 1;
    [SerializeField] private Vector3 m_Preview = Vector3.zero;

    private int gridWidth = 1, gridHeight = 1, gridDepth = 1;
    private GridMinMax xMinMax = new GridMinMax(0f, 0f), yMinMax = new GridMinMax(0f, 0f), zMinMax = new GridMinMax(0f, 0f);
    private bool m_ToolActive = false;

    private Vector3 m_GridPosition = Vector3.zero;
    private float m_XGridLock = 0f;
    private float m_YGridLock = 0f;
    private float m_ZGridLock = 0f;
    private bool m_DisplayGrid = true;
    private Axis m_CurrentGridToDisplay = Axis.x;
    private float m_DisplayExtent = 5f;
    private float m_EditRange = 100f;
    private Vector3 m_AreaStart = Vector3.zero;
    private bool m_DragArea = false;
    private bool m_SpawnAsPrefab = false;

    private ObjectPool[] m_PaintTypePools = new ObjectPool[(int)TileType.TILETYPE_MAX];
    private GameObject m_PoolParent = null;
    private const string k_PoolParentName = "PaintTypePoolParent";

    private static readonly Color s_LightBlue = new Color(0.26f, 0.82f, 1f);
    private static readonly Color s_SoftGreen = new Color(0.51f, 1f, 0.53f);
    private static readonly Color s_SoftRed = new Color(1f, 0.27f, 0.35f);
    private static readonly Color s_Beige = new Color(1f, 0.45f, 0f);
    private static readonly Color s_LightBlueTransparent = new Color(0.26f, 0.82f, 1f, 0.1f);
    private static readonly Color s_SoftGreenTransparent = new Color(0.51f, 1f, 0.53f, 0.1f);
    private static readonly Color s_SoftRedTransparent = new Color(1f, 0.27f, 0.35f, 0.1f);
    private static readonly Color s_BeigeTransparent = new Color(1f, 0.45f, 0f, 0.1f);

    private const string k_ActivateTool = "Press E to Deactivate";
    private const string k_PrefabMustBeSelected = "Prefab must be selected for anything to show";
    private const string k_ActivateGrid = "Press Shift+E to activate grid";
    private const string k_SwapGrid = "Press Q to swap between grid-axis'";
    private const string k_Focus = "Press F to move grid where pointer points";
    private const string k_Box = "Hold Shift+Left click and drag mouse to create an area of objects";
    private const string k_ToggleTileEditorText = "Toggle Tile Editor";
    private const string k_ToggleAlgorithmPainterText = "Toggle Algorithm Painter";
    private const string k_ActiveText = "Active";
    private const string k_InactiveText = "Inactive";
    private const string k_SetParentText = "Set Parent";
    private const string k_TypeToDraw = "Type to draw: ";
    private GUIContent m_ToggleTileEditorContent = null;
    private GUIContent m_ToggleAlgorithmPainterContent = null;
    private GUIContent m_SetParentContent = null;
    private bool m_AlgorithmPainterActive = false;
    private TileType m_SelectedPaintType = TileType.Empty;


    private readonly Dictionary<TileType, Color> GetColorByType = new Dictionary<TileType, Color>()
    {
        { TileType.Empty, Color.clear },
        { TileType.Ground, s_SoftGreenTransparent },
        { TileType.Wall, s_BeigeTransparent },
        { TileType.Entrance, s_LightBlueTransparent }
    };

    private GameObject PoolParent
    {
        get
        {
            if(m_PoolParent == null)
            {
                m_PoolParent = GameObject.Find(k_PoolParentName);
                if (m_PoolParent == null)
                {
                    m_PoolParent = new GameObject(k_PoolParentName);
                }
            }
            return m_PoolParent;
        }
        set
        {
            m_PoolParent = value;
        }
    }

    private const string m_ParentName = "TileEditorParent";

    private GameObject m_Parent = null;
    #endregion

    #region Properties
    private GameObject Parent
    {
        get
        {
            if(!m_Parent)
            {
                m_Parent = GameObject.Find(m_ParentName);
                if (!m_Parent)
                {
                    m_Parent = new GameObject(m_ParentName);
                }
            }
            return m_Parent;
        }
        set
        {
            m_Parent = value;
        }
    }
    #endregion

    #region Private Enums
    private enum Axis
    {
        x,
        y,
        z
    }

    private enum ReSizeType
    {
        Add,
        Remove
    }
    #endregion

    [MenuItem("Window/Tools/TileEditor")]
    static void Init()
    {
        TileEditorTool window = GetWindow(typeof(TileEditorTool)) as TileEditorTool;
        window.Show();
    }

    private void OnEnable()
    {

        string[] assets = AssetDatabase.FindAssets("t:TileEditorData");
        if (assets.Length > 0)
        {
            m_ToolData = AssetDatabase.LoadAssetAtPath<TileEditorData>(AssetDatabase.GUIDToAssetPath(assets[0]));
        }



        gridWidth = 1;
        gridHeight = 1;
        gridDepth = 1;
        TileType[,,] m_TileGridTypes = new TileType[gridWidth, gridHeight, gridDepth];
        GameObject[,,] m_TileGrid = new GameObject[gridWidth, gridHeight, gridDepth];


        SceneView.duringSceneGui += OnScene;

        CheckPools();
        //Load from file
    }

    private void OnDisable() 
    {
        SceneView.duringSceneGui -= OnScene;
    }


    private void CheckPools()
    {
        foreach(ObjectPool pool in m_PaintTypePools)
        {
            if (pool == null)
            {
                FetchPools();
                break;
            }
        }
    }

    private ObjectPool GetPool(TileType type)
    {
        return m_PaintTypePools[(int)type];
    }

    private void FetchPools()
    {
        for(int i = 0; i < (int)TileType.TILETYPE_MAX; i++)
        {
            m_PaintTypePools[i] = new ObjectPool(10, GetPaintTypeObject((TileType)i), 10, GetPoolParent((TileType)i).transform);
        }
    }

    private GameObject GetPaintTypeObject(TileType type)
    {
        if (m_ToolData.PaintTypeObjects[(int)type] == null)
        {
            Debug.LogError($"No such object of type {type} exists in {m_ToolData.GetType().ToString()} -> {nameof(m_ToolData.PaintTypeObjects)}.");
            return null;
        }
        else
        {
            return m_ToolData.PaintTypeObjects[(int)type];
        }
    }

    private string GetPaintTypeObjectName(TileType type)
    {
        return Enum.GetName(typeof(TileType), type) + "Pooled";
    }

    private GameObject GetPoolParent(TileType type)
    {
        Transform poolParent = PoolParent.transform.Find(GetPoolParentName(type));
        if (!poolParent)
        {
            GameObject poolParentObject = new GameObject(GetPoolParentName(type));
            poolParentObject.transform.parent = PoolParent.transform;
            poolParent = poolParentObject.transform;
        }
        return poolParent.gameObject;
    }

    private string GetPoolParentName(TileType type)
    {
        return Enum.GetName(typeof(TileType), type) + "Pool";
    }

    private void SetupGUI()
    {
        if(m_ToggleTileEditorContent == null)
        {
            m_ToggleTileEditorContent = new GUIContent(k_ToggleTileEditorText);
        }
        if(m_ToggleAlgorithmPainterContent == null)
        {
            m_ToggleAlgorithmPainterContent = new GUIContent(k_ToggleAlgorithmPainterText);
        }
        if(m_SetParentContent == null)
        {
            m_SetParentContent = new GUIContent(k_SetParentText);
        }
    }

    private void OnGUI()
    {
        SetupGUI();

        GUILayout.Label(k_ActivateTool);
        GUILayout.Label(k_PrefabMustBeSelected);
        GUILayout.Label(k_ActivateGrid);
        GUILayout.Label(k_SwapGrid);
        GUILayout.Label(k_Focus);
        GUILayout.Label(k_Box);

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button(m_ToggleTileEditorContent))
        {
            m_ToolActive = !m_ToolActive;
            m_AlgorithmPainterActive = false;
        }
        GUILayout.Label(m_ToolActive ? k_ActiveText : k_InactiveText);
        GUILayout.EndHorizontal();

        if (m_ToolActive)
        {
            
        }

        GUILayout.Space(5);

        if (m_ToolActive)
        {
            m_SpawnAsPrefab = GUILayout.Toggle(m_SpawnAsPrefab, "Spawn As Prefab");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(m_SetParentContent))
            {
                Parent = Selection.activeGameObject;
            }
            GUILayout.Label(Parent.name);
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button(m_ToggleAlgorithmPainterContent))
        {
            m_AlgorithmPainterActive = !m_AlgorithmPainterActive;
            m_ToolActive = false;
        }
        GUILayout.Label(m_AlgorithmPainterActive ? k_ActiveText : k_InactiveText);
        GUILayout.EndHorizontal();

        if (m_AlgorithmPainterActive)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(k_TypeToDraw);
            Rect enumRect = GUILayoutUtility.GetLastRect();
            enumRect.x = GUILayoutUtility.GetLastRect().x + GUI.skin.label.CalcSize(new GUIContent(k_TypeToDraw)).x;
            enumRect.width = 80;
            m_SelectedPaintType = (TileType)EditorGUI.EnumPopup(enumRect, m_SelectedPaintType);
            GUILayout.EndHorizontal();
        }
    }

    private struct GridMinMax
    {
        public float min, max;

        public GridMinMax(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
        
        public override string ToString()
        {
            return $"GridMinMax(Min: {min}, Max: {max})"; 
        }
    }

    

    private void OnScene(SceneView scene)
    {
        Event e = Event.current;


        //DefineTile(Vector3.up, TileType.Ground);
        if (e.type == EventType.MouseMove)
        {
            scene.Repaint();
        }


        //if (e.type == EventType.KeyDown && e.character == 'e')
        //{
        //    m_ToolActive = !m_ToolActive;
        //}


        if (m_ToolActive || m_AlgorithmPainterActive) {

            if (m_DisplayGrid)
            {
                DrawGrid(5);
            }

            

            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            Vector3 hitPoint = m_Preview;
            Vector3 cameraPosition = scene.camera.transform.position;

            if (e.type == EventType.KeyDown && e.character == 'E')
            {
                m_DisplayGrid = !m_DisplayGrid;
                m_XGridLock = Mathf.Round(hitPoint.x);
                m_YGridLock = Mathf.Round(hitPoint.y);
                m_ZGridLock = Mathf.Round(hitPoint.z);
            }

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                hitPoint = hit.point;
            }
            else
            {
                hit.distance = Mathf.Infinity;
            }

            if (m_DisplayGrid)
            {
                float distanceToGrid = GetDistanceToDisplayedGrid(ray.direction, cameraPosition);

                if (distanceToGrid > m_EditRange && hit.distance > m_EditRange)
                {
                    return;
                }

                if (distanceToGrid < hit.distance)
                {
                    hitPoint = cameraPosition + ray.direction * distanceToGrid;
                    if (m_ToolActive)
                    {
                        m_Preview = (hitPoint + GetDisplayedGridNormal(cameraPosition) * m_TileSize * 0.1f).ToGridPos(GetChunkOffset(), m_TileSize).ToWorldPos(GetChunkOffset(), m_TileSize);
                    }
                    else if (m_AlgorithmPainterActive)
                    {
                        m_Preview = (hitPoint - GetDisplayedGridNormal(cameraPosition) * m_TileSize * 0.1f).ToGridPos(GetChunkOffset(), m_TileSize).ToWorldPos(GetChunkOffset(), m_TileSize);
                    }
                }
                else
                {
                    if (m_ToolActive)
                    {
                        m_Preview = (hit.point + hit.normal * m_TileSize * 0.1f).ToGridPos(GetChunkOffset(), m_TileSize).ToWorldPos(GetChunkOffset(), m_TileSize);
                    }
                    else if (m_AlgorithmPainterActive)
                    {
                        m_Preview = (hit.point - hit.normal * m_TileSize * 0.1f).ToGridPos(GetChunkOffset(), m_TileSize).ToWorldPos(GetChunkOffset(), m_TileSize);
                    }
                }

                if (e.type == EventType.KeyDown && e.character == 'q')
                {
                    switch (m_CurrentGridToDisplay)
                    {
                        case Axis.x:
                            m_CurrentGridToDisplay = Axis.y;
                            m_YGridLock = Mathf.Round(hit.point.y);
                            break;
                        case Axis.y:
                            m_CurrentGridToDisplay = Axis.z;
                            m_ZGridLock = Mathf.Round(hit.point.z);
                            break;
                        case Axis.z:
                            m_CurrentGridToDisplay = Axis.x;
                            m_XGridLock = Mathf.Round(hit.point.x);
                            break;
                    }
                    e.Use();
                }
                UpdateGridPosition(hitPoint);

                if (e.character == 'f')
                {
                    switch (m_CurrentGridToDisplay)
                    {
                        case Axis.x:
                            m_XGridLock = Mathf.Round(hit.point.x);
                            break;
                        case Axis.y:
                            m_YGridLock = Mathf.Round(hit.point.y);
                            break;
                        case Axis.z:
                            m_ZGridLock = Mathf.Round(hit.point.z);
                            break;
                    }
                    e.Use();
                }
                else if (e.modifiers == EventModifiers.Shift && e.type == EventType.ScrollWheel)
                {
                    switch (m_CurrentGridToDisplay)
                    {
                        case Axis.x:
                            m_XGridLock += Mathf.Sign(e.delta.y) * m_TileSize;
                            break;
                        case Axis.y:
                            m_YGridLock += Mathf.Sign(e.delta.y) * m_TileSize;
                            break;
                        case Axis.z:
                            m_ZGridLock += Mathf.Sign(e.delta.y) * m_TileSize;
                            break;
                    }
                    e.Use();
                }
            }
            else
            {
                if (hit.distance > m_EditRange)
                {
                    return;
                }
                if (m_ToolActive)
                {
                    m_Preview = (hit.point + hit.normal * m_TileSize * 0.1f).ToGridPos(GetChunkOffset(), m_TileSize).ToWorldPos(GetChunkOffset(), m_TileSize);
                }
                else if (m_AlgorithmPainterActive)
                {
                    m_Preview = (hit.point - hit.normal * m_TileSize * 0.1f).ToGridPos(GetChunkOffset(), m_TileSize).ToWorldPos(GetChunkOffset(), m_TileSize);
                }
            }
            if ((m_ToolActive && Selection.activeObject != null && PrefabUtility.GetPrefabAssetType(Selection.activeObject) != PrefabAssetType.NotAPrefab) || m_AlgorithmPainterActive)
            {
                Color cachedColor = Handles.color;
                Handles.color = s_Beige;
                Handles.DrawWireCube(m_Preview + Vector3.up * m_TileSize * 0.5f, Vector3.one);
                Handles.color = cachedColor;

                if (e.modifiers == EventModifiers.Shift && e.type == EventType.MouseDown && e.button == 0)
                {
                    m_AreaStart = m_Preview + Vector3.up * m_TileSize * 0.5f;
                    GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Keyboard);
                    m_DragArea = true;
                }

                if (m_DragArea)
                {
                    if (e.modifiers != EventModifiers.Shift)
                    {
                        m_DragArea = false;
                    }
                    else
                    {
                        Vector3 target = m_Preview + Vector3.up * m_TileSize * 0.5f;
                        float xSign = Mathf.Sign(target.x - m_AreaStart.x);
                        float ySign = Mathf.Sign(target.y - m_AreaStart.y);
                        float zSign = Mathf.Sign(target.z - m_AreaStart.z);
                        Handles.DrawWireCube(m_AreaStart + (target - m_AreaStart) * 0.5f, target - m_AreaStart + new Vector3(xSign, ySign, zSign));

                        if (e.type == EventType.MouseUp && e.button == 0)
                        {
                            for (float x = m_AreaStart.x; (xSign == 1 && x <= target.x) || (xSign == -1 && x >= target.x); x += m_TileSize * xSign)
                            {
                                for (float y = m_AreaStart.y; (ySign == 1 && y <= target.y) || (ySign == -1 && y >= target.y); y += m_TileSize * ySign)
                                {
                                    for (float z = m_AreaStart.z; (zSign == 1 && z <= target.z) || (zSign == -1 && z >= target.z); z += m_TileSize * zSign)
                                    {
                                        if (m_ToolActive)
                                        {
                                            AddTile(new Vector3(x, y, z), Selection.activeObject);
                                        }
                                        else if (m_AlgorithmPainterActive)
                                        {
                                            DefineTile(new Vector3(x, y, z), m_SelectedPaintType);
                                            // Painter
                                        }
                                    }
                                }
                            }
                            m_DragArea = false;
                        }
                    }
                }
                else if (e.button == 0 && e.type == EventType.MouseDown)
                {
                    if (m_ToolActive)
                    {
                        AddTile(m_Preview, Selection.activeObject);
                    }
                    else if (m_AlgorithmPainterActive)
                    {
                        DefineTile(m_Preview, m_SelectedPaintType);
                        // Painter
                    }
                    GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Keyboard);
                    //HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                    e.Use();
                }
            }
            if (m_AlgorithmPainterActive)
            {
                //RenderTiles();
            }
        }
    }



    private float GetDistanceToDisplayedGrid(Vector3 pointDirection, Vector3 cameraPosition)
    {
        if (m_CurrentGridToDisplay == Axis.x)
        {
            float dotAngleToGrid = Vector3.Dot(Vector3.right * Mathf.Sign(m_XGridLock - cameraPosition.x), pointDirection);

            if (dotAngleToGrid > 0f)
            {
                float angle = Mathf.Acos(dotAngleToGrid);
                return Mathf.Abs(cameraPosition.x - m_XGridLock) / Mathf.Cos(angle);
            }
        }
        else if(m_CurrentGridToDisplay == Axis.y)
        {
            float dotAngleToGrid = Vector3.Dot(Vector3.up * Mathf.Sign(m_YGridLock - cameraPosition.y), pointDirection);

            if (dotAngleToGrid > 0f)
            {
                float angle = Mathf.Acos(dotAngleToGrid);
                return Mathf.Abs(cameraPosition.y - m_YGridLock) / Mathf.Cos(angle);
            }
        }
        else if(m_CurrentGridToDisplay == Axis.z)
        {
            float dotAngleToGrid = Vector3.Dot(Vector3.forward * Mathf.Sign(m_ZGridLock - cameraPosition.z), pointDirection);

            if (dotAngleToGrid > 0f)
            {
                float angle = Mathf.Acos(dotAngleToGrid);
                return Mathf.Abs(cameraPosition.z - m_ZGridLock) / Mathf.Cos(angle);
            }
        }

        return Mathf.Infinity;
    }

    private Vector3 GetDisplayedGridNormal(Vector3 cameraPosition)
    {
        if (m_CurrentGridToDisplay == Axis.x)
        {
            return Vector3.right * Mathf.Sign(cameraPosition.x - m_XGridLock);
        }
        else if(m_CurrentGridToDisplay == Axis.y)
        {
            return Vector3.up * Mathf.Sign(cameraPosition.y - m_YGridLock);
        }
        else if(m_CurrentGridToDisplay == Axis.z)
        {
            return Vector3.forward * Mathf.Sign(cameraPosition.z - m_ZGridLock);
        }
        return Vector3.zero;
    }


    public void AddTile(Vector3 worldPosition, TileType tileType)
    {

        CheckBounds(worldPosition, ReSizeType.Add);

        Vector3Int gridPos = worldPosition.ToGridPos(GetChunkOffset(), m_TileSize);


        m_TileGridTypes[gridPos.x, gridPos.y, gridPos.z] = tileType;


    }


    public void AddTile(Vector3 worldPosition, UnityEngine.Object obj)
    {
        //CheckBounds(worldPosition, ReSizeType.Add);

        //Vector3Int gridPos = worldPosition.ToGridPos(GetChunkOffset(), m_TileSize);

        //m_TileGridTypes[gridPos.x, gridPos.y, gridPos.z] = TileType.Block;
        //if (m_SpawnAsPrefab)
        //{
        //    m_TileGrid[gridPos.x, gridPos.y, gridPos.z] = PrefabUtility.InstantiatePrefab(obj) as GameObject;
        //}
        //else
        //{
        //    m_TileGrid[gridPos.x, gridPos.y, gridPos.z] = Instantiate(obj as GameObject);
        //}
        //m_TileGrid[gridPos.x, gridPos.y, gridPos.z].transform.parent = Parent.transform;
        //m_TileGrid[gridPos.x, gridPos.y, gridPos.z].transform.position = gridPos.ToWorldPos(GetChunkOffset(), m_TileSize);

        GameObject go = null;
        if (m_SpawnAsPrefab)
        {
            go = PrefabUtility.InstantiatePrefab(obj) as GameObject;
        }
        else
        {
            go = Instantiate(obj as GameObject);
        }

        go.transform.parent = Parent.transform;
        go.transform.position = worldPosition.ToGridPos(GetChunkOffset(), m_TileSize).ToWorldPos(GetChunkOffset(), m_TileSize);
    }

    

    private void DefineTile(Vector3 worldPosition, TileType type)
    {
        

        CheckBounds(worldPosition, ReSizeType.Add);

        Vector3Int gridPos = worldPosition.ToGridPos(GetChunkOffset(), m_TileSize);

        m_TileGridTypes[gridPos.x, gridPos.y, gridPos.z] = type;
        GameObject tile = GetPool(type).Rent(false);
        tile.transform.position = gridPos.ToWorldPos(GetChunkOffset(), m_TileSize);
        tile.SetActive(true);
    }

    private void RenderTiles()
    {
        Color cachedColor = Handles.color;
        for (int x = 0; x < m_TileGridTypes.GetLength(0); x++)
        {
            for (int y = 0; y < m_TileGridTypes.GetLength(1); y++)
            {
                for (int z = 0; z < m_TileGridTypes.GetLength(2); z++)
                {
                    Handles.color = GetColorByType[m_TileGridTypes[x, y, z]];
                    Handles.CubeHandleCap(
                        0,
                        new Vector3Int(x, y, z).ToWorldPos(GetChunkOffset(), m_TileSize),
                        Quaternion.identity,
                        m_TileSize,
                        EventType.Repaint
                        );
                }
            }
        }
        Handles.color = cachedColor;
    }

    public void RemoveTile(Vector3 worldPosition)
    {
        Vector3Int gridPos = worldPosition.ToGridPos(GetChunkOffset(), m_TileSize);
        m_TileGridTypes[gridPos.x, gridPos.y, gridPos.z] = TileType.Empty;
    }

    private Vector3 GetChunkOffset()
    {
        return new Vector3(xMinMax.min, yMinMax.min, zMinMax.min);
    }


    private bool CheckBounds(Vector3 worldPosition, ReSizeType reSizeType)
    {
        bool result = false;
        while (worldPosition.x < xMinMax.min)
        {
            ReSize(TileDirection.Left, Axis.x, reSizeType);
            xMinMax.min -= m_TileSize;
            result = true;
        }
        while (worldPosition.x > xMinMax.max)
        {
            ReSize(TileDirection.Right, Axis.x, reSizeType);
            xMinMax.max += m_TileSize;
            result = true; 
        }
        while (worldPosition.y < yMinMax.min)
        {
            ReSize(TileDirection.Down, Axis.y, reSizeType);
            yMinMax.min -= m_TileSize;
            result = true;
        }
        while (worldPosition.y > yMinMax.max)
        {
            ReSize(TileDirection.Up, Axis.y, reSizeType);
            yMinMax.max += m_TileSize;
            result = true;
        }
        while (worldPosition.z < zMinMax.min)
        {
            ReSize(TileDirection.Backward, Axis.z, reSizeType);
            zMinMax.min -= m_TileSize;
            result = true;
        }
        while (worldPosition.z > zMinMax.max)
        {
            ReSize(TileDirection.Forward, Axis.z, reSizeType);
            zMinMax.max += m_TileSize;
            result = true;
        }
        return result;
    }

    private void ReSize(TileDirection direction, Axis axis, ReSizeType reSizeType)
    {
        TileType[,,] newTypeArray = new TileType[0, 0, 0];
        GameObject[,,] newObjectArray = new GameObject[0, 0, 0];

        switch (axis)
        {
            case Axis.x:
                newTypeArray = new TileType[ModifyAxisSize(Axis.x, reSizeType), gridHeight, gridDepth];
                newObjectArray = new GameObject[gridWidth, gridHeight, gridDepth];
                break;
            case Axis.y:
                newTypeArray = new TileType[gridWidth, ModifyAxisSize(Axis.y, reSizeType), gridDepth];
                newObjectArray = new GameObject[gridWidth, gridHeight, gridDepth];
                break;
            case Axis.z:
                newTypeArray = new TileType[gridWidth, gridHeight, ModifyAxisSize(Axis.z, reSizeType)];
                newObjectArray = new GameObject[gridWidth, gridHeight, gridDepth];
                break;
        }

        switch (direction)
        {
            case TileDirection.Right:
            case TileDirection.Up:
            case TileDirection.Forward:
                ChangeEndingOfArrays(newObjectArray, newTypeArray, reSizeType, axis);
                break;
            default:
                ChangeBeginningOfArrays(newObjectArray, newTypeArray, reSizeType, axis);
                break;
        }

        m_TileGridTypes = newTypeArray;
    }

    private int ModifyAxisSize(Axis axis, ReSizeType reSizeType)
    {
        switch (axis)
        {
            case Axis.x:
                return reSizeType == ReSizeType.Add ? ++gridWidth : --gridWidth;
            case Axis.y:
                return reSizeType == ReSizeType.Add ? ++gridHeight : --gridHeight;
            case Axis.z:
                return reSizeType == ReSizeType.Add ? ++gridDepth : --gridDepth;
            default:
                return 0;
        }
    }

    private void ChangeBeginningOfArrays(GameObject[,,] newObjectArray, TileType[,,] newTypeArray, ReSizeType reSizeType, Axis axis)
    {
        if (reSizeType == ReSizeType.Add) 
        {
            switch (axis)
            {
                case Axis.x:
                    for (int y = 0; y < gridHeight; y++)
                        for (int z = 0; z < gridDepth; z++)
                        {
                            newTypeArray[0, y, z] = TileType.Empty;
                            newObjectArray[0, y, z] = null;
                        }

                    for (int x = 1; x < gridWidth; x++)
                    {
                        for (int y = 0; y < gridHeight; y++)
                        {
                            for (int z = 0; z < gridDepth; z++)
                            {
                                newTypeArray[x, y, z] = m_TileGridTypes[x - 1, y, z];
                            }
                        }
                    }
                    break;
                case Axis.y:
                    for (int x = 0; x < gridWidth; x++)
                        for (int z = 0; z < gridDepth; z++)
                        {
                            newTypeArray[x, 0, z] = TileType.Empty;
                            newObjectArray[x, 0, z] = null;
                        }

                    for (int x = 0; x < gridWidth; x++)
                    {
                        for (int y = 1; y < gridHeight; y++)
                        {
                            for (int z = 0; z < gridDepth; z++)
                            {
                                newTypeArray[x, y, z] = m_TileGridTypes[x, y - 1, z];
                            }
                        }
                    }
                    break;
                case Axis.z:
                    for (int x = 0; x < gridWidth; x++)
                        for (int y = 0; y < gridHeight; y++)
                        {
                            newTypeArray[x, y, 0] = TileType.Empty;
                            newObjectArray[x, y, 0] = null;
                        }

                    for (int x = 0; x < gridWidth; x++)
                    {
                        for (int y = 0; y < gridHeight; y++)
                        {
                            for (int z = 1; z < gridDepth; z++)
                            {
                                newTypeArray[x, y, z] = m_TileGridTypes[x, y, z - 1];
                            }
                        }
                    }
                    break;
            }
        }
        else
        {
            switch (axis)
            {
                case Axis.x:
                    for (int x = 0; x < gridWidth; x++)
                    {
                        for (int y = 0; y < gridHeight; y++)
                        {
                            for (int z = 0; z < gridDepth; z++)
                            {
                                newTypeArray[x, y, z] = m_TileGridTypes[x + 1, y, z];
                            }
                        }
                    }
                    break;
                case Axis.y:
                    for (int x = 0; x < gridWidth; x++)
                    {
                        for (int y = 0; y < gridHeight; y++)
                        {
                            for (int z = 0; z < gridDepth; z++)
                            {
                                newTypeArray[x, y, z] = m_TileGridTypes[x, y + 1, z];
                            }
                        }
                    }
                    break; 
                case Axis.z:
                    for (int x = 0; x < gridWidth; x++)
                    {
                        for (int y = 0; y < gridHeight; y++)
                        {
                            for (int z = 0; z < gridDepth; z++)
                            {
                                newTypeArray[x, y, z] = m_TileGridTypes[x, y, z + 1];
                            }
                        }
                    }
                    break;
            }
        }
    }

    private void ChangeEndingOfArrays(GameObject[,,] newObjectArray, TileType[,,] newTypeArray, ReSizeType reSizeType, Axis axis)
    {
        //Debug.Log("x: " + newTypeArray.GetLength(0) + "   y: " + newTypeArray.GetLength(1) + "   z: " + newTypeArray.GetLength(2));
        //Debug.Log("x: " + m_TileGridTypes.GetLength(0) + "   y: " + m_TileGridTypes.GetLength(1) + "   z: " + m_TileGridTypes.GetLength(2));  
        if(reSizeType == ReSizeType.Add)
        {
            for(int x = 0; x < m_TileGridTypes.GetLength(0); x++)
            {
                for (int y = 0; y < m_TileGridTypes.GetLength(1); y++)
                {
                    for(int z = 0; z < m_TileGridTypes.GetLength(2); z++)
                    {
                        newTypeArray[x, y, z] = m_TileGridTypes[x, y, z];
                    }
                }
            }

            switch (axis)
            {
                case Axis.x:
                    for (int y = 0; y < gridHeight; y++)
                        for (int z = 0; z < gridDepth; z++)
                        {
                            newTypeArray[m_TileGridTypes.GetLength(0), y, z] = TileType.Empty;
                        }
                    break;
                case Axis.y:
                    for (int x = 0; x < gridWidth; x++)
                        for (int z = 0; z < gridDepth; z++) 
                        {
                            newTypeArray[x, m_TileGridTypes.GetLength(1), z] = TileType.Empty;
                        }
                    break;
                case Axis.z:
                    for (int x = 0; x < gridWidth; x++)
                        for (int y = 0; y < gridHeight; y++)
                        {
                            newTypeArray[x, y, m_TileGridTypes.GetLength(2)] = TileType.Empty;
                        }
                    break;

            }
        }
        else
        {
            for(int x = 0; x < gridWidth; x++)
            {
                for(int y = 0; y < gridHeight; y++)
                {
                    for(int z = 0; z < gridDepth; z++) 
                    {
                        newTypeArray[x, y, z] = m_TileGridTypes[x, y, z];
                    }
                }
            }
        }
    }

    private void DrawGrid(float extent)
    {
        Color cachedColor = Handles.color;
        
        Handles.zTest = UnityEngine.Rendering.CompareFunction.Less;
        if(m_CurrentGridToDisplay == Axis.x)
        {
            Handles.color = s_SoftRed;
            // Draw horizontal lines
            for (float y = -extent; y < extent; y += m_TileSize)
            {
                float yFloored = Mathf.FloorToInt(y);
                if(yFloored < -extent)
                {
                    continue;
                }

                float z = Mathf.Cos(Mathf.Asin(y / extent)) * extent;
                Handles.DrawLine(new Vector3(m_GridPosition.x, Mathf.Round(m_GridPosition.y) + yFloored, m_GridPosition.z - z),new Vector3(m_GridPosition.x, Mathf.Round(m_GridPosition.y) + yFloored, m_GridPosition.z + z));
            }

            // Draw vertical lines
            for(float z = -extent; z < extent; z += m_TileSize)
            {
                float zFloored = Mathf.FloorToInt(z);
                if(zFloored < -extent)
                {
                    continue;
                }

                float y = Mathf.Sin(Mathf.Acos(z / extent)) * extent;
                Handles.DrawLine(new Vector3(m_GridPosition.x, m_GridPosition.y - y, Mathf.Round(m_GridPosition.z) + zFloored), new Vector3(m_GridPosition.x, m_GridPosition.y + y, Mathf.Round(m_GridPosition.z) + zFloored));
            }
        }
        else if(m_CurrentGridToDisplay == Axis.y)
        {
            Handles.color = s_SoftGreen;
            // Draw Depth Lines
            for(float x = -extent; x < extent; x += m_TileSize)
            {
                float xFloored = Mathf.FloorToInt(x);
                if (xFloored < -extent)
                {
                    continue;
                }

                float z = Mathf.Sin(Mathf.Acos(x / extent)) * extent;
                Handles.DrawLine(new Vector3(Mathf.Round(m_GridPosition.x) + xFloored, m_GridPosition.y, m_GridPosition.z - z), new Vector3(Mathf.Round(m_GridPosition.x) + xFloored, m_GridPosition.y, m_GridPosition.z + z));
            }

            // Draw Horizontal Lines
            for(float z = -extent; z < extent; z += m_TileSize)
            {
                float zFloored = Mathf.FloorToInt(z);
                if(zFloored < -extent)
                {
                    continue;
                }

                float x = Mathf.Sin(Mathf.Acos(z / extent)) * extent;
                Handles.DrawLine(new Vector3(m_GridPosition.x - x, m_GridPosition.y, Mathf.Round(m_GridPosition.z) + zFloored), new Vector3(m_GridPosition.x + x, m_GridPosition.y, Mathf.Round(m_GridPosition.z) + zFloored));
            }
        }
        else if(m_CurrentGridToDisplay == Axis.z)
        {
            Handles.color = s_LightBlue;
            // Draw Vertical Lines
            for(float x = -extent; x < extent; x += m_TileSize)
            {
                float xFloored = Mathf.FloorToInt(x);
                if(xFloored < -extent)
                {
                    continue;
                }

                float y = Mathf.Sin(Mathf.Acos(x / extent)) * extent;
                Handles.DrawLine(new Vector3(Mathf.Round(m_GridPosition.x) + xFloored, m_GridPosition.y - y, m_GridPosition.z), new Vector3(Mathf.Round(m_GridPosition.x) + xFloored, m_GridPosition.y + y, m_GridPosition.z)); 
            }

            // Draw Horizontal Lines
            for(float y = -extent; y < extent; y += m_TileSize)
            {
                float yFloored = Mathf.FloorToInt(y);
                if(yFloored < -extent)
                {
                    continue;
                }

                float x = Mathf.Sin(Mathf.Acos(y / extent)) * extent;
                Handles.DrawLine(new Vector3(m_GridPosition.x - x, Mathf.Round(m_GridPosition.y) + yFloored, m_GridPosition.z), new Vector3(m_GridPosition.x + x, Mathf.Round(m_GridPosition.y) + yFloored, m_GridPosition.z));
            }
        }
        Handles.color = cachedColor;
    }

    private void UpdateGridPosition(Vector3 worldPosition)
    {
        switch (m_CurrentGridToDisplay)
        {
            case Axis.x:
                m_GridPosition = new Vector3(m_XGridLock, worldPosition.y, worldPosition.z);
                break;
            case Axis.y:
                m_GridPosition = new Vector3(worldPosition.x, m_YGridLock, worldPosition.z);
                break;
            case Axis.z:
                m_GridPosition = new Vector3(worldPosition.x, worldPosition.y, m_ZGridLock);
                break;
        }
    }

}
#endif  
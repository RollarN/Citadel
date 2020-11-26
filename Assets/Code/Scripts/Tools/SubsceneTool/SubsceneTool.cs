#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class SubsceneTool : EditorWindow
{
    [SerializeField] private SubsceneToolData m_ToolData = null;
    private SerializedObject m_AsSerialized;
    private const string k_ToolDataName = "m_ToolData";
    private const string k_CreateToolDataName = "Create Config File";
    private GUIStyle m_ToggleStyle = null;
    private GUIContent m_ToggleContent = null;
    private GUIStyle m_LabelStyle = null;
    private GUIContent m_LabelContent = null;
    private const float k_Padding = 2.5f;
    private Vector2 m_SubsceneScrollPosition = Vector2.zero;


    [MenuItem("Window/Tools/SubsceneTool")]
    static void Init()
    {
        SubsceneTool window = GetWindow<SubsceneTool>();
        window.Show();
    }

    private void OnEnable()
    {
        string[] assets = AssetDatabase.FindAssets("t:SubsceneToolData");
        if (assets.Length > 0)
        {
            m_ToolData = AssetDatabase.LoadAssetAtPath<SubsceneToolData>(AssetDatabase.GUIDToAssetPath(assets[0]));
        }

        m_AsSerialized = new SerializedObject(this);

        if (m_ToolData)
        {
            m_ToolData.CheckNewOrRemovedSubscenes();
        }
    }

    private void OnGUI()
    {
        SetupGUI();

        GUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(m_AsSerialized.FindProperty(k_ToolDataName));


        if (!m_ToolData)
        {
            if (GUILayout.Button(k_CreateToolDataName))
            {
                m_ToolData = CreateInstance<SubsceneToolData>();
                AssetDatabase.CreateAsset(m_ToolData, "Assets/SubsceneToolData.asset");
            }
            GUILayout.EndHorizontal();
            m_AsSerialized.ApplyModifiedProperties();
            return;
        }
        GUILayout.EndHorizontal();

        m_SubsceneScrollPosition = GUILayout.BeginScrollView(m_SubsceneScrollPosition, GUILayout.Width(position.width));

        m_ToolData.UpdateActiveStates();
        for (int i = 0; i < m_ToolData.Subscenes.Count; i++)
        {
            GUILayout.BeginHorizontal();
            Rect drawRect = GUILayoutUtility.GetRect(m_ToggleStyle.CalcSize(m_ToggleContent).x, m_ToggleStyle.CalcSize(m_ToggleContent).y);
            drawRect.width = m_ToggleStyle.CalcSize(m_ToggleContent).x;
            drawRect.x += k_Padding;
            m_ToolData.SubscenesActiveState[i] = GUI.Toggle(drawRect, m_ToolData.SubscenesActiveState[i], m_ToggleContent, m_ToggleStyle);
            drawRect.x += drawRect.width;
            m_LabelContent = new GUIContent(m_ToolData.Subscenes[i].gameObject.name);
            drawRect.width = m_LabelStyle.CalcSize(m_LabelContent).x;
            //GUILayout.Label(SubsceneToolData.Subscenes[i].gameObject.name);
            GUI.Label(drawRect, m_LabelContent);
            GUILayout.EndHorizontal();
            GUILayout.Space(k_Padding);
        }
        m_ToolData.UpdateSubsceneStates();

        GUILayout.Space(k_Padding);

        GUILayout.EndScrollView();
    } 

    private void SetupGUI()
    {
        if (m_ToggleStyle == null)
        {
            m_ToggleStyle = new GUIStyle(GUI.skin.toggle);
        }
        if (m_ToggleContent == null)
        {
            m_ToggleContent = new GUIContent(string.Empty);
        }
        if(m_LabelStyle == null)
        {
            m_LabelStyle = new GUIStyle(GUI.skin.label);
        }
    }
}
#endif
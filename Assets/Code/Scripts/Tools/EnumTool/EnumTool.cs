
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using UnityEngine.UIElements;
using UnityEditor.VersionControl;

/// <summary>
/// Provides the ability to create, reorder and remove Enums via an editor window.
/// </summary>
public class EnumTool : EditorWindow
{

    [SerializeField] private EnumToolData m_ToolData = null;

#region Constant Strings
    private const string k_CreateEnumControlName = "EnumToolCreateEnum";
    private const string k_AddEnumControlName = "EnumToolAddEnum";
    private const string k_ChoosePath = "Choose Script Path";
    private const string k_PublicEnumAsText = "public enum ";
    private const string k_SelectFolder = "Select Folder";
    private const string k_ToolDataName = "m_ToolData";
    private const string k_CreateEnum = "Create Enum";
    private const string k_Settings = "Tool Settings";
    private const string k_RightCurlyBracket = "}";
    private const string k_ScriptExtension = ".cs";
    private const string k_LeftCurlyBracket = "{";
    private const string k_Indentation = "    ";
#endregion


    private const float k_ReorderButtonPadding = 2f;
    private const float k_Padding = 3f;
    private const int k_NewEnumTextWidth = 150;

    private bool m_Recompile = false;


    private SerializedObject m_AsSerialized = null;
    private readonly GUIContent m_ChoosePathContent = new GUIContent(k_ChoosePath);
    private readonly GUIContent m_CreateEnumContent = new GUIContent(k_CreateEnum);


    private Vector2 m_EnumScrollPosition = Vector2.zero;
    private Vector2 m_EnumReorderButtonSize = new Vector2(16f, 16f);
    private string m_NewEnum = string.Empty;
    private bool[] m_EnumFoldouts = new bool[0];
    private string[] m_AddEnumStrings = new string[0];
    private string[] m_AddEnumControlNames = new string[0];
    private bool m_ShowSettings = false;
    private bool m_ReFocusTextField = false;
    private string m_NameToFocus = string.Empty;



    
    public string ProjectPath
    {
        get
        {
            return Application.dataPath.Substring(0, Application.dataPath.Length - 6); 
        }
    }

    private SerializedObject AsSerialized
    {
        get
        {
            if(m_AsSerialized == null)
            {
                m_AsSerialized = new SerializedObject(this);
            }
            return m_AsSerialized;
        }
    }

    [MenuItem("Window/Tools/EnumHub")]
    static void Init()
    {
        EnumTool window = GetWindow(typeof(EnumTool)) as EnumTool;
        window.Show();
    }

    private void OnEnable()
    {
        string[] assets = AssetDatabase.FindAssets("t:EnumToolData");
        if(assets.Length > 0)
        {
            m_ToolData = AssetDatabase.LoadAssetAtPath<EnumToolData>(AssetDatabase.GUIDToAssetPath(assets[0]));
        }

        if (m_ToolData && (string.IsNullOrWhiteSpace(m_ToolData.enumScriptsInternalPath) || !AssetDatabase.IsValidFolder(m_ToolData.enumScriptsInternalPath.Substring(0, m_ToolData.enumScriptsInternalPath.Length - 1))))
        {
            m_ToolData.enumScriptsInternalPath = Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(assets[0])).Replace('\\', '/') + '/';
        }
    }

    private void SetupGUI()
    {
        if (m_ToolData)
        {
            m_ToolData.enumBackgroundStyle = new GUIStyle();
            m_ToolData.enumBackgroundStyle.normal.background = m_ToolData.BlackTexture;
            m_ToolData.enumBackgroundStyle.margin = new RectOffset();

            m_ToolData.darkGrayLabelStyle = new GUIStyle(GUI.skin.label);
            m_ToolData.darkGrayLabelStyle.normal.background = m_ToolData.DarkGrayTexture;
            m_ToolData.darkGrayLabelStyle.margin = new RectOffset(0, 0, 2, 2);

            m_ToolData.grayLabelStyle = new GUIStyle(GUI.skin.label);
            m_ToolData.grayLabelStyle.normal.background = m_ToolData.GrayTexture;
            m_ToolData.grayLabelStyle.margin = new RectOffset(0, 0, 2, 2);
        }
    }


    private void OnGUI()
    {
        SetupGUI();

        m_EnumScrollPosition = GUILayout.BeginScrollView(m_EnumScrollPosition,  GUILayout.Width(position.width));

        m_ShowSettings = EditorGUILayout.Foldout(m_ShowSettings, k_Settings, true);
        if (m_ShowSettings)
        {
            // Present the EnumToolData variable as a PropertyField.
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(AsSerialized.FindProperty(k_ToolDataName));
            AsSerialized.ApplyModifiedProperties();

            // Present various configurations for the tool.
            if (m_ToolData)
            {
                GUILayout.EndHorizontal();
                
                EditorGUILayout.PropertyField(m_ToolData.AddIconSerialized);
                EditorGUILayout.PropertyField(m_ToolData.SubtractionIconSerialized);
                EditorGUILayout.PropertyField(m_ToolData.UpIconSerialized);
                EditorGUILayout.PropertyField(m_ToolData.DownIconSerialized);


                GUILayout.BeginHorizontal();
                Vector2 buttonSize = new Vector2(GUI.skin.button.CalcSize(m_ChoosePathContent).x, GUI.skin.label.CalcSize(new GUIContent(GetFullPathToScripts())).y);
                if (GUILayout.Button(m_ChoosePathContent, GUILayout.Width(buttonSize.x), GUILayout.Height(buttonSize.y)))
                {
                    m_ToolData.enumScriptsInternalPath = GetRelativePath(EditorUtility.OpenFolderPanel(k_SelectFolder, GetFullPathToScripts(), string.Empty)) + "/";
                    string[] assets = AssetDatabase.FindAssets("t:EnumToolData");
                    if (assets.Length > 0)
                    {
                        Task statusTask = Provider.Status(AssetDatabase.GUIDToAssetPath(AssetDatabase.GUIDToAssetPath(assets[0])));
                        statusTask.Wait();
                        if (Provider.CheckoutIsValid(statusTask.assetList[0]))
                        {
                            Provider.Checkout(statusTask.assetList[0], CheckoutMode.Both);
                        }
                    }
                }

                Rect buttonRect = GUILayoutUtility.GetLastRect();
                Rect pathRect = new Rect(buttonRect.x + buttonRect.width, buttonRect.y, GUI.skin.label.CalcSize(new GUIContent(GetFullPathToScripts())).x, buttonRect.height);
                GUI.Label(pathRect, new GUIContent(GetFullPathToScripts()));
                GUILayout.EndHorizontal();

                m_ToolData.AsSerialized.ApplyModifiedProperties();
            }
            else
            {
                if(GUILayout.Button("Create ScriptableObject", GUILayout.Height(18), GUILayout.Width(150)))
                {
                    m_ToolData = CreateInstance<EnumToolData>();
                    AssetDatabase.CreateAsset(m_ToolData, "Assets/EnumToolData.asset");
                }
                GUILayoutUtility.GetRect(80, 18);
                GUILayout.EndHorizontal();
            }
        }

        // Early out if there is no EnumToolData
        if (!m_ToolData || !m_ToolData.AddIcon || !m_ToolData.SubtractionIcon || !m_ToolData.UpIcon || !m_ToolData.DownIcon)
        {
            EditorGUILayout.LabelField("(Missing Assets in Tool Settings)");
            GUILayout.EndScrollView();
            return;
        }
        GUILayout.Space(10);


        GUILayout.Label("Enums", EditorStyles.boldLabel);
        
        ///
        /// Display each Enum generated by this tool, found in the configurated path.
        ///

        // Gather generated files from the configurated path.
        string[] files = Directory.GetFiles(GetFullPathToScripts());
        List<string> validFiles = new List<string>();
        foreach (string file in files)
        {
            if (Path.GetExtension(file) == k_ScriptExtension)
            {
                StreamReader reader = new StreamReader(file);
                string firstLine = reader.ReadLine();
                if (firstLine != null && firstLine.Contains(k_PublicEnumAsText))
                {
                    validFiles.Add(file);
                }
                reader.Close();
            }
        }
        UpdateArrays(validFiles.Count);

        if(validFiles.Count <= 0)
        {
            GUILayout.Label("Could not find any valid files.");
        }

        GUILayout.BeginVertical(m_ToolData.enumBackgroundStyle);
        for (int i = 0; i < validFiles.Count; i++)
        {
            m_EnumFoldouts[i] = EditorGUILayout.Foldout(m_EnumFoldouts[i], Path.GetFileNameWithoutExtension(validFiles[i]), false);

            if (m_EnumFoldouts[i])
            {
                EditorGUI.indentLevel++;

                string[] lines = File.ReadAllLines(validFiles[i]);
                int firstEnumIndex = 0, maxEnumIndex = 0;
                for (int j = 0; j < lines.Length; j++)
                {
                    if (lines[j].Contains(k_LeftCurlyBracket))
                    {
                        firstEnumIndex = ++j;
                    }
                    else if (lines[j].Contains(k_RightCurlyBracket))
                    {
                        maxEnumIndex = j - 1;
                        break;
                    }
                }

                for (int j = firstEnumIndex; j < maxEnumIndex; j++)
                {
                    if (string.IsNullOrWhiteSpace(lines[j]))
                        continue;

                    GUIContent buttonContent = new GUIContent();
                    buttonContent.image = m_ToolData.AddIcon;

                    string currentEnum = "   " + lines[j].Replace(" ", string.Empty).Replace(",", string.Empty);

                    GUILayout.BeginHorizontal((j % 2 == 0) ? m_ToolData.darkGrayLabelStyle : m_ToolData.grayLabelStyle);


                    Rect baseRect = GUILayoutUtility.GetRect(m_EnumReorderButtonSize.x, m_EnumReorderButtonSize.y);
                    baseRect.width = m_EnumReorderButtonSize.x;
                    baseRect.height = m_EnumReorderButtonSize.y;


                    {
                        Rect buttonRect = baseRect;
                        baseRect.x += buttonRect.width + k_ReorderButtonPadding;
                        if(TexturedButton(buttonRect, m_ToolData.SubtractionIcon))
                        {
                            if (CheckoutEnumIfValid(GetRelativePath(validFiles[i])))
                            {
                                RemoveEnumInFile(validFiles[i], j - firstEnumIndex);
                                m_Recompile = true;
                            }
                        }
                    }

                    if(j - firstEnumIndex == 0)
                    {
                        GUI.enabled = false;
                    }

                    {
                        Rect buttonRect = baseRect;
                        baseRect.x += buttonRect.width + k_ReorderButtonPadding;
                        if (TexturedButton(buttonRect, m_ToolData.UpIcon))
                        {
                            if (j - firstEnumIndex > 0)
                            {
                                if (CheckoutEnumIfValid(GetRelativePath(validFiles[i])))
                                {
                                    MoveEnumUpInFile(validFiles[i], j - firstEnumIndex);
                                    m_Recompile = true;
                                }
                            }
                        }
                    }

                    if (j - firstEnumIndex == 0)
                    {
                        GUI.enabled = true;
                    }

                    if (maxEnumIndex - j == 1)
                    {
                        GUI.enabled = false;
                    }

                    {
                        Rect buttonRect = baseRect;
                        baseRect.x += buttonRect.width + k_ReorderButtonPadding;
                        if(TexturedButton(buttonRect, m_ToolData.DownIcon))
                        {
                            if (CheckoutEnumIfValid(GetRelativePath(validFiles[i])))
                            {
                                MoveEnumDownInFile(validFiles[i], j - firstEnumIndex);
                                m_Recompile = true;
                            }
                        }
                    }

                    if (maxEnumIndex - j == 1)
                    {
                        GUI.enabled = true;
                    }

                    GUIContent enumContent = new GUIContent(currentEnum);
                    baseRect.width = GUI.skin.label.CalcSize(enumContent).x;
                    GUI.Label(baseRect, enumContent);

                    GUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel--;



                GUILayout.BeginHorizontal(maxEnumIndex % 2 == 0 ? m_ToolData.darkGrayLabelStyle : m_ToolData.grayLabelStyle);
                Rect addEnumRect = GUILayoutUtility.GetRect(m_EnumReorderButtonSize.x, m_EnumReorderButtonSize.y);
                addEnumRect.width = m_EnumReorderButtonSize.x;

                Rect addEnumTextRect = addEnumRect;
                addEnumTextRect.x += m_EnumReorderButtonSize.x + k_ReorderButtonPadding;
                addEnumTextRect.width = k_NewEnumTextWidth;

                GUI.SetNextControlName(m_AddEnumControlNames[i]);
                m_AddEnumStrings[i] = EditorGUI.TextField(addEnumTextRect, m_AddEnumStrings[i]).Replace(' ', '_').Trim(',', '.');
                
                if (m_ReFocusTextField && m_NameToFocus.Equals(m_AddEnumControlNames[i]))
                {
                    EditorGUI.FocusTextInControl(m_AddEnumControlNames[i]);
                    m_ReFocusTextField = false;
                }
                if (TexturedButton(addEnumRect, m_ToolData.AddIcon) || CheckReturnPress(m_AddEnumControlNames[i]))
                {
                    if (!string.IsNullOrWhiteSpace(m_AddEnumStrings[i])
                        && !ArrayContainsNewEnum(File.ReadAllLines(validFiles[i]), m_AddEnumStrings[i]) 
                        && !char.IsDigit(m_AddEnumStrings[i][0]))
                    {
                        if (CheckoutEnumIfValid(GetRelativePath(validFiles[i])))
                        {
                            AddToEnumFile(validFiles[i], m_AddEnumStrings[i]);
                            m_AddEnumStrings[i] = string.Empty;
                            m_ReFocusTextField = true;
                            m_NameToFocus = m_AddEnumControlNames[i];
                            m_Recompile = true;
                        }
                    }
                }
                GUILayout.EndHorizontal();

            }            
        }







#region New Enum
        GUILayout.EndVertical();

        GUILayout.EndScrollView();


        GUIStyle createEnumStyle = new GUIStyle();
        createEnumStyle.padding.top = 0;
        createEnumStyle.padding.bottom = 0;
        GUILayout.BeginVertical(createEnumStyle);

        

        GUILayout.BeginHorizontal();
        GUI.DrawTexture(GUILayoutUtility.GetRect(position.width, 1), m_ToolData.DarkGrayTexture);
        GUILayout.EndHorizontal();


        GUILayout.Space(4);


        GUILayout.BeginHorizontal(GUILayout.Height(20));
        Vector2 newEnumButtonSize = GUI.skin.button.CalcSize(m_CreateEnumContent);
        Rect newEnumButtonRect = GUILayoutUtility.GetRect(newEnumButtonSize.x, newEnumButtonSize.y);

        newEnumButtonRect.width = newEnumButtonSize.x;
        newEnumButtonRect.x += k_Padding;

        Rect newEnumTextRect = GUILayoutUtility.GetLastRect();
        newEnumTextRect.x = newEnumButtonRect.x + newEnumButtonRect.width + k_Padding;
        newEnumTextRect.width = k_NewEnumTextWidth;


        GUI.SetNextControlName(k_CreateEnumControlName);
        m_NewEnum = EditorGUI.TextField(newEnumTextRect, m_NewEnum);

        if(m_ReFocusTextField && m_NameToFocus.Equals(k_CreateEnumControlName))
        {
            EditorGUI.FocusTextInControl(k_CreateEnumControlName);
            m_ReFocusTextField = false;
        }

        if (GUI.Button(newEnumButtonRect, m_CreateEnumContent) || CheckReturnPress(k_CreateEnumControlName))
        {
            if (!string.IsNullOrWhiteSpace(m_NewEnum))
            {
                CreateEnumFile(GetFullPathToScripts(), m_NewEnum);
                m_NewEnum = string.Empty;
                m_ReFocusTextField = true;
            }
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(4);
        GUILayout.EndVertical();

#endregion


    }

    bool TexturedButton(Rect rect, Texture2D tex)
    {
        bool clicked = GUI.Button(rect, string.Empty);
        GUI.DrawTexture(rect, tex);
        return clicked;
    }

    private bool ArrayContainsNewEnum(in string[] array, string comparison)
    {
        foreach(string s in array)
        {
            if (s == k_Indentation + comparison + ",")
            {
                return true;
            }
        }
        return false;
    }

    public void CreateEnumFile(string path, string name)
    {
        using (StreamWriter file = File.CreateText(path + name + k_ScriptExtension))
        {
            string maxEnum = string.Empty;

            foreach(char c in name)
            {
                if (char.IsUpper(c) && c != name[0])
                {
                    maxEnum += '_';
                }
                maxEnum += char.ToUpper(c);
            }
            maxEnum += "_MAX";
            file.Write(k_PublicEnumAsText + name + "\n{\n" + k_Indentation + maxEnum + "\n}");
            file.Close();
        }
        AssetDatabase.ImportAsset(m_ToolData.enumScriptsInternalPath + name + k_ScriptExtension);
    }

    public void AddToEnumFile(string fullPath, string enumName)
    {
        string[] lines = File.ReadAllLines(fullPath);
        Queue<string> newLines = new Queue<string>();

        int enumEnd = 0;


        for (int i = 0; i < lines.Length; i++)
        {
            if (i + 1 < lines.Length && lines[i + 1].Contains(k_RightCurlyBracket))
            {
                newLines.Enqueue(k_Indentation + enumName + ',');
                int nextLine = i;
                while (string.IsNullOrWhiteSpace(lines[nextLine]))
                {
                    nextLine++;
                    if(nextLine == lines.Length)
                    {
                        return;
                    }
                }
                newLines.Enqueue(lines[nextLine]);
                enumEnd = nextLine + 1;
                break;
            }
            else if (!string.IsNullOrWhiteSpace(lines[i]))
            {
                newLines.Enqueue(lines[i]);
            }
        }


        for(int i = enumEnd; i < lines.Length; i++)
        {
            newLines.Enqueue(lines[i]);
        }

        StreamWriter file = new StreamWriter(fullPath);
        while (newLines.Count > 0)
        {
            file.WriteLine(newLines.Dequeue());
        }
        file.Close(); 
        m_ToolData.InvokeOnChange();
    }

    private void RemoveEnumInFile(string fullPath, int index)
    {
        string[] lines = File.ReadAllLines(fullPath);
        Queue<string> newLines = new Queue<string>();

        int enumEnd = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains(k_LeftCurlyBracket))
            {
                for (int j = -1; j < index; j++, i++)
                {
                    newLines.Enqueue(lines[i]);
                }
            }
            else if (lines[i].Contains(k_RightCurlyBracket))
            {
                enumEnd = i;
                break;
            }
            else if (!string.IsNullOrWhiteSpace(lines[i]))
            {
                newLines.Enqueue(lines[i]);
            }
        }

        for (int i = enumEnd; i < lines.Length; i++)
        {
            newLines.Enqueue(lines[i]);
        }

        StreamWriter file = new StreamWriter(fullPath);
        while (newLines.Count > 0)
        {
            file.WriteLine(newLines.Dequeue());
        }
        file.Close();
        m_ToolData.InvokeOnChange();
    }

    private void MoveEnumUpInFile(string fullPath, int index)
    {
        string[] lines = File.ReadAllLines(fullPath);
        Queue<string> newLines = new Queue<string>();

        int enumEnd = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains(k_LeftCurlyBracket))
            {
                for (int j = -1; j < index - 1; j++, i++)
                {
                    newLines.Enqueue(lines[i]);
                }
                newLines.Enqueue(lines[i + 1]);
                newLines.Enqueue(lines[i]);
                i++;
            }
            else if (lines[i].Contains(k_RightCurlyBracket))
            {
                enumEnd = i;
                break;
            }
            else if (!string.IsNullOrWhiteSpace(lines[i]))
            {
                newLines.Enqueue(lines[i]);
            }
        }

        for (int i = enumEnd; i < lines.Length; i++)
        {
            newLines.Enqueue(lines[i]);
        }

        StreamWriter file = new StreamWriter(fullPath);
        while (newLines.Count > 0)
        {
            file.WriteLine(newLines.Dequeue());
        }
        file.Close();
        m_ToolData.InvokeOnChange();

    }

    private void MoveEnumDownInFile(string fullPath, int index)
    {
        string[] lines = File.ReadAllLines(fullPath);
        Queue<string> newLines = new Queue<string>();

        int enumEnd = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains(k_LeftCurlyBracket))
            {
                for (int j = -1; j < index; j++, i++)
                {
                    newLines.Enqueue(lines[i]);
                }
                newLines.Enqueue(lines[i + 1]);
                newLines.Enqueue(lines[i]);
                i++;
            }
            else if (lines[i].Contains(k_RightCurlyBracket))
            {
                enumEnd = i;
                break;
            }
            else if (!string.IsNullOrWhiteSpace(lines[i]))
            {
                newLines.Enqueue(lines[i]);
            }
        }

        for (int i = enumEnd; i < lines.Length; i++)
        {
            newLines.Enqueue(lines[i]);
        }

        StreamWriter file = new StreamWriter(fullPath);
        while (newLines.Count > 0)
        {
            file.WriteLine(newLines.Dequeue());
        }
        file.Close(); 
        m_ToolData.InvokeOnChange();
    }

    private void UpdateArrays(int count)
    {
        if(m_EnumFoldouts.Length < count)
        {
            Array.Resize(ref m_EnumFoldouts, count);
        }
        else if(m_EnumFoldouts.Length > count)
        {
            m_EnumFoldouts = new bool[count];
        }

        if(m_AddEnumStrings.Length < count)
        {
            Array.Resize(ref m_AddEnumStrings, count);
            for(int i = 0; i < m_AddEnumStrings.Length; i++)
            {
                m_AddEnumStrings[i] = string.Empty;
            }
        }
        else if(m_AddEnumStrings.Length > count)
        {
            m_AddEnumStrings = new string[count];
        }

        if(m_AddEnumControlNames.Length != count)
        {
            m_AddEnumControlNames = new string[count];
            for (int i = 0;  i < m_AddEnumControlNames.Length; i++)
            {
                m_AddEnumControlNames[i] = k_AddEnumControlName + i.ToString();
            }
        }
    }

    public string GetFullPathToScripts()
    {
        return ProjectPath + m_ToolData.enumScriptsInternalPath;
    }

    private bool CheckReturnPress(string textFieldName)
    {
        return Event.current.rawType == EventType.KeyUp
                && Event.current.keyCode == KeyCode.Return
                && GUI.GetNameOfFocusedControl().Equals(textFieldName);
    }
    public bool CheckoutEnumIfValid(string internalPath)
    {
        Task statusTask = Provider.Status(Provider.GetAssetByPath(internalPath));
        statusTask.Wait();
        if(statusTask.assetList[0].IsState(Asset.States.AddedLocal) || statusTask.assetList[0].IsState(Asset.States.CheckedOutLocal))
        {
            return true;
        }
        else if(statusTask.assetList[0].IsState(Asset.States.CheckedOutRemote) || statusTask.assetList[0].IsState(Asset.States.LockedRemote))
        {
            return false;
        }
        else if (Provider.CheckoutIsValid(statusTask.assetList[0]))
        {
            Task coTask = Provider.Checkout(statusTask.assetList[0], CheckoutMode.Both);
            coTask.Wait();
            return true;
        }
        return false;
    }

    public string GetRelativePath(string fullPath)
    {
        return fullPath.Substring(ProjectPath.Length);
    }

    private void OnLostFocus()
    {
        if (m_Recompile)
        {
            AssetDatabase.Refresh();
            m_Recompile = false;
        }
    }
}



#endif
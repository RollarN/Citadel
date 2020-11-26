#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "EnumToolData", menuName = "ScriptableObjects/EnumToolData")]
public class EnumToolData : ScriptableObject
{
    public event Action OnChange;

    public Texture2D AddIcon = null;
    public Texture2D SubtractionIcon = null;
    public Texture2D UpIcon = null;
    public Texture2D DownIcon = null;

    public Color darkGray = new Color(0.12f, 0.12f, 0.12f, 1);
    public Color gray = new Color(0.3f, 0.3f, 0.3f, 1);
    public Color veryDark = new Color(0.08f, 0.08f, 0.08f, 1);

    [HideInInspector] private Texture2D darkGrayTexture;
    [HideInInspector] private Texture2D grayTexture;
    [HideInInspector] private Texture2D blackTexture;

    public Texture2D DarkGrayTexture
    {
        get
        {
            if (!darkGrayTexture)
            {
                darkGrayTexture = new Texture2D(1, 1);
                darkGrayTexture.SetColor(darkGray);
            }
            return darkGrayTexture;
        }
        private set
        {
            darkGrayTexture = value;
        }
    }

    public Texture2D GrayTexture
    {
        get
        {
            if (!grayTexture)
            {
                grayTexture = new Texture2D(1, 1);
                grayTexture.SetColor(gray);
            }
            return grayTexture;
        }
        private set
        {
            grayTexture = value;
        }
    }

    public Texture2D BlackTexture
    {
        get
        {
            if (!blackTexture)
            {
                blackTexture = new Texture2D(1, 1);
                blackTexture.SetColor(veryDark);
            }
            return blackTexture;
        }
        private set
        {
            blackTexture = value;
        }
    }


    public string enumScriptsInternalPath = string.Empty;


    public GUIStyle enumBackgroundStyle = null;
    public GUIStyle grayLabelStyle = null;
    public GUIStyle darkGrayLabelStyle = null;


    public SerializedProperty AddIconSerialized = null;
    public SerializedProperty SubtractionIconSerialized = null;
    public SerializedProperty UpIconSerialized = null;
    public SerializedProperty DownIconSerialized = null;

    public SerializedObject AsSerialized = null;

    private void OnEnable()
    {
        AsSerialized = new SerializedObject(this);
        AddIconSerialized = AsSerialized.FindProperty("AddIcon");
        SubtractionIconSerialized = AsSerialized.FindProperty("SubtractionIcon");
        UpIconSerialized = AsSerialized.FindProperty("UpIcon");
        DownIconSerialized = AsSerialized.FindProperty("DownIcon");

        //ResetTextures();
    }

    private void ResetTextures()
    {
        //DestroyImmediate(blackTexture);
        //DestroyImmediate(darkGrayTexture);
        //DestroyImmediate(grayTexture);

        

        

        
    }

    public void InvokeOnChange()
    {
        OnChange?.Invoke();
    }

    private void OnValidate()
    {
        //ResetTextures();
    }
}
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileEditorData", menuName = "ScriptableObjects/TileEditorData")]
public class TileEditorData : ScriptableObject
{
    [SerializeField] private GameObject[] m_PaintTypeObjects = new GameObject[(int)TileType.TILETYPE_MAX];
    [SerializeField] [HideInInspector] private TileType[] m_TrackedTypes = new TileType[(int)TileType.TILETYPE_MAX];

    public GameObject[] PaintTypeObjects
    {
        get
        {
            LookForEnumChanges();
            return m_PaintTypeObjects;
        }
    }

    public GameObject GetPaintTypeObject(TileType type)
    {
        return PaintTypeObjects[(int)type];
    }

    private void LookForEnumChanges()
    {
        if(m_PaintTypeObjects.Length != (int)TileType.TILETYPE_MAX)
        {
            UpdatePaintTypeObjects();
            return;
        }

        for(int i = 0; i < m_PaintTypeObjects.Length; i++)
        {
            if(m_TrackedTypes[i] != (TileType)i)
            {
                UpdatePaintTypeObjects();
                m_TrackedTypes = new TileType[(int)TileType.TILETYPE_MAX];
                for(int j = 0; j < (int)TileType.TILETYPE_MAX; j++)
                {
                    m_TrackedTypes[j] = (TileType)j;
                }
            }
        }
    }

    private void UpdatePaintTypeObjects()
    {
        GameObject[] newArray = new GameObject[(int)TileType.TILETYPE_MAX];

        for(int i = 0; i < newArray.Length; i++)
        {
            for (int j = 0; j < m_TrackedTypes.Length; j++)
            {
                if(m_TrackedTypes[j] == (TileType)i)
                {
                    newArray[i] = m_PaintTypeObjects[j];
                    break;
                }   
            }
        }
        
    }
}

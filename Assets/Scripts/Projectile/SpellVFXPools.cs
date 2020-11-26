using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum VFXType
{
    Projectile,
    Impact,
    Lingering
}

[CreateAssetMenu(fileName = "ProjectileConfigData", menuName = "ScriptableObjects/ProjectileConfigData")]
public class SpellVFXPools : ScriptableObject
{
    [SerializeField] [HideInInspector] private GameObject[] m_ProjectilePrefabs = new GameObject[(int)ElementType.ELEMENT_TYPE_MAX];
    [SerializeField] [HideInInspector] private GameObject[] m_ImpactPrefabs = new GameObject[(int)ElementType.ELEMENT_TYPE_MAX];
    [SerializeField] [HideInInspector] private GameObject[] m_LingeringPrefabs = new GameObject[(int)ElementType.ELEMENT_TYPE_MAX];
    [SerializeField] [HideInInspector] private string[] m_TrackedElementTypes = new string[(int)ElementType.ELEMENT_TYPE_MAX];

    [System.NonSerialized] private ObjectPool[] m_ProjectilePools = new ObjectPool[(int)ElementType.ELEMENT_TYPE_MAX];
    [System.NonSerialized] private ObjectPool[] m_ImpactPools = new ObjectPool[(int)ElementType.ELEMENT_TYPE_MAX];
    [System.NonSerialized] private ObjectPool[] m_LingeringPools = new ObjectPool[(int)ElementType.ELEMENT_TYPE_MAX];
    
    private GameObject m_VFXPoolParent = null;
    private const string k_VFXPoolParentName = "VFXPoolParent";

    private GameObject VFXPoolParent
    {
        get
        {
            if(m_VFXPoolParent == null)
            {
                m_VFXPoolParent = new GameObject(k_VFXPoolParentName);
            }
            return m_VFXPoolParent;
        }
    }

#if UNITY_EDITOR
    [SerializeField] private SerializedObject m_AsSerialized = null;
    public SerializedObject AsSerialized
    {
        get
        {
            if(m_AsSerialized == null)
            {
                m_AsSerialized = new SerializedObject(this);
            }
            return m_AsSerialized;
        }
        private set
        {
            m_AsSerialized = value;
        }
    }
#endif

    #region Properties
    public GameObject[] ProjectilePrefabs
    {
        get
        {
            CheckPrefabs<ElementType>();
            return m_ProjectilePrefabs;
        }
        private set
        {
            m_ProjectilePrefabs = value;
        }
    }

    public GameObject[] ImpactPrefabs
    {
        get
        {
            CheckPrefabs<ElementType>();
            return m_ImpactPrefabs;
        }
        private set
        {
            m_ImpactPrefabs = value;
        }
    }

    public GameObject[] LingeringPrefabs
    {
        get
        {
            CheckPrefabs<ElementType>();
            return m_LingeringPrefabs;
        }
        private set
        {
            m_LingeringPrefabs = value;
        }
    }
    #endregion


    public ObjectPool GetVFXPool(VFXType vfxType, ElementType elementType)
    {
        switch (vfxType)
        {
            case VFXType.Projectile:
                return m_ProjectilePools[(int)elementType];
            case VFXType.Impact:
                return m_ImpactPools[(int)elementType];
            case VFXType.Lingering:
                return m_LingeringPools[(int)elementType];
            default:
                return null;
        }
    }


    public void GeneratePools()
    {
        for (int i = 0; i < (int)ElementType.ELEMENT_TYPE_MAX; i++)
        {
            if (ProjectilePrefabs[i] != null && m_ProjectilePools[i] == null)
            {
                m_ProjectilePools[i] = new ObjectPool(10, ProjectilePrefabs[i], 10, VFXPoolParent.transform);
            }
            if (ImpactPrefabs[i] != null && m_ImpactPools[i] == null)
            {
                m_ImpactPools[i] = new ObjectPool(10, ImpactPrefabs[i], 10, VFXPoolParent.transform);
            }
            if (LingeringPrefabs[i] != null && m_LingeringPools[i] == null)
            {
                m_LingeringPools[i] = new ObjectPool(10, LingeringPrefabs[i], 10, VFXPoolParent.transform);
            }
        }
    }

    public void CheckPrefabs<TEnum>() where TEnum : struct, IComparable
    {
        EnumArrayUtility.CheckAssetArrays<TEnum>(
                ref m_ProjectilePrefabs,
                ref m_ImpactPrefabs,
                ref m_LingeringPrefabs,
                ref m_TrackedElementTypes
            );
    }


    //private void UpdatePrefabArrays()
    //{
    //    UpdatePrefabArray(ref m_ProjectilePrefabs);
    //    UpdatePrefabArray(ref m_ImpactPrefabs);
    //    UpdatePrefabArray(ref m_LingeringPrefabs);
    //    UpdateElementArray();
    //}

    //private void UpdatePrefabArray(ref GameObject[] prefabArray)
    //{
    //    GameObject[] newObjectArray = new GameObject[(int)ElementType.ELEMENT_TYPE_MAX];
    //    for(int i = 0; i < (int)ElementType.ELEMENT_TYPE_MAX; i++)
    //    {
    //        for(int j = 0; j < prefabArray.Length; j++)
    //        {
    //            if (prefabArray[j] == null)
    //            {
    //                break;
    //            }
    //            if((ElementType)i == m_TrackedElementTypes[j])
    //            {
    //                newObjectArray[i] = prefabArray[j];
    //                break;
    //            }
    //        }
    //        newObjectArray[i] = null;
    //    }
    //    prefabArray = newObjectArray;
    //}

    //private void UpdateElementArray()
    //{
    //    m_TrackedElementTypes = new ElementType[(int)ElementType.ELEMENT_TYPE_MAX];
    //    for (int i = 0; i < m_TrackedElementTypes.Length; i++)
    //    {
    //        m_TrackedElementTypes[i] = (ElementType)i;
    //    }
    //}
}

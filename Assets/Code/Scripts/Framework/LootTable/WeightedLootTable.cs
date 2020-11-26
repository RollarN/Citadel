using UnityEngine;

public class WeightedLootTable : MonoBehaviour
{
    [HideInInspector] public GameObject[] m_ItemNames = new GameObject[(int)ItemEnums.ITEM_ENUMS_MAX];
    [HideInInspector] public uint[] m_ProbabilityToDrop = new uint[(int)ItemEnums.ITEM_ENUMS_MAX];
    [SerializeField] private ItemEnums[] m_Trackables = new ItemEnums[(int)ItemEnums.ITEM_ENUMS_MAX];
    [SerializeField] private bool m_IsActive = false;
    private readonly ObjectPool[] m_ItemPools = new ObjectPool[(int)ItemEnums.ITEM_ENUMS_MAX];
    private GameObject m_Parent;
    public static WeightedLootTable S_WeightedLootTable { get; set; }

    public ItemEnums[] Trackables
    {
        get => m_Trackables;
        set => m_Trackables = value;
    }

    public GameObject[] ItemNames
    {
        get => m_ItemNames;
        set => m_ItemNames = value;
    }

    public bool IsActive
    {
        get => m_IsActive;
        set => m_IsActive = value;
    }

    private void Awake()
    {
        S_WeightedLootTable = this;
        CreateObjectPools();
    }

    private void CreateObjectPools()
    {
        for (int i = 0; i < (int)ItemEnums.ITEM_ENUMS_MAX; i++)
        {
            if (m_ItemNames[i])
            {
                m_Parent = new GameObject("Pool of " + m_ItemNames[i].name);
                m_Parent.transform.parent = transform;

                m_ItemPools[i] = new ObjectPool(5, m_ItemNames[i], 1, m_Parent.transform);
            }
        }
    }

    private void Spawn(int index, Vector3 spawnPosition)
    {
        var loot = m_ItemPools[index]?.Rent(true);

        if (loot == null)
            return;

        loot.transform.position = spawnPosition;
    }

    public void RandomItem(Vector3 spawnPosition)
    {
        float range = m_ProbabilityToDrop[0];
        if (Random.Range(0, 100) < range)
            Spawn(0, spawnPosition);
        
        //Fixat för att vi bara har ett item
        /*
        uint range = 0;

        for (int i = 0; i < m_ProbabilityToDrop.Length; i++)
        {
            range += m_ProbabilityToDrop[i];
        }
        
        uint selected = (uint)Random.Range(0, range);
        uint topItem = 0;

        for (int i = 0; i < m_ProbabilityToDrop.Length; i++)
        {
            topItem += m_ProbabilityToDrop[i];

            if (selected < topItem)
            {
                Spawn(i, spawnPosition);
                return;
            }
        }*/
    }
}
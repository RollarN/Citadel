using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] private float m_LifeTime = 3f;
    private float m_MaxLifeTime;
    private readonly ItemEnums[] m_Items = new ItemEnums[(int)ItemEnums.ITEM_ENUMS_MAX];

    private void Start()
    {
        m_MaxLifeTime = m_LifeTime;
    }

    private void Awake()
    {
        m_LifeTime = m_MaxLifeTime;
    }

    private void Update()
    {
        m_LifeTime -= Time.deltaTime;

        if (m_LifeTime <= 0f)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<Character>();

        if (!player)
            return;

        foreach (var item in m_Items)
        {
            if (gameObject.name == item.ToString())
            {
                //player.AddItem(gameObject.stats);
            }
        }
    }
}
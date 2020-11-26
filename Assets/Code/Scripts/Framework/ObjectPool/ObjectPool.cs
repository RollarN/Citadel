using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class ObjectPool
{
    private readonly uint m_ExpandBy;
    private readonly GameObject m_Prefab;
    private Transform m_Parent;
    public readonly Stack<GameObject> objects = new Stack<GameObject>();

    /// <summary>
    /// Creates a new ObjectPool
    /// </summary>
    /// <param name="initSize">Initial size of pool.</param>
    /// <param name="prefab">Object to pool.</param>
    /// <param name="expandBy">Amount to expand pool by when its empty.</param>
    /// <param name="parent">Pooled objects parent transform.</param>
    public ObjectPool(uint initSize, GameObject prefab, uint expandBy = 1, Transform parent = null)
    {
        m_ExpandBy = expandBy < 1 ? 1 : expandBy;
        m_Parent = parent;
        m_Prefab = prefab;
        Expand(initSize < 1 ? 1 : initSize);
    }

    private void Expand(uint amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject instance = Object.Instantiate(m_Prefab, m_Parent);
            EmittOnDisable emittOnDisable = instance.AddComponent<EmittOnDisable>();
            emittOnDisable.OnDisableGameObject += UnRent;
            instance.SetActive(false);
            objects.Push(instance);
        }
    }

    private void UnRent(GameObject gameObject)
    {
        objects.Push(gameObject);
    }

    public GameObject Rent(bool activate)
    {
        if (objects.Count == 0)
        {
            Expand(m_ExpandBy);
        }
        GameObject instance = objects.Pop();
        instance = instance != null ? instance : Rent(activate);
        instance.SetActive(activate);
        return instance;
    }
}
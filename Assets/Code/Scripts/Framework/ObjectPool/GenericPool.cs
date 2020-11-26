using System.Collections.Generic;
using UnityEngine;

public abstract class GenericPool<T> : MonoBehaviour where T : Component
{
    [SerializeField]
    private T[] prefab = null;

    public static GenericPool<T> instance { get; private set; }
    private Queue<T> objects = new Queue<T>();

    private void Awake()
    {
        instance = this;
    }
    public T Get()
    {
        if (objects.Count == 0)
        {
            CreateNewObject();
        }

        return objects.Dequeue();
    }

    private void CreateNewObject()
    {
        foreach (var item in prefab)
        {
            var newObject = GameObject.Instantiate(item);
            newObject.gameObject.SetActive(false);
            objects.Enqueue(newObject);
        }
    }

    public void ReturnToPool(T objectToReturn)
    {
        objectToReturn.gameObject.SetActive(false);
        objects.Enqueue(objectToReturn);
    }
}
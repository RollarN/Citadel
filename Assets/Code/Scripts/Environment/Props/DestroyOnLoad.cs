using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DestroyOnLoad : MonoBehaviour
{
    public bool ObjectActiveOnSave { get; set; }

    private void Awake()
    {
        SaveManager.OnLoad += DestroySelf;
    }
    private void OnDestroy()
    {
        SaveManager.OnLoad -= DestroySelf;
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }
}

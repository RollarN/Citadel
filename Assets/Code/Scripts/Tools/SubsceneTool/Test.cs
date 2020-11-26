﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Test : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float Hello;
    public bool isover = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isover = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isover = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isover)
        {
            Debug.Log("isover");
        }
    }
}

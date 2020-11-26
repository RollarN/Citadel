using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    // Start is called before the first frame update
    [HideInInspector]public Transform m_Target;
    [HideInInspector]public Vector3 m_Offset;
    [SerializeField]private float m_Smoothing = 0.5f;
    // Update is called once per frame
    void LateUpdate()
    {
       transform.position = Vector3.Lerp(transform.position, m_Target.position + m_Offset, m_Smoothing);
    }
}

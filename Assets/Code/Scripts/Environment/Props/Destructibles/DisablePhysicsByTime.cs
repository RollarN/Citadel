using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisablePhysicsByTime : MonoBehaviour
{
    public float m_DisableDelay = 5f;
    private bool m_Disabling = false;
    // Start is called before the first frame update
    private void OnEnable()
    {
        InvokeDisable();
    }
    void Start()
    {
    }
    //IN CASE you want to call this from another script lul
    void InvokeDisable()
    {
        if (m_Disabling)
            return;
        m_Disabling = true;
        Invoke("DisablePhysics", m_DisableDelay);
    }
    public void DisablePhysics()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<Collider>().enabled = false;
            transform.GetChild(i).GetComponent<Rigidbody>().isKinematic = true;
        }
        this.enabled = false;

    }
    private void OnDisable()
    {
        CancelInvoke("DisablePhysics");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleDisabler : MonoBehaviour
{
    [SerializeField] ParticleSystem m_System = null;
    private float m_Time = 0f;

    private void OnEnable()
    {
        m_Time = Time.time;
        m_System.Play(true);
    }

    private void OnDisable()
    {
        m_System.Stop(true);
    }

    private void Update()
    {
        if(Time.time - m_Time < m_System.main.duration || m_System.particleCount > 0)
        {
            return;
        }
        gameObject.SetActive(false);
    }
}

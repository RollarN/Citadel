using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeOnTrigger : MonoBehaviour
{
    [SerializeField] private LayerMask m_TriggerLayer = default;
    [SerializeField] private ExplodeProp m_DestructibleComponent = null;
    [SerializeField] private float m_ExplosionForce = 500f;
    [SerializeField] private Transform m_ExplosionPosition = null;
    [SerializeField] private float m_ExplosionRadius = 5f;
    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & m_TriggerLayer) != 0)
        {
            if (m_DestructibleComponent == null)
            {
                GetComponent<IDestructible>()?.Explode(m_ExplosionForce, m_ExplosionPosition.position, m_ExplosionRadius);
            }
            else
            {
                m_DestructibleComponent.Explode(m_ExplosionForce, m_ExplosionPosition.position, m_ExplosionRadius);
            }
        }
    }
}

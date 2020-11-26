using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileTurret : MonoBehaviour
{
    [SerializeField] private SpellData m_ProjectileData;
    [SerializeField] private ParticleSeeker m_LightningVFXParticle;
    [SerializeField] private Transform m_Target;
    // Start is called before the first frame update
    public void Start()
    {
        if (m_ProjectileData.ElementType == ElementType.Lightning)
        {
            m_LightningVFXParticle.target = m_Target;
            m_LightningVFXParticle.GetComponent<ParticleSystem>().Play();
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(m_Target.position, m_ProjectileData.ImpactRadius);
    }
    // Update is called once per frame
    void Update()
    {
        if(m_ProjectileData.ElementType == ElementType.Lightning)
            m_ProjectileData.TriggerSpellImpact(m_Target.position, gameObject);   
    }
}

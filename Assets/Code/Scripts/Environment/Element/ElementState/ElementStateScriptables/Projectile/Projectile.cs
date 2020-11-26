using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Projectile : MonoBehaviour
{
    private SpellData m_ProjectileData;
    private float m_DistanceRemaining;
    private float m_Charge = 1;
    [Tooltip("Maximum amount of props that this can explode at any given frame")]
    private Collider[] m_ExplosivePropOverlapBuffer = new Collider[3];
    public Collider[] ExplosivePropOverLapBuffer { get => m_ExplosivePropOverlapBuffer; set => m_ExplosivePropOverlapBuffer = value; }
    private GameObject sender;

    public void SetProjectileData(SpellData projectileData, GameObject sender, float charge = 1)
    {
        m_Charge = charge;
        m_ProjectileData = projectileData;
        m_DistanceRemaining = m_ProjectileData.Distance;
        this.sender = sender;

        transform.localScale = projectileData.VisualProjectileRadiusScaledByElementType * charge;
    }

    private void Update()
    {
        if (m_DistanceRemaining <= 0)
        {
            gameObject.SetActive(false);
        }
        m_DistanceRemaining -= m_ProjectileData.TravelSpeed * Time.deltaTime * m_Charge;

        m_ProjectileData.UpdateProjectile(this, sender, m_Charge);
    }
}

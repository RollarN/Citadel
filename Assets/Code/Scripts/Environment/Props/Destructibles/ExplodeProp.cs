using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ExplodeProp : MonoBehaviour, IDestructible, ISavable
{
    [SerializeField] private GameObject m_ExplosionPrefab = null;
    [SerializeField] private Vector3 m_RotationOffset = Vector3.zero;
    [SerializeField] private DestructiblePropData m_PropData = null;
    [SerializeField] private float m_ExplosionRadius = 1f;
    [SerializeField] private float m_ExplosionForce = 25f;
    [SerializeField] private float m_ImpactForceTreshholdToExplode = 7f;
    [SerializeField] private SpellData m_OptionalExplosionData;
    [SerializeField] private AudioClip m_explosionClip;
    [SerializeField] private Rigidbody m_RigidBody = null;
    [Header("ExplosionDamage")]
    [SerializeField] private float m_ImpactExplosionDmgRadius = 1f;
    [SerializeField] private float m_ImpactExplosionDmg = 20f;
    [SerializeField] private uint m_ExplosionDmgColliderCount = 2;
    [SerializeField] private LayerMask m_ExplosionDmgLayerMask;
    private Collider[] m_ExplosionColliderBuffer;
    private bool m_exploded = false;
    private GameObject m_InstantiatedExplosionPrefab = null;
    public bool ObjectActiveOnSave { get; set; }
    


    private void Awake()
    {
        if (!m_RigidBody)
        {
            m_RigidBody = GetComponent<Rigidbody>();
            m_ExplosionColliderBuffer = new Collider[m_ExplosionDmgColliderCount];
        }
    }
    private void Start()
    {
        SetUpExplosionPrefab();
    }

    private void SetUpExplosionPrefab()
    {
        if (m_ExplosionPrefab == null)
        {
            Debug.LogWarning($"Explosion version of prefab is missing ({gameObject.name})");
            return;
        }
        m_InstantiatedExplosionPrefab = Instantiate(m_ExplosionPrefab, Vector3.zero, Quaternion.identity);

        
        m_InstantiatedExplosionPrefab.SetActive(false);
    }

    public void Explode(float explosionForce, Vector3 explosionPosition, float explosionRadius, float upwardsModifier = 0f)
    {
        if (m_exploded)
            return;
        m_exploded = true;

        if (m_InstantiatedExplosionPrefab == null)
        {
            Debug.LogWarning($"Explosion version of prefab is missing ({gameObject.name})");
            return;
        }

        m_InstantiatedExplosionPrefab.transform.position = transform.position;
        m_InstantiatedExplosionPrefab.transform.rotation = transform.rotation * Quaternion.Euler(m_RotationOffset);

        if (!m_InstantiatedExplosionPrefab.GetComponent<DestroyOnLoad>())
        {
            m_InstantiatedExplosionPrefab.AddComponent<DestroyOnLoad>();
        }

        m_InstantiatedExplosionPrefab.SetActive(true);
        if (m_OptionalExplosionData)
        {
            m_OptionalExplosionData.TriggerSpellImpact(transform.position, gameObject);
        }
        else
        {
            if (AudioManager.instance && m_explosionClip)
                AudioManager.instance.PlayClipAtPoint(m_explosionClip, explosionPosition, TargetAudioMixer.PropExplosion);

            Array.ForEach(
                Physics.OverlapSphere(transform.position, m_ExplosionRadius, m_PropData.PartsMask),
                collider => collider.GetComponent<Rigidbody>().AddExplosionForce(explosionForce, explosionPosition, explosionRadius, upwardsModifier, ForceMode.Impulse));

            int OverlappingTargets = Physics.OverlapSphereNonAlloc(transform.position, m_ImpactExplosionDmgRadius, m_ExplosionColliderBuffer, m_ExplosionDmgLayerMask);
            if (OverlappingTargets > 0)
                for (int i = 0; i < OverlappingTargets; i++)
                    m_ExplosionColliderBuffer[i].GetComponent<IHealth<ElementType>>()?.TakeDamage(m_ImpactExplosionDmg, ElementType.Neutral);
        }
        


        gameObject.SetActive(false);
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > m_ImpactForceTreshholdToExplode)
            Explode(collision.relativeVelocity.magnitude, transform.position, m_ExplosionRadius);
    }

    public void OnSave(StreamWriter sw, out int addedCount)
    {
        sw.WriteLine(m_exploded);
        sw.WriteLine(transform.position.Serialize());
        sw.WriteLine(transform.rotation.Serialize());
        addedCount = 3;
        if (m_RigidBody)
        {
            sw.WriteLine(m_RigidBody.velocity.Serialize());
            addedCount = 4;
        }

    }

    public void OnLoad(string[] savedData, int startIndex)
    {
        if (m_exploded && ObjectActiveOnSave)
        {
            SetUpExplosionPrefab();
        }

        m_exploded = bool.Parse(savedData[startIndex++]);
        transform.position = savedData[startIndex++].DeserializeToVector3();
        transform.rotation = savedData[startIndex++].DeserializeToQuaternion();
        if (m_RigidBody)
        {
            m_RigidBody.velocity = savedData[startIndex++].DeserializeToVector3();
        }

        gameObject.SetActive(ObjectActiveOnSave);
    }
}

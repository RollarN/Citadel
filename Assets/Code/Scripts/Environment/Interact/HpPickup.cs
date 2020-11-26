using UnityEngine;

public class HpPickup : MonoBehaviour
{
    [Header("Settings")]
    public float lifeTime = 10f;
    public float hp = 30f;
    private float m_LifeTime;
    public AudioClip m_HealClip;
    private void OnEnable()
    {
        //m_LifeTime = lifeTime;
    }

    private void Update()
    {/*
        m_LifeTime -= Time.deltaTime;

        if (m_LifeTime <= 0f)
        {
            gameObject.SetActive(false);
        }*/
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerController>();

        if (!player)
            return;
        player.GeneralAudioSource.PlayOneShot(m_HealClip);
        player.RestoreHealth(hp);
        gameObject.SetActive(false);
    }
}
using UnityEngine;
using UnityEngine.Audio;
public enum TargetAudioMixer
{
    PlayerSpell,
    PlayerDashSpell,
    EnemySpell,
    PlayerCharacter,
    EnemyCharacter,
    PropExplosion,
    PropGeneric,
    PlayerPushBack
    
}
public class AudioManager : MonoBehaviour
{

    public static AudioManager instance;
    [SerializeField] private uint m_AmountOfAudioSources;
    [SerializeField] private GameObject m_AudioSourcePrefab;
    [SerializeField] private AudioMixerGroup m_PlayerSpellMixerGroup;
    [SerializeField] private AudioMixerGroup m_EnemySpellMixerGroup;
    [SerializeField] private AudioMixerGroup m_PlayerCharacterMixerGroup;
    [SerializeField] private AudioMixerGroup m_EnemyCharacterMixerGroup;
    [SerializeField] private AudioMixerGroup m_PropExplosionMixerGroup;
    [SerializeField] private AudioMixerGroup m_PropGenericMixerGroup;
    [SerializeField] private AudioMixerGroup m_PlayerDashMixerGroup;
    [SerializeField] private AudioMixerGroup m_PlayerPushBackMixerGroup;
    private MyAudioSource[] m_AudioSources;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        { 
            instance = this;
        }
        m_AudioSources = new MyAudioSource[(int)m_AmountOfAudioSources];
        for (int i = 0; i < m_AmountOfAudioSources; i++)
        {
            GameObject go = Instantiate(m_AudioSourcePrefab, transform);
            m_AudioSources[i] = go.GetComponent<MyAudioSource>();
        }
    }

    public void Update()
    {
    }
    private bool TryGetAudioSource(out MyAudioSource audioSource)
    {
        audioSource = null;
        for(int i = 0; i < m_AudioSources.Length; i++)
        {
            if (!m_AudioSources[i].gameObject.activeInHierarchy)
            {
                m_AudioSources[i].gameObject.SetActive(true);
                audioSource = m_AudioSources[i];
                return true;
            }
        }
        return false;
    }
    private AudioMixerGroup GetAudioMixerGroup(TargetAudioMixer targetAudioMixer)
    {
        switch(targetAudioMixer)
        {
            case TargetAudioMixer.EnemyCharacter:
                return m_EnemyCharacterMixerGroup;
            case TargetAudioMixer.PlayerCharacter:
                return m_PlayerCharacterMixerGroup;
            case TargetAudioMixer.PlayerSpell:
                return m_PlayerSpellMixerGroup;
            case TargetAudioMixer.EnemySpell:
                return m_EnemySpellMixerGroup;
            case TargetAudioMixer.PropExplosion:
                return m_PropExplosionMixerGroup;
            case TargetAudioMixer.PropGeneric:
                return m_PropGenericMixerGroup;
            case TargetAudioMixer.PlayerDashSpell:
                return m_PlayerDashMixerGroup;
            case TargetAudioMixer.PlayerPushBack:
                return m_PlayerPushBackMixerGroup;
            default:
                return null;
        }
    }

    public void PlayClipAtPoint(AudioClip clip, Vector3 position, TargetAudioMixer targetAudioMixer, float threeDScale = 0.8f) 
    {
        if(TryGetAudioSource(out MyAudioSource audioSource))
        {
            audioSource.transform.position = position;
            audioSource.PlayClipAndDeactivate(clip, GetAudioMixerGroup(targetAudioMixer), threeDScale);
        }
    }


}

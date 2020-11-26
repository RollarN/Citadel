
using UnityEngine;

using UnityEngine.Audio;
public class MyAudioSource : MonoBehaviour
{
   [SerializeField] private AudioSource audioSource;
    public void PlayClipAndDeactivate(AudioClip clip, AudioMixerGroup audioMixerGroup, float threeDScale = 1f)
    {
        gameObject.SetActive(true);
        audioSource.PlayOneShot(clip);
        audioSource.outputAudioMixerGroup = audioMixerGroup;
        audioSource.spatialBlend = threeDScale;
        Invoke("DeActivate", clip.length);
    }
    private void DeActivate()
    {
        gameObject.SetActive(false);
    }

}

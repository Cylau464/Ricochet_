using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    //[Header("AudioClips")]
    //[SerializeField] private AudioClip _winClip = null;
    //[SerializeField] private AudioClip _loseClip = null;

    [Header("Mixer Group")]
    [SerializeField] private AudioMixerGroup ambientGroup = null;
    [SerializeField] private AudioMixerGroup musicGroup = null;
    [SerializeField] private AudioMixerGroup SFXGroup = null;

    private AudioSource ambientSource = null;
    private AudioSource musicSource = null;
    private AudioSource SFXSource = null;

    private static AudioManager current;

    private void Awake()
    {
        if (current != null && current != this)
        {
            //...destroy this. There can be only one AudioManager
            Destroy(gameObject);
            return;
        }

        current = this;
        DontDestroyOnLoad(gameObject);

        ambientSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();
        SFXSource = gameObject.AddComponent<AudioSource>();

        ambientSource.outputAudioMixerGroup = ambientGroup;
        musicSource.outputAudioMixerGroup = musicGroup;
        SFXSource.outputAudioMixerGroup = SFXGroup;
    }

    public static AudioSource PlayClipAtPosition(AudioClip clip, Vector3 position, float volume = 1f, float minDistance = 1f, float pitch = 1f, Transform parent = null)
    {
        GameObject go = new GameObject("One Shot Audio");
        go.transform.position = position;
        go.transform.parent = parent;
        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.spatialBlend = 1f;
        source.minDistance = minDistance;
        source.pitch = pitch;
        source.Play();
        Destroy(go, source.clip.length);

        return source;

    }

    private void OnLevelWasLoaded()
    {
        current.SFXSource.Stop();
    }
}

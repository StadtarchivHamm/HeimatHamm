using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BGMusicManager : Singleton<BGMusicManager>
{
    #region Fields
    private AudioSource m_AudioSource;
    private bool m_IsPlaying;
    private float m_DefaultVolume = 0.5f;
    #endregion
    #region Properties
    public bool IsPlaying { get => m_IsPlaying; }
    #endregion

    #region Methods
    private new void Awake()
    {
        base.Awake();

        m_AudioSource = gameObject.AddComponent<AudioSource>();
        m_AudioSource.playOnAwake = false;
        m_AudioSource.loop = true;
        m_AudioSource.volume = 0.5f;
    }

    public void PlayClip(AudioClip clip)
    {
        StopAllCoroutines();
        m_AudioSource.volume = m_DefaultVolume;
        m_IsPlaying = true;
        m_AudioSource.clip = clip;
        m_AudioSource.Play();
    }

    public void FadeSoundOut(float length = 1.5f)
    {
        m_IsPlaying = false;
        StartCoroutine(FadeSoundOutCoroutine(length));
    }

    public IEnumerator FadeSoundOutCoroutine(float length)
    {
        float timer = 0;
        while (timer < length)
        {
            m_AudioSource.volume = Mathf.Lerp(m_DefaultVolume, 0, Mathf.Sin(timer / length * Mathf.PI * 0.4f));
            timer += Time.deltaTime;
            yield return null;
        }
        m_AudioSource.Stop();
    }
    #endregion
}

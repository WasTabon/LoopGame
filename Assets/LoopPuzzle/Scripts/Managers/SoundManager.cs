using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    private const string SfxMuteKey = "sfx_muted";
    private const string MusicMuteKey = "music_muted";

    private const int SfxSourceCount = 6;
    private readonly List<AudioSource> sfxSources = new List<AudioSource>();
    private AudioSource musicSource;
    private int nextSfxIndex;

    private AudioClip clickClip;
    private AudioClip backClip;
    private AudioClip transitionClip;
    private AudioClip winClip;
    private AudioClip flowClip;

    private bool sfxMuted;
    private bool musicMuted;

    public bool SfxMuted => sfxMuted;
    public bool MusicMuted => musicMuted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        sfxMuted = PlayerPrefs.GetInt(SfxMuteKey, 0) == 1;
        musicMuted = PlayerPrefs.GetInt(MusicMuteKey, 0) == 1;

        BuildSources();
        GenerateClips();
    }

    private void BuildSources()
    {
        for (int i = 0; i < SfxSourceCount; i++)
        {
            GameObject go = new GameObject("SfxSource_" + i);
            go.transform.SetParent(transform);
            AudioSource src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            sfxSources.Add(src);
        }

        GameObject musicGo = new GameObject("MusicSource");
        musicGo.transform.SetParent(transform);
        musicSource = musicGo.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
    }

    private void GenerateClips()
    {
        clickClip = CreateBeep(880f, 0.08f, 0.5f);
        backClip = CreateBeep(440f, 0.08f, 0.5f);
        transitionClip = CreateBeep(620f, 0.12f, 0.4f);
        winClip = CreateArpeggio(new float[] { 523f, 659f, 784f, 1047f }, 0.10f, 0.45f);
        flowClip = CreateSweep(300f, 900f, 0.5f, 0.3f);
    }

    private AudioClip CreateSweep(float startFreq, float endFreq, float duration, float volume)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        AudioClip clip = AudioClip.Create("sweep", sampleCount, 1, sampleRate, false);

        float[] data = new float[sampleCount];
        float phase = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = t / duration;
            float freq = Mathf.Lerp(startFreq, endFreq, progress);
            phase += 2f * Mathf.PI * freq / sampleRate;
            float envelope = Mathf.Sin(progress * Mathf.PI);
            data[i] = Mathf.Sin(phase) * volume * envelope;
        }

        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateArpeggio(float[] frequencies, float noteDuration, float volume)
    {
        int sampleRate = 44100;
        int noteSamples = Mathf.CeilToInt(sampleRate * noteDuration);
        int totalSamples = noteSamples * frequencies.Length;
        AudioClip clip = AudioClip.Create("arpeggio", totalSamples, 1, sampleRate, false);

        float[] data = new float[totalSamples];
        for (int n = 0; n < frequencies.Length; n++)
        {
            for (int i = 0; i < noteSamples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Clamp01(1f - (t / noteDuration));
                data[n * noteSamples + i] = Mathf.Sin(2f * Mathf.PI * frequencies[n] * t) * volume * envelope;
            }
        }

        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateBeep(float frequency, float duration, float volume)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        AudioClip clip = AudioClip.Create("beep_" + frequency, sampleCount, 1, sampleRate, false);

        float[] data = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Clamp01(1f - (t / duration));
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * volume * envelope;
        }

        clip.SetData(data, 0);
        return clip;
    }

    public void PlayClick()
    {
        PlaySfx(clickClip);
    }

    public void PlayBack()
    {
        PlaySfx(backClip);
    }

    public void PlayTransition()
    {
        PlaySfx(transitionClip);
    }

    public void PlayWin()
    {
        PlaySfx(winClip);
    }

    public void PlayFlow()
    {
        PlaySfx(flowClip);
    }

    private void PlaySfx(AudioClip clip)
    {
        if (sfxMuted) return;
        if (clip == null) return;

        AudioSource src = sfxSources[nextSfxIndex];
        nextSfxIndex = (nextSfxIndex + 1) % sfxSources.Count;
        src.PlayOneShot(clip);
    }

    public void SetSfxMuted(bool muted)
    {
        sfxMuted = muted;
        PlayerPrefs.SetInt(SfxMuteKey, muted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetMusicMuted(bool muted)
    {
        musicMuted = muted;
        PlayerPrefs.SetInt(MusicMuteKey, muted ? 1 : 0);
        PlayerPrefs.Save();
        musicSource.mute = muted;
    }
}

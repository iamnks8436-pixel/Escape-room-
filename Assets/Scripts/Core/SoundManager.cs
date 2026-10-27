using System;
using UnityEngine;

/// <summary>
/// SoundManager manages all ambient audio and sound effects in "VR Escape Room — The Lost Relic".
/// 
/// Key Principles (Phase 17):
/// - Audio clips and AudioSources are fully optional; if unassigned, procedural synthesized audio clips
///   are generated at runtime in memory so sound effects work immediately without missing audio files.
/// - Never throws NullReferenceExceptions or errors if audio is missing.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("Audio source for ambient background audio")]
    public AudioSource ambientSource;
    [Tooltip("Audio source for one-shot 2D sound effects")]
    public AudioSource sfxSource;

    [Header("Sound Clips (Optional - procedural fallbacks generated if null)")]
    public AudioClip ambientDroneClip;
    public AudioClip clockTickClip;
    public AudioClip clueDiscoveryClip;
    public AudioClip keypadClickClip;
    public AudioClip wrongCodeClip;
    public AudioClip correctCodeClip;
    public AudioClip chestUnlockClip;
    public AudioClip chestOpenClip;
    public AudioClip keyPickupClip;
    public AudioClip doorUnlockClip;
    public AudioClip doorOpenClip;
    public AudioClip victoryClip;
    public AudioClip defeatClip;
    public AudioClip paperRustleClip;
    public AudioClip chairMoveClip;

    [Header("Volume Configuration")]
    [Range(0f, 1f)] public float ambientVolume = 0.35f;
    [Range(0f, 1f)] public float sfxVolume = 0.85f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Ensure ambient AudioSource exists
        if (ambientSource == null)
        {
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.loop = true;
            ambientSource.playOnAwake = false;
        }

        // Ensure SFX AudioSource exists
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        // Generate procedural audio clips for any missing clips
        GenerateProceduralFallbacks();
    }

    private void Start()
    {
        PlayAmbient();
    }

    /// <summary>
    /// Starts playing looping eerie ambient music / drone.
    /// </summary>
    public void PlayAmbient()
    {
        if (ambientDroneClip != null && ambientSource != null)
        {
            ambientSource.clip = ambientDroneClip;
            ambientSource.volume = ambientVolume;
            ambientSource.loop = true;
            ambientSource.Play();
        }
    }

    public void StopAmbient()
    {
        if (ambientSource != null)
        {
            ambientSource.Stop();
        }
    }

    public void PlayClockTick() => PlaySound(clockTickClip, 0.4f);
    public void PlayButtonClick() => PlaySound(keypadClickClip, 1.0f);
    public void PlayWrongCode() => PlaySound(wrongCodeClip, 1.0f);
    public void PlayCorrectCode() => PlaySound(correctCodeClip, 1.0f);
    public void PlayBoxUnlock() => PlaySound(chestUnlockClip, 1.0f);
    public void PlayBoxOpen() => PlaySound(chestOpenClip, 0.8f);
    public void PlayDoorUnlock() => PlaySound(doorUnlockClip, 1.0f);
    public void PlayDoorOpen() => PlaySound(doorOpenClip, 1.0f);
    public void PlayVictory() => PlaySound(victoryClip, 1.0f);
    public void PlayDefeat() => PlaySound(defeatClip, 1.0f);
    public void PlayPaperRustle() => PlaySound(paperRustleClip, 0.75f);
    public void PlayChairMove() => PlaySound(chairMoveClip, 0.85f);
    public void PlayKeyPickup() => PlaySound(keyPickupClip, 1.0f);
    public void PlaySecretDiscovered() => PlaySound(clueDiscoveryClip, 1.0f);

    private void PlaySound(AudioClip clip, float volumeScale)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
        }
    }

    /// <summary>
    /// Creates in-memory synthesized audio waveforms for any clips not provided via assets.
    /// This ensures all sounds work out of the box with zero external dependencies.
    /// </summary>
    private void GenerateProceduralFallbacks()
    {
        if (ambientDroneClip == null)
            ambientDroneClip = CreateTone(110f, 4.0f, WaveType.SineDrone);

        if (keypadClickClip == null)
            keypadClickClip = CreateTone(880f, 0.06f, WaveType.Click);

        if (wrongCodeClip == null)
            wrongCodeClip = CreateTone(180f, 0.35f, WaveType.Buzzer);

        if (correctCodeClip == null)
            correctCodeClip = CreateTone(784f, 0.45f, WaveType.Chime);

        if (chestUnlockClip == null)
            chestUnlockClip = CreateTone(520f, 0.28f, WaveType.Thud);

        if (chestOpenClip == null)
            chestOpenClip = CreateTone(220f, 0.6f, WaveType.Creak);

        if (doorUnlockClip == null)
            doorUnlockClip = CreateTone(440f, 0.25f, WaveType.Thud);

        if (doorOpenClip == null)
            doorOpenClip = CreateTone(160f, 1.2f, WaveType.Creak);

        if (victoryClip == null)
            victoryClip = CreateTone(523.25f, 2.5f, WaveType.Fanfare);

        if (defeatClip == null)
            defeatClip = CreateTone(130f, 1.8f, WaveType.DefeatHorn);

        if (paperRustleClip == null)
            paperRustleClip = CreateTone(1200f, 0.18f, WaveType.NoiseRustle);

        if (chairMoveClip == null)
            chairMoveClip = CreateTone(180f, 0.30f, WaveType.NoiseScrape);

        if (keyPickupClip == null)
            keyPickupClip = CreateTone(1046.5f, 0.22f, WaveType.Chime);

        if (clueDiscoveryClip == null)
            clueDiscoveryClip = CreateTone(659.25f, 0.50f, WaveType.Chime);

        if (clockTickClip == null)
            clockTickClip = CreateTone(1400f, 0.04f, WaveType.Click);
    }

    private enum WaveType { SineDrone, Click, Buzzer, Chime, Thud, Creak, Fanfare, DefeatHorn, NoiseRustle, NoiseScrape }

    private AudioClip CreateTone(float baseFreq, float duration, WaveType type)
    {
        int sampleRate = 44100;
        int totalSamples = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[totalSamples];

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / totalSamples;

            switch (type)
            {
                case WaveType.SineDrone:
                    // Low pulsing mysterious chamber drone
                    float wobble = 1f + 0.08f * Mathf.Sin(2f * Mathf.PI * 0.4f * t);
                    samples[i] = 0.25f * Mathf.Sin(2f * Mathf.PI * (baseFreq * wobble) * t)
                               + 0.10f * Mathf.Sin(2f * Mathf.PI * (baseFreq * 1.5f) * t);
                    break;

                case WaveType.Click:
                    // Crisp UI click
                    float envClick = Mathf.Exp(- progress * 24f);
                    samples[i] = Mathf.Sin(2f * Mathf.PI * baseFreq * t) * envClick;
                    break;

                case WaveType.Buzzer:
                    // Harsh low sawtooth buzzer for incorrect submission
                    float saw = 2f * (t * baseFreq - Mathf.Floor(t * baseFreq + 0.5f));
                    float envBuzz = 1f - progress;
                    samples[i] = saw * envBuzz * 0.6f;
                    break;

                case WaveType.Chime:
                    // Resonant pleasant harmonic bell
                    float envChime = Mathf.Exp(- progress * 6f);
                    samples[i] = (Mathf.Sin(2f * Mathf.PI * baseFreq * t)
                                + 0.5f * Mathf.Sin(2f * Mathf.PI * (baseFreq * 1.5f) * t)) * envChime * 0.5f;
                    break;

                case WaveType.Thud:
                    // Mechanical lock click / deadbolt latch
                    float envThud = Mathf.Exp(- progress * 18f);
                    samples[i] = Mathf.Sin(2f * Mathf.PI * (baseFreq * (1f - progress * 0.5f)) * t) * envThud * 0.7f;
                    break;

                case WaveType.Creak:
                    // Wooden hinge friction creak
                    float jitter = Mathf.Sin(2f * Mathf.PI * 45f * t);
                    float creakFreq = baseFreq + jitter * 35f;
                    float envCreak = Mathf.Sin(Mathf.PI * progress);
                    samples[i] = Mathf.Sin(2f * Mathf.PI * creakFreq * t) * envCreak * 0.4f;
                    break;

                case WaveType.Fanfare:
                    // Ascending cheerful harmonic chord (C5 - E5 - G5 - C6)
                    float chordFreq = baseFreq;
                    if (progress > 0.25f) chordFreq = baseFreq * 1.25f; // E
                    if (progress > 0.50f) chordFreq = baseFreq * 1.50f; // G
                    if (progress > 0.75f) chordFreq = baseFreq * 2.00f; // C6
                    float envFanfare = Mathf.Exp(- (progress % 0.25f) * 4f);
                    samples[i] = Mathf.Sin(2f * Mathf.PI * chordFreq * t) * envFanfare * 0.5f;
                    break;

                case WaveType.DefeatHorn:
                    // Descending mournful horn
                    float descFreq = Mathf.Lerp(baseFreq, baseFreq * 0.65f, progress);
                    float envDefeat = 1f - progress;
                    samples[i] = (Mathf.Sin(2f * Mathf.PI * descFreq * t)
                                + 0.4f * Mathf.Sin(2f * Mathf.PI * descFreq * 2f * t)) * envDefeat * 0.6f;
                    break;

                case WaveType.NoiseRustle:
                case WaveType.NoiseScrape:
                    // Filtered noise for paper rustle or chair dragging
                    float noise = (UnityEngine.Random.value * 2f - 1f) * Mathf.Sin(Mathf.PI * progress);
                    samples[i] = noise * 0.35f;
                    break;
            }
        }

        AudioClip clip = AudioClip.Create($"Procedural_{type}", totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}

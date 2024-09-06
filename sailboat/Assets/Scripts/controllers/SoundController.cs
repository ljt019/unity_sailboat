using UnityEngine;
using System.Collections;

public class SoundController : MonoBehaviour
{
    [Header("Audio Source References")]
    [SerializeField] public AudioSource calmAmbientSound;
    [SerializeField] public AudioSource stormyAmbientSound;

    public enum SoundState
    {
        Calm,
        Stormy
    }

    private SoundState currentSoundState = SoundState.Calm;

    [Header("Transition Settings")]
    [SerializeField] private float transitionDelay = 2.0f; // Delay before starting the transition
    [SerializeField] private float transitionDuration = 5.0f; // Duration for fading between sounds
    [SerializeField] private float blendOverlap = 0.2f; // Amount of overlap between sounds (0.0 to 0.5)

    private void Start()
    {
        // Ensure both audio sources are initialized and playing
        if (calmAmbientSound != null) calmAmbientSound.Play();
        if (stormyAmbientSound != null) stormyAmbientSound.Play();

        calmAmbientSound.volume = 1;
        stormyAmbientSound.volume = 0;
    }

    public void SetWeatherState(SoundState newState)
    {
        if (currentSoundState != newState)
        {
            currentSoundState = newState;
            StartCoroutine(DelayedTransitionAudio());
        }
    }

    private IEnumerator DelayedTransitionAudio()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(transitionDelay);

        // Start the transition
        yield return StartCoroutine(TransitionAudio());
    }

    private IEnumerator TransitionAudio()
    {
        float elapsedTime = 0;
        float startCalmVolume = calmAmbientSound.volume;
        float startStormyVolume = stormyAmbientSound.volume;
        float targetCalmVolume = (currentSoundState == SoundState.Calm) ? 1 : 0;
        float targetStormyVolume = (currentSoundState == SoundState.Stormy) ? 1 : 0;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;

            // Use a custom curve for a more natural transition
            float curve = CustomEaseInOutCurve(t);

            // Adjust volumes with overlap
            calmAmbientSound.volume = Mathf.Lerp(startCalmVolume, targetCalmVolume, AdjustVolumeForBlend(curve, targetCalmVolume == 1));
            stormyAmbientSound.volume = Mathf.Lerp(startStormyVolume, targetStormyVolume, AdjustVolumeForBlend(curve, targetStormyVolume == 1));

            yield return null;
        }

        calmAmbientSound.volume = targetCalmVolume;
        stormyAmbientSound.volume = targetStormyVolume;
    }

    private float CustomEaseInOutCurve(float t)
    {
        return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
    }

    private float AdjustVolumeForBlend(float t, bool isIncreasing)
    {
        if (isIncreasing)
        {
            return Mathf.Lerp(0f - blendOverlap, 1f, t);
        }
        else
        {
            return Mathf.Lerp(1f, 0f - blendOverlap, t);
        }
    }

    public void SetCalmSound()
    {
        SetWeatherState(SoundState.Calm);
    }

    public void setStormySound()
    {
        SetWeatherState(SoundState.Stormy);
    }

    public SoundState getState()
    {
        return currentSoundState;
    }
}
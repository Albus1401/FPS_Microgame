using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SimpleAmbienceManager : MonoBehaviour
{
    [System.Serializable]
    public class AmbienceState
    {
        public string Name;

        [Tooltip("AudioSource containing this area's loop")]
        public AudioSource Source;

        [Tooltip("Optional mixer snapshot for this area")]
        public AudioMixerSnapshot Snapshot;

        [Range(0f, 1f)]
        public float Volume = 1f;
    }

    [Header("Ambience States")]
    [SerializeField] private AmbienceState[] states;

    [Header("Starting State")]
    [SerializeField] private int startingState;

    [Header("Transition")]
    [SerializeField, Min(0.01f)]
    private float transitionDuration = 2f;

    private readonly List<AudioSource> allSources = new();

    private AudioSource currentSource;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        // Collect all unique AudioSources from the states.
        foreach (AmbienceState state in states)
        {
            if (state.Source != null &&
                !allSources.Contains(state.Source))
            {
                allSources.Add(state.Source);
            }
        }

        foreach (AudioSource source in allSources)
        {
            source.loop = true;
            source.playOnAwake = false;
            source.volume = 0f;
            source.Stop();
        }
    }

    private void Start()
    {
        if (!IsValidState(startingState))
            return;

        AmbienceState state = states[startingState];

        if (state.Source != null)
        {
            currentSource = state.Source;
            currentSource.volume = state.Volume;
            currentSource.Play();
        }

        if (state.Snapshot != null)
            state.Snapshot.TransitionTo(0f);
    }

    public void SwitchTo(
        int stateNumber,
        bool changeSound,
        bool changeSnapshot)
    {
        if (!IsValidState(stateNumber))
        {
            Debug.LogWarning(
                $"Ambience state {stateNumber} does not exist.",
                this
            );

            return;
        }

        AmbienceState targetState = states[stateNumber];

        if (changeSound && targetState.Source != null)
        {
            StartSoundTransition(
                targetState.Source,
                targetState.Volume
            );
        }

        if (changeSnapshot && targetState.Snapshot != null)
        {
            targetState.Snapshot.TransitionTo(
                transitionDuration
            );
        }
    }

    private void StartSoundTransition(
        AudioSource targetSource,
        float targetVolume)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        if (!targetSource.isPlaying)
        {
            targetSource.volume = 0f;
            targetSource.Play();
        }

        currentSource = targetSource;

        fadeCoroutine = StartCoroutine(
            FadeSources(targetSource, targetVolume)
        );
    }

    private IEnumerator FadeSources(
        AudioSource targetSource,
        float targetVolume)
    {
        float[] startingVolumes =
            new float[allSources.Count];

        for (int i = 0; i < allSources.Count; i++)
            startingVolumes[i] = allSources[i].volume;

        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / transitionDuration
            );

            t = Mathf.SmoothStep(0f, 1f, t);

            for (int i = 0; i < allSources.Count; i++)
            {
                AudioSource source = allSources[i];

                float destinationVolume =
                    source == targetSource
                        ? targetVolume
                        : 0f;

                source.volume = Mathf.Lerp(
                    startingVolumes[i],
                    destinationVolume,
                    t
                );
            }

            yield return null;
        }

        foreach (AudioSource source in allSources)
        {
            if (source == targetSource)
            {
                source.volume = targetVolume;
            }
            else
            {
                source.volume = 0f;
                source.Stop();
            }
        }

        fadeCoroutine = null;
    }

    private bool IsValidState(int stateNumber)
    {
        return states != null &&
               stateNumber >= 0 &&
               stateNumber < states.Length;
    }
}
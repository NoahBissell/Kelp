using EasyButtons;
using UnityEngine;

/// <summary>
/// Dynamically normalizes an arbitrary signal to 0-1 using a rolling adaptive max.
/// Useful for acceleration, angular velocity, audio intensity, steering force, etc.
///
/// Pattern:
/// raw signal -> smoothing -> adaptive range estimate -> normalized output
/// </summary>
[System.Serializable]
public class AdaptiveNormalizer
{
    [Header("Signal Smoothing")]
    [Tooltip("Higher = reacts faster to changes")]
    public float signalSmoothing = 8f;

    [Header("Adaptive Range")]
    [Tooltip("How quickly the estimated max falls over time")]
    public float maxDecayRate = 0.5f;

    [Tooltip("Minimum allowed adaptive max")]
    public float minimumMax = 0.01f;

    [Header("Output Shaping")]
    [Tooltip("1 = linear, <1 boosts small values, >1 emphasizes large values")]
    public float responseExponent = 1f;

    [Tooltip("Apply SmoothStep after normalization")]
    public bool smoothStep = true;

    public bool useBaked;

    // Internal state
    float smoothedSignal;
    [SerializeField] float adaptiveMax = 1f;

    /// <summary>
    /// Feed a raw signal and get a stable normalized 0-1 output.
    /// </summary>
    public float Update(float rawSignal, float deltaTime)
    {
        rawSignal = Mathf.Abs(rawSignal);

        // Exponential smoothing
        float signalAlpha = 1f - Mathf.Exp(-signalSmoothing * deltaTime);
        smoothedSignal = Mathf.Lerp(smoothedSignal, rawSignal, signalAlpha);
        
        
        // Adaptive max:
        // rises instantly, falls slowly
        if (!useBaked)
        {
            adaptiveMax *= Mathf.Exp(-maxDecayRate * deltaTime);
            adaptiveMax = Mathf.Max(adaptiveMax, smoothedSignal, minimumMax);
        }

        // Normalize
        float normalized = smoothedSignal / adaptiveMax;
        normalized = Mathf.Clamp01(normalized);

        // Shape response curve
        normalized = Mathf.Pow(normalized, responseExponent);

        // Optional perceptual smoothing
        if (smoothStep)
        {
            normalized = normalized * normalized * (3f - 2f * normalized);
        }

        return normalized;
    }
    

    /// <summary>
    /// Current estimated maximum.
    /// Useful for debugging.
    /// </summary>
    public float CurrentMax => adaptiveMax;

    /// <summary>
    /// Current smoothed signal.
    /// Useful for debugging.
    /// </summary>
    public float CurrentSignal => smoothedSignal;

    /// <summary>
    /// Reset internal state.
    /// </summary>
    public void Reset(float initialMax = 1f)
    {
        smoothedSignal = 0f;
        adaptiveMax = Mathf.Max(initialMax, minimumMax);
    }
}
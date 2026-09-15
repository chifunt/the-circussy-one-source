using UnityEngine;

namespace TheCircussyOne.Visuals
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Light))]
    public sealed class OrganicLightFlicker : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Light targetLight;
        [SerializeField] private bool previewInEditMode;
        [SerializeField] private bool captureBaselineOnEnable = true;
        [SerializeField, Min(0f)] private float baseIntensity = 1f;

        [Header("Brightness")]
        [SerializeField, Range(0f, 2f)] private float intensityVariance = 0.22f;
        [SerializeField, Range(0f, 2f)] private float minimumMultiplier = 0.72f;
        [SerializeField, Range(0f, 3f)] private float maximumMultiplier = 1.35f;
        [SerializeField, Min(0.01f)] private float noiseSpeed = 1.15f;
        [SerializeField, Min(0.01f)] private float secondaryNoiseSpeed = 3.7f;
        [SerializeField, Range(0f, 1f)] private float secondaryNoiseWeight = 0.28f;
        [SerializeField, Min(0.01f)] private float responseSpeed = 12f;

        [Header("Organic Bursts")]
        [SerializeField, Range(0f, 1f)] private float burstChancePerSecond = 0.12f;
        [SerializeField, Range(0f, 1f)] private float burstStrength = 0.18f;
        [SerializeField, Range(0.04f, 1.5f)] private float burstDurationRangeMin = 0.16f;
        [SerializeField, Range(0.04f, 1.5f)] private float burstDurationRangeMax = 0.42f;
        [SerializeField, Range(0f, 1f)] private float dimBurstBias = 0.68f;

        [Header("Optional Color And Range")]
        [SerializeField, Range(0f, 1f)] private float rangeVariance;
        [SerializeField, Range(0f, 1f)] private float warmColorVariance;
        [SerializeField] private Color warmColor = new(1f, 0.79f, 0.45f, 1f);
        [SerializeField] private Color coolColor = new(1f, 0.93f, 0.75f, 1f);

        [Header("Seed")]
        [SerializeField] private bool randomizeSeedOnEnable = true;
        [SerializeField] private int seed = 37191;

        private float baseRange;
        private Color baseColor = Color.white;
        private float currentIntensity;
        private float nextBurstCheckTime;
        private float burstStartTime;
        private float burstDuration;
        private float burstDirection;
        private float burstPhase;
        private float noiseOffsetA;
        private float noiseOffsetB;
        private bool hasCapturedBaseline;

        private void Reset()
        {
            targetLight = GetComponent<Light>();
            CaptureBaseline();
        }

        private void OnValidate()
        {
            if (targetLight == null)
            {
                targetLight = GetComponent<Light>();
            }

            if (maximumMultiplier < minimumMultiplier)
            {
                maximumMultiplier = minimumMultiplier;
            }

            if (burstDurationRangeMax < burstDurationRangeMin)
            {
                burstDurationRangeMax = burstDurationRangeMin;
            }

            if (!Application.isPlaying && previewInEditMode)
            {
                Apply(Time.realtimeSinceStartup, 0f);
            }
        }

        private void OnEnable()
        {
            if (targetLight == null)
            {
                targetLight = GetComponent<Light>();
            }

            if (captureBaselineOnEnable || !hasCapturedBaseline)
            {
                CaptureBaseline();
            }

            int runtimeSeed = randomizeSeedOnEnable ? Random.Range(int.MinValue, int.MaxValue) : seed;
            noiseOffsetA = Mathf.Abs(runtimeSeed * 0.0173f) % 1000f;
            noiseOffsetB = Mathf.Abs(runtimeSeed * 0.0437f + 19.17f) % 1000f;
            currentIntensity = baseIntensity;
            burstStartTime = -999f;
            nextBurstCheckTime = Time.realtimeSinceStartup + 0.1f;
        }

        private void OnDisable()
        {
            if (targetLight == null || !hasCapturedBaseline)
            {
                return;
            }

            targetLight.intensity = baseIntensity;
            targetLight.range = baseRange;
            targetLight.color = baseColor;
        }

        private void Update()
        {
            if (!Application.isPlaying && !previewInEditMode)
            {
                return;
            }

            Apply(Time.realtimeSinceStartup, Time.unscaledDeltaTime);
        }

        public void CaptureBaseline()
        {
            if (targetLight == null)
            {
                return;
            }

            baseIntensity = targetLight.intensity;
            baseRange = targetLight.range;
            baseColor = targetLight.color;
            hasCapturedBaseline = true;
        }

        private void Apply(float time, float deltaTime)
        {
            if (targetLight == null)
            {
                return;
            }

            UpdateBurst(time);
            float targetMultiplier = EvaluateMultiplier(time);
            float targetIntensity = baseIntensity * targetMultiplier;
            float blend = deltaTime <= 0f ? 1f : 1f - Mathf.Exp(-responseSpeed * deltaTime);
            currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, blend);
            targetLight.intensity = Mathf.Max(0f, currentIntensity);

            if (rangeVariance > 0f)
            {
                float rangeMultiplier = Mathf.Lerp(1f, targetMultiplier, rangeVariance);
                targetLight.range = Mathf.Max(0f, baseRange * rangeMultiplier);
            }

            if (warmColorVariance > 0f)
            {
                float colorBlend = Mathf.InverseLerp(minimumMultiplier, maximumMultiplier, targetMultiplier);
                Color flickerColor = Color.Lerp(warmColor, coolColor, colorBlend);
                targetLight.color = Color.Lerp(baseColor, flickerColor, warmColorVariance);
            }
        }

        private float EvaluateMultiplier(float time)
        {
            float primary = CenteredNoise(noiseOffsetA + time * noiseSpeed);
            float secondary = CenteredNoise(noiseOffsetB + time * secondaryNoiseSpeed) * secondaryNoiseWeight;
            float burst = EvaluateBurst(time);
            float multiplier = 1f + (primary + secondary) * intensityVariance + burst;
            return Mathf.Clamp(multiplier, minimumMultiplier, maximumMultiplier);
        }

        private void UpdateBurst(float time)
        {
            if (burstChancePerSecond <= 0f || burstStrength <= 0f || time < nextBurstCheckTime)
            {
                return;
            }

            const float CheckInterval = 0.15f;
            float chance = burstChancePerSecond * CheckInterval;
            if (Random.value < chance)
            {
                burstStartTime = time;
                burstDuration = Random.Range(burstDurationRangeMin, burstDurationRangeMax);
                burstDirection = Random.value < dimBurstBias ? -1f : 1f;
                burstPhase = Random.Range(0.82f, 1.18f);
            }

            nextBurstCheckTime = time + CheckInterval;
        }

        private float EvaluateBurst(float time)
        {
            if (burstDuration <= 0f)
            {
                return 0f;
            }

            float age = (time - burstStartTime) / burstDuration;
            if (age < 0f || age > 1f)
            {
                return 0f;
            }

            float envelope = Mathf.Sin(age * Mathf.PI);
            float flutter = 0.72f + 0.28f * Mathf.Sin(age * Mathf.PI * 5f * burstPhase);
            return burstDirection * burstStrength * envelope * flutter;
        }

        private static float CenteredNoise(float value)
        {
            return Mathf.PerlinNoise(value, value * 0.37f + 11.23f) * 2f - 1f;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace TheCircussyOne.Visuals
{
    public sealed class MainMenuBackdropAnimator : MonoBehaviour
    {
        private const string BulbName = "Menu Animated Bulb";
        private const string SpotlightName = "Menu Animated Spotlight";

        [SerializeField, Min(0f)] private float bulbPulseSpeed = 0.85f;
        [SerializeField, Range(0f, 1f)] private float bulbPulseAmount = 0.18f;
        [SerializeField, Min(0f)] private float spotlightDriftSpeed = 0.16f;
        [SerializeField, Range(0f, 18f)] private float spotlightDriftDegrees = 5f;

        private readonly List<LightState> bulbs = new();
        private readonly List<LightState> spotlights = new();

        private void Awake()
        {
            CacheLights();
        }

        private void OnEnable()
        {
            if (bulbs.Count == 0 && spotlights.Count == 0)
            {
                CacheLights();
            }
        }

        private void Update()
        {
            float time = Time.unscaledTime;
            for (int i = 0; i < bulbs.Count; i++)
            {
                Light light = bulbs[i].Light;
                if (light == null)
                {
                    continue;
                }

                float pulse = 1f + Mathf.Sin(time * bulbPulseSpeed + bulbs[i].Phase) * bulbPulseAmount;
                light.intensity = bulbs[i].BaseIntensity * pulse;
            }

            for (int i = 0; i < spotlights.Count; i++)
            {
                Light light = spotlights[i].Light;
                if (light == null)
                {
                    continue;
                }

                float yaw = Mathf.Sin(time * spotlightDriftSpeed + spotlights[i].Phase) * spotlightDriftDegrees;
                float pitch = Mathf.Cos(time * spotlightDriftSpeed * 0.73f + spotlights[i].Phase) * spotlightDriftDegrees * 0.42f;
                light.transform.localRotation = spotlights[i].BaseLocalRotation * Quaternion.Euler(pitch, yaw, 0f);
            }
        }

        private void CacheLights()
        {
            bulbs.Clear();
            spotlights.Clear();
            Light[] lights = GetComponentsInChildren<Light>(includeInactive: true);
            for (int i = 0; i < lights.Length; i++)
            {
                Light light = lights[i];
                if (light == null)
                {
                    continue;
                }

                if (light.name.StartsWith(BulbName, System.StringComparison.Ordinal))
                {
                    bulbs.Add(new LightState(light, i * 0.67f));
                }
                else if (light.name.StartsWith(SpotlightName, System.StringComparison.Ordinal))
                {
                    spotlights.Add(new LightState(light, i * 0.91f));
                }
            }
        }

        private readonly struct LightState
        {
            public LightState(Light light, float phase)
            {
                Light = light;
                BaseIntensity = light != null ? light.intensity : 0f;
                BaseLocalRotation = light != null ? light.transform.localRotation : Quaternion.identity;
                Phase = phase;
            }

            public Light Light { get; }
            public float BaseIntensity { get; }
            public Quaternion BaseLocalRotation { get; }
            public float Phase { get; }
        }
    }
}

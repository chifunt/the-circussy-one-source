using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Visuals
{
    [RequireComponent(typeof(TextMeshPro))]
    public sealed class DamageNumberView : MonoBehaviour
    {
        [SerializeField] private TextMeshPro text;
        [SerializeField] private Renderer textRenderer;

        public bool IsActive => gameObject.activeInHierarchy;

        private void Awake()
        {
            ResolveComponents();
        }

        public void Prepare(DamageFeedbackVisualConfig config, int amount, Vector3 position)
        {
            ResolveComponents();
            transform.position = position;
            transform.localScale = Vector3.one * Mathf.Max(0.001f, config.baseWorldScale * config.startScale);
            gameObject.SetActive(true);

            text.text = amount.ToString();
            if (config.damageNumberFont != null)
            {
                text.font = config.damageNumberFont;
            }

            text.fontSize = config.fontSize;
            text.color = config.numberColor;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.richText = false;

            if (textRenderer != null)
            {
                textRenderer.sortingOrder = config.sortingOrder;
                textRenderer.shadowCastingMode = ShadowCastingMode.Off;
                textRenderer.receiveShadows = false;
                textRenderer.lightProbeUsage = LightProbeUsage.Off;
                textRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            }
        }

        public void SetAmount(int amount)
        {
            ResolveComponents();
            text.text = amount.ToString();
        }

        public void ApplyFrame(DamageNumberFrame frame, Camera camera)
        {
            ResolveComponents();
            transform.position = frame.Position;
            transform.localScale = Vector3.one * frame.Scale;
            text.color = frame.Color;

            if (camera == null)
            {
                return;
            }

            Vector3 awayFromCamera = transform.position - camera.transform.position;
            if (awayFromCamera.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(awayFromCamera.normalized, camera.transform.up);
            }
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }

        private void ResolveComponents()
        {
            if (text == null)
            {
                text = GetComponent<TextMeshPro>();
            }

            if (textRenderer == null)
            {
                textRenderer = GetComponent<Renderer>();
            }
        }
    }
}

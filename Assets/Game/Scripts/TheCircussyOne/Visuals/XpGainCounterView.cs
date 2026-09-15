using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;

namespace TheCircussyOne.Visuals
{
    [RequireComponent(typeof(TextMeshPro))]
    public sealed class XpGainCounterView : MonoBehaviour
    {
        [SerializeField] private TextMeshPro text;
        [SerializeField] private Renderer textRenderer;

        public bool IsActive => gameObject.activeInHierarchy;
        public string Text => text != null ? text.text : string.Empty;
        public Color TextColor => text != null ? text.color : Color.clear;

        private void Awake()
        {
            ResolveComponents();
        }

        public void Prepare(XpGainCounterVisualConfig config)
        {
            ResolveComponents();
            gameObject.SetActive(true);

            if (config != null && config.font != null)
            {
                text.font = config.font;
            }

            text.fontSize = config != null ? config.fontSize : 3.6f;
            text.color = config != null ? config.textColor : Color.cyan;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.richText = false;

            if (textRenderer != null)
            {
                textRenderer.sortingOrder = config != null ? config.sortingOrder : 245;
                textRenderer.shadowCastingMode = ShadowCastingMode.Off;
                textRenderer.receiveShadows = false;
                textRenderer.lightProbeUsage = LightProbeUsage.Off;
                textRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            }
        }

        public void ApplyFrame(XpGainCounterFrame frame, Camera camera)
        {
            ResolveComponents();
            if (!frame.Visible)
            {
                Deactivate();
                return;
            }

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            transform.position = frame.Position;
            transform.localScale = Vector3.one * frame.Scale;
            text.text = frame.Text;
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

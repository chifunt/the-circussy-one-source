using UnityEngine;
using UnityEngine.UIElements;

namespace TheCircussyOne.Visuals
{
    public sealed class MarqueeButtonEffectHost : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private string targetClassName = "marquee-button";
        [SerializeField] private MarqueeButtonEffectSettings settings = new();

        private MarqueeButtonEffectController controller;
        private VisualElement root;

        public void Register(VisualElement target)
        {
            EnsureController();
            controller?.Register(target);
        }

        public void Pulse(VisualElement target)
        {
            controller?.Pulse(target);
        }

        public void Rebuild()
        {
            BindRoot();
            EnsureController();
            controller?.Clear();
            if (root == null || string.IsNullOrWhiteSpace(targetClassName))
            {
                return;
            }

            root.Query<VisualElement>(className: targetClassName).ForEach(Register);
        }

        private void Awake()
        {
            document ??= GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            Rebuild();
        }

        private void Update()
        {
            controller?.Tick(Time.unscaledDeltaTime, Time.unscaledTime);
        }

        private void OnDisable()
        {
            controller?.Dispose();
            controller = null;
            root = null;
        }

        private void BindRoot()
        {
            document ??= GetComponent<UIDocument>();
            root = document != null ? document.rootVisualElement : null;
        }

        private void EnsureController()
        {
            controller ??= new MarqueeButtonEffectController(settings);
        }
    }
}

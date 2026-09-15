using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class XpGainCounterFactory
    {
        private readonly XpGainCounterVisualConfig config;
        private readonly XpGainCounterView prefab;
        private readonly Transform root;
        private readonly string instanceName;
        private XpGainCounterView view;

        public XpGainCounterFactory(XpGainCounterVisualConfig config, XpGainCounterView prefab, Transform root, string instanceName = "XP Gain Counter")
        {
            this.config = config;
            this.prefab = prefab;
            this.root = root;
            this.instanceName = string.IsNullOrWhiteSpace(instanceName) ? "XP Gain Counter" : instanceName;
        }

        public XpGainCounterView View => view;

        public int Prewarm()
        {
            bool alreadyCreated = view != null;
            XpGainCounterView target = GetOrCreate();
            if (target == null)
            {
                return 0;
            }

            Release();
            return alreadyCreated ? 0 : 1;
        }

        public XpGainCounterView GetOrCreate()
        {
            if (view != null)
            {
                return view;
            }

            if (prefab == null)
            {
                return null;
            }

            view = Object.Instantiate(prefab, root);
            view.name = instanceName;
            view.Prepare(config);
            return view;
        }

        public void Release()
        {
            if (view == null)
            {
                return;
            }

            view.Deactivate();
            if (root != null)
            {
                view.transform.SetParent(root, false);
            }
        }
    }
}

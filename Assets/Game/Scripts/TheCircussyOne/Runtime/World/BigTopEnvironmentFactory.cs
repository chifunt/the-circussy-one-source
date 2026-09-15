using Cysharp.Threading.Tasks;
using TheCircussyOne.Config;
using TheCircussyOne.Rules;
using TheCircussyOne.Visuals;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public sealed class BigTopEnvironmentFactory
    {
        public GameObject CreateFallbackRoot()
        {
            return new GameObject("Big Top Environment Runtime Root");
        }

        public BigTopEnvironmentView CreateView(
            Transform parent,
            BigTopEnvironmentSpec spec,
            RunWorldGenerationConfig config)
        {
            var viewObject = new GameObject("Big Top Environment Visual");
            viewObject.transform.SetParent(parent, worldPositionStays: false);
            viewObject.transform.localPosition = Vector3.zero;
            viewObject.transform.localRotation = Quaternion.identity;
            viewObject.transform.localScale = Vector3.one;
            BigTopEnvironmentView view = viewObject.AddComponent<BigTopEnvironmentView>();
            view.Configure(spec, config);
            return view;
        }

        public async UniTask<BigTopEnvironmentView> CreateViewAsync(
            Transform parent,
            BigTopEnvironmentSpec spec,
            RunWorldGenerationConfig config,
            WorldLoadTimingDiagnostics timing = null)
        {
            var viewObject = new GameObject("Big Top Environment Visual");
            viewObject.transform.SetParent(parent, worldPositionStays: false);
            viewObject.transform.localPosition = Vector3.zero;
            viewObject.transform.localRotation = Quaternion.identity;
            viewObject.transform.localScale = Vector3.one;
            BigTopEnvironmentView view = viewObject.AddComponent<BigTopEnvironmentView>();
            await view.ConfigureAsync(spec, config, timing);
            return view;
        }

        public void DestroyView(BigTopEnvironmentView view)
        {
            DestroyObject(view != null ? view.gameObject : null);
        }

        public void DestroyRoot(GameObject root)
        {
            DestroyObject(root);
        }

        private static void DestroyObject(Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(target);
                return;
            }

            Object.DestroyImmediate(target);
        }
    }
}

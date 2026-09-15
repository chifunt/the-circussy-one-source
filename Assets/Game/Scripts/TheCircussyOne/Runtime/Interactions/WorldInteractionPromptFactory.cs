using TheCircussyOne.Config;
using TheCircussyOne.Visuals;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheCircussyOne.Runtime
{
    public sealed class WorldInteractionPromptFactory
    {
        private readonly WorldInteractionPromptVisualConfig config;
        private readonly WorldInteractionPromptView prefab;
        private readonly Transform root;
        private readonly Material textMaterial;
        private readonly Material barMaterial;
        private WorldInteractionPromptView view;

        public WorldInteractionPromptFactory(
            WorldInteractionPromptVisualConfig config,
            WorldInteractionPromptView prefab,
            Transform root,
            Material textMaterial = null,
            Material barMaterial = null)
        {
            this.config = config;
            this.prefab = prefab;
            this.root = root;
            this.textMaterial = textMaterial;
            this.barMaterial = barMaterial;
        }

        public WorldInteractionPromptView View
        {
            get
            {
                if (view == null)
                {
                    view = null;
                    return null;
                }

                return view;
            }
        }

        public int Prewarm()
        {
            bool alreadyCreated = view != null;
            WorldInteractionPromptView target = GetOrCreate();
            if (target == null)
            {
                return 0;
            }

            target.Deactivate();
            return alreadyCreated ? 0 : 1;
        }

        public WorldInteractionPromptView GetOrCreate()
        {
            if (view != null)
            {
                return view;
            }

            view = null;

            if (prefab != null)
            {
                view = Object.Instantiate(prefab, root);
                view.name = "World Interaction Prompt";
            }
            else
            {
                var promptObject = new GameObject("World Interaction Prompt");
                if (root != null)
                {
                    promptObject.transform.SetParent(root, false);
                }

                view = promptObject.AddComponent<WorldInteractionPromptView>();
            }

            view.ConfigureDefaults(config, textMaterial, barMaterial);
            return view;
        }

        public void Clear()
        {
            WorldInteractionPromptView target = view;
            view = null;
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(target.gameObject);
            }
            else
            {
                Object.DestroyImmediate(target.gameObject);
            }
        }
    }
}

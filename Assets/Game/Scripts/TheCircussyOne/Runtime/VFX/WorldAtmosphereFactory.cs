using UnityEngine;
using TheCircussyOne.Config;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class WorldAtmosphereFactory
    {
        private readonly WorldAmbientDustView dustPrefab;
        private readonly Transform root;

        public WorldAtmosphereFactory(WorldAmbientDustView dustPrefab, Transform root)
        {
            this.dustPrefab = dustPrefab;
            this.root = root;
        }

        public WorldAmbientDustView CreateDust(VfxVisualConfig config)
        {
            return CreateDust(config, WorldAtmosphereLayerId.AmbientDust, "World Ambient Dust");
        }

        public WorldAmbientDustView CreateDust(
            VfxVisualConfig config,
            WorldAtmosphereLayerId layer,
            string name)
        {
            if (dustPrefab == null)
            {
                return null;
            }

            WorldAmbientDustView view = Object.Instantiate(dustPrefab, root);
            view.name = string.IsNullOrWhiteSpace(name) ? layer.ToString() : name;
            view.ConfigureDefaults(config, layer);
            return view;
        }

        public void DestroyDust(WorldAmbientDustView view)
        {
            if (view == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(view.gameObject);
            }
            else
            {
                Object.DestroyImmediate(view.gameObject);
            }
        }
    }
}

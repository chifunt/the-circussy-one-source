using System;
using UnityEngine;
using VContainer.Unity;
using TheCircussyOne.Config;
using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class WorldAtmosphereSystem : IStartable, ITickable, IDisposable
    {
        private readonly VfxVisualConfig config;
        private readonly WorldAtmosphereFactory factory;
        private readonly PlayerView player;
        private WorldAmbientDustView activeDust;
        private WorldAmbientDustView activeFloorHaze;
        private WorldAmbientDustView activeGodRayDust;

        public WorldAtmosphereSystem(
            VfxVisualConfig config,
            WorldAtmosphereFactory factory,
            PlayerView player)
        {
            this.config = config;
            this.factory = factory;
            this.player = player;
        }

        public int ActiveViewCount =>
            ActiveLayerCount(activeDust)
            + ActiveLayerCount(activeFloorHaze)
            + ActiveLayerCount(activeGodRayDust);
        public WorldAmbientDustView ActiveDust => activeDust;
        public WorldAmbientDustView ActiveFloorHaze => activeFloorHaze;
        public WorldAmbientDustView ActiveGodRayDust => activeGodRayDust;

        public void Start()
        {
            Tick();
        }

        public void Tick()
        {
            if (config == null || !config.enabled || factory == null || player == null)
            {
                DeactivateAll();
                return;
            }

            Vector3 position = player.transform.position;
            UpdateLayer(ref activeDust, WorldAtmosphereLayerId.AmbientDust, "World Ambient Dust", position);
            UpdateLayer(ref activeFloorHaze, WorldAtmosphereLayerId.FloorHaze, "World Floor Haze", position);
            UpdateLayer(ref activeGodRayDust, WorldAtmosphereLayerId.GodRayDust, "World God Ray Dust", position);
        }

        public void ForEachActiveView(Action<Component> visitor)
        {
            if (visitor == null)
            {
                return;
            }

            VisitActive(activeDust, visitor);
            VisitActive(activeFloorHaze, visitor);
            VisitActive(activeGodRayDust, visitor);
        }

        public void Dispose()
        {
            DestroyLayer(ref activeDust);
            DestroyLayer(ref activeFloorHaze);
            DestroyLayer(ref activeGodRayDust);
        }

        private void UpdateLayer(
            ref WorldAmbientDustView view,
            WorldAtmosphereLayerId layer,
            string name,
            Vector3 position)
        {
            if (!WorldAmbientDustView.IsLayerEnabled(config, layer))
            {
                view?.Deactivate();
                return;
            }

            view ??= factory.CreateDust(config, layer, name);
            view?.Show(config, position, layer);
        }

        private void DeactivateAll()
        {
            activeDust?.Deactivate();
            activeFloorHaze?.Deactivate();
            activeGodRayDust?.Deactivate();
        }

        private void DestroyLayer(ref WorldAmbientDustView view)
        {
            if (view == null)
            {
                return;
            }

            factory.DestroyDust(view);
            view = null;
        }

        private static int ActiveLayerCount(WorldAmbientDustView view)
        {
            return view != null && view.IsActive ? 1 : 0;
        }

        private static void VisitActive(WorldAmbientDustView view, Action<Component> visitor)
        {
            if (view == null || !view.IsActive)
            {
                return;
            }

            visitor(view);
        }
    }
}

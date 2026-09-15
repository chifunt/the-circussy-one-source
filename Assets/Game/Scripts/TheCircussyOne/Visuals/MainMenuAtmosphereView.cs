using TheCircussyOne.Config;
using TheCircussyOne.Runtime;
using UnityEngine;

namespace TheCircussyOne.Visuals
{
    public sealed class MainMenuAtmosphereView : MonoBehaviour
    {
        [SerializeField] private VfxVisualConfig config;
        [SerializeField] private WorldAmbientDustView ambientDust;
        [SerializeField] private WorldAmbientDustView floorHaze;
        [SerializeField] private WorldAmbientDustView godRayDust;
        [SerializeField] private bool atmosphereEnabled = true;
        [SerializeField] private bool ambientDustEnabled = true;
        [SerializeField] private bool floorHazeEnabled = true;
        [SerializeField] private bool godRayDustEnabled = true;
        [SerializeField] private Vector3 anchorWorldPosition;

        public void Configure(
            VfxVisualConfig vfxConfig,
            WorldAmbientDustView ambientDustView,
            WorldAmbientDustView floorHazeView,
            WorldAmbientDustView godRayDustView,
            bool atmosphereEnabled = true,
            bool ambientDustEnabled = true,
            bool floorHazeEnabled = true,
            bool godRayDustEnabled = true,
            Vector3 anchorPosition = default)
        {
            config = vfxConfig;
            ambientDust = ambientDustView;
            floorHaze = floorHazeView;
            godRayDust = godRayDustView;
            this.atmosphereEnabled = atmosphereEnabled;
            this.ambientDustEnabled = ambientDustEnabled;
            this.floorHazeEnabled = floorHazeEnabled;
            this.godRayDustEnabled = godRayDustEnabled;
            anchorWorldPosition = anchorPosition;
            Refresh();
        }

        private void OnEnable()
        {
            Refresh();
        }

        private void Start()
        {
            Refresh();
        }

        public void Refresh()
        {
            bool enabled = atmosphereEnabled && config != null && config.enabled;
            ApplyLayer(ambientDust, WorldAtmosphereLayerId.AmbientDust, enabled && ambientDustEnabled);
            ApplyLayer(floorHaze, WorldAtmosphereLayerId.FloorHaze, enabled && floorHazeEnabled);
            ApplyLayer(godRayDust, WorldAtmosphereLayerId.GodRayDust, enabled && godRayDustEnabled);
        }

        private void ApplyLayer(WorldAmbientDustView view, WorldAtmosphereLayerId layer, bool enabled)
        {
            if (view == null)
            {
                return;
            }

            if (!enabled || !WorldAmbientDustView.IsLayerEnabled(config, layer))
            {
                view.Deactivate();
                return;
            }

            view.Show(config, anchorWorldPosition, layer);
        }
    }
}

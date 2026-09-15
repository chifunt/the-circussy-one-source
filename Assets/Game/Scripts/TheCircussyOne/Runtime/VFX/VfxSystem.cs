using UnityEngine;
using VContainer.Unity;
using TheCircussyOne.Config;

namespace TheCircussyOne.Runtime
{
    public sealed class VfxSystem : ITickable, IVfxSpawner
    {
        private readonly VfxVisualConfig config;
        private readonly VfxEffectFactory factory;

        public VfxSystem(VfxVisualConfig config, VfxEffectFactory factory)
        {
            this.config = config;
            this.factory = factory;
        }

        public int ActiveCount => factory?.Active.Count ?? 0;

        public void Show(VfxEffectId effectId, Vector3 position)
        {
            Show(effectId, position, Vector3.forward);
        }

        public void Show(VfxEffectId effectId, Vector3 position, Vector3 direction)
        {
            if (config == null || !config.enabled || factory == null)
            {
                return;
            }

            VfxOneShotSettings settings = config.SettingsFor(effectId);
            if (settings == null || !settings.enabled)
            {
                return;
            }

            Quaternion rotation = direction.sqrMagnitude > 0.001f
                ? Quaternion.LookRotation(direction.normalized, Vector3.up)
                : Quaternion.identity;
            factory.Spawn(effectId, position, rotation);
        }

        public void Tick()
        {
            if (factory == null)
            {
                return;
            }

            for (int i = factory.Active.Count - 1; i >= 0; i--)
            {
                VfxEffectRuntime effect = factory.Active[i];
                if (effect == null || effect.View == null || !effect.View.IsActive || !effect.View.IsAlive())
                {
                    factory.Release(effect);
                }
            }
        }
    }
}

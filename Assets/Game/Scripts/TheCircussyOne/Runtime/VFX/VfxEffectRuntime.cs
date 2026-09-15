using TheCircussyOne.Visuals;

namespace TheCircussyOne.Runtime
{
    public sealed class VfxEffectRuntime
    {
        public VfxEffectRuntime(VfxEffectId effectId, ParticleEffectView view)
        {
            EffectId = effectId;
            View = view;
        }

        public VfxEffectId EffectId { get; }
        public ParticleEffectView View { get; }
    }
}

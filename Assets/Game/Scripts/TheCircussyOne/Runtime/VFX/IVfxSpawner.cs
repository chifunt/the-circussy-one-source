using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public interface IVfxSpawner
    {
        void Show(VfxEffectId effectId, Vector3 position);
        void Show(VfxEffectId effectId, Vector3 position, Vector3 direction);
    }
}

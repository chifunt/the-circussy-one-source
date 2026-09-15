using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public interface IProjectileExplosionSpawner
    {
        void Show(Vector3 position, float radius, Vector3 direction);
    }
}

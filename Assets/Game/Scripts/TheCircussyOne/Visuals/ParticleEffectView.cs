using UnityEngine;
using TheCircussyOne.Runtime;

namespace TheCircussyOne.Visuals
{
    public sealed class ParticleEffectView : MonoBehaviour
    {
        [SerializeField] private VfxEffectId effectId;
        [SerializeField] private ParticleSystem primarySystem;

        public VfxEffectId EffectId => effectId;
        public bool IsActive => gameObject.activeInHierarchy;

        private void Awake()
        {
            ResolveComponents();
        }

        public void Configure(VfxEffectId id)
        {
            effectId = id;
            name = "Particle Effect View";
            ResolveComponents();
        }

        public void PlayAt(Vector3 position, Quaternion rotation)
        {
            ResolveComponents();
            transform.SetPositionAndRotation(position, rotation);
            gameObject.SetActive(true);

            ParticleSystem[] systems = GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < systems.Length; i++)
            {
                systems[i].gameObject.SetActive(true);
                systems[i].Clear(withChildren: true);
                systems[i].Play(withChildren: true);
            }
        }

        public bool IsAlive()
        {
            ResolveComponents();
            if (primarySystem == null)
            {
                return false;
            }

            return primarySystem.IsAlive(withChildren: true);
        }

        public void Deactivate()
        {
            ParticleSystem[] systems = GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < systems.Length; i++)
            {
                systems[i].Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            gameObject.SetActive(false);
        }

        private void ResolveComponents()
        {
            if (primarySystem == null)
            {
                primarySystem = GetComponentInChildren<ParticleSystem>(includeInactive: true);
            }
        }
    }
}

using PrimeTween;
using UnityEngine;

namespace TheCircussyOne.Runtime
{
    public static class PrimeTweenRuntimeBootstrap
    {
        public const int TweenCapacity = 4096;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        public static void ConfigureCapacity()
        {
            PrimeTweenConfig.SetTweensCapacity(TweenCapacity);
        }
    }
}

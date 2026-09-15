using System;
using UnityEngine;

namespace TheCircussyOne.Rules
{
    public static class DeterministicSeed
    {
        private const double UintUnit = 1.0 / 4294967296.0;

        public static int Mix(int seed)
        {
            unchecked
            {
                uint state = (uint)seed;
                state ^= state >> 16;
                state *= 2246822519u;
                state ^= state >> 13;
                state *= 3266489917u;
                state ^= state >> 16;
                return (int)state;
            }
        }

        public static int Combine(params int[] values)
        {
            unchecked
            {
                uint state = 2166136261u;
                if (values != null)
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        state ^= (uint)Mix(values[i]);
                        state *= 16777619u;
                    }
                }

                return Mix((int)state);
            }
        }

        public static int ContentPosition(string contentId, Vector3 position)
        {
            return Combine(
                StringHash(contentId),
                Mathf.RoundToInt(position.x * 100f),
                Mathf.RoundToInt(position.y * 100f),
                Mathf.RoundToInt(position.z * 100f));
        }

        public static int ContentPosition(int runSeed, string contentId, Vector3 position)
        {
            return Combine(runSeed, ContentPosition(contentId, position));
        }

        public static int StringHash(string value)
        {
            unchecked
            {
                uint hash = 2166136261u;
                if (!string.IsNullOrEmpty(value))
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        hash ^= value[i];
                        hash *= 16777619u;
                    }
                }

                return (int)hash;
            }
        }

        public static float ToFloat01(int seed)
        {
            unchecked
            {
                uint mixed = (uint)Mix(seed);
                return (float)(mixed * UintUnit);
            }
        }

        public static float ToSignedFloat(int seed)
        {
            return ToFloat01(seed) * 2f - 1f;
        }

        public static float ToAngleRadians(int seed)
        {
            return ToFloat01(seed) * Mathf.PI * 2f;
        }

        public static int NewRuntimeSeed(int runIndex)
        {
            unchecked
            {
                int timeSeed = Environment.TickCount;
                long ticks = DateTime.UtcNow.Ticks;
                byte[] guidBytes = Guid.NewGuid().ToByteArray();
                int guidSeed = BitConverter.ToInt32(guidBytes, 0)
                    ^ BitConverter.ToInt32(guidBytes, 4)
                    ^ BitConverter.ToInt32(guidBytes, 8)
                    ^ BitConverter.ToInt32(guidBytes, 12);
                return Combine(timeSeed, (int)ticks, (int)(ticks >> 32), guidSeed, runIndex);
            }
        }
    }
}

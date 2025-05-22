using Unity.Collections;
using UnityEngine;

namespace Game.MapGraph
{
    public static class NativeListExtensions
    {
        public static void Shuffle<T>(this NativeList<T> list) where T : unmanaged
        {
            for (int i = list.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
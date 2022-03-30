using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Game
{
    public static class ListExt
    {
        public static T RemoveBySwap<T>(this List<T> list, int index)
        {
            var t = list[index];
            if (index != list.Count - 1)
            {
                list[index] = list[list.Count - 1];
            }
            list.RemoveAt(list.Count - 1);
            return t;
        }

        public static void RemoveBySwap<T>(this List<T> list, T item)
        {
            int index = list.IndexOf(item);
            RemoveBySwap(list, index);
        }

        public static void RemoveBySwap<T>(this List<T> list, Predicate<T> predicate)
        {
            int index = list.FindIndex(predicate);
            RemoveBySwap(list, index);
        }

        public static T RemoveBySwap<T>(this T[] array, int index, int len)
        {
#if UNITY_EDITOR || ENABLE_LOG
            if (index < 0 || index >= len || array.Length < len)
            {
                throw new IndexOutOfRangeException("remove by swap");
            }
#endif
            if (len == 0) return default;

            T t = array[index];
            if (len == 1 || index == len - 1)
            {
                array[index] = default;
            }
            else
            {
                array[index] = array[len - 1];
            }
            return t;
        }
    }
}

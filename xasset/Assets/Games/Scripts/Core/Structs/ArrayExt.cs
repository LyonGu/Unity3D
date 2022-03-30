//Create By Zhangjinhui
//***********************

using System;

namespace DXGame
{
    public static class ArrayExt
    {
        public static void Move(this Array array, int fromIndex, int toIndex, int len)
        {
            int flen = fromIndex + len;
            int tlen = toIndex + len;
            if (array.Length < flen || array.Length < tlen)
                throw new IndexOutOfRangeException(
                    $"array move exception: array len = {array.Length},from={flen} to={tlen}");

            Array.Copy(array, fromIndex, array, toIndex, len);
        }
    }
}

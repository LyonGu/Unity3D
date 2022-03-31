
namespace Game
{
    using UnityEngine;

    /// <summary>
    /// The utility tool for screen.
    /// </summary>
    public static class ScreenUtil
    {
        /// <summary>
        /// Check and limit the screen resolution.
        /// </summary>
        public static void CheckAndLimitScreen(int limit)
        {
            if (Screen.width > Screen.height)
            {
                if (Screen.height >= limit)
                {
                    var radio = (float)Screen.width / Screen.height;
                    Screen.SetResolution((int)(limit * radio), limit, true);
                }
            }
            else
            {
                if (Screen.width >= limit)
                {
                    var radio = (float)Screen.width / Screen.height;
                    Screen.SetResolution(limit, (int)(limit * radio), true);
                }
            }
        }
    }
}

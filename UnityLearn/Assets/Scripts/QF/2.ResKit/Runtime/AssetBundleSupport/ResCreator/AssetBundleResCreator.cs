using UnityEngine;

namespace QFramework
{
    public class AssetBundleResCreator : IResCreator
    {
        public bool Match(ResSearchKeys resSearchKeys)
        {
            bool isMatch = resSearchKeys.AssetType == typeof(AssetBundle);
            return isMatch;
        }

        public IRes Create(ResSearchKeys resSearchKeys)
        {
            return AssetBundleRes.Allocate(resSearchKeys.AssetName);
        }
    }
}
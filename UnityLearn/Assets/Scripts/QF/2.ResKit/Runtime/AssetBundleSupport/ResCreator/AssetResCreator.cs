namespace QFramework
{
    public class AssetResCreator : IResCreator
    {
        public bool Match(ResSearchKeys resSearchKeys)
        {
            var assetData =  AssetBundleSettings.AssetBundleConfigFile.GetAssetData(resSearchKeys); // 通过去一个ab的配置里找，记录了工程里使用的所有的ab配置

            if (assetData != null)
            {
                return assetData.AssetType == ResLoadType.ABAsset;
            }

            return false;
        }

        public IRes Create(ResSearchKeys resSearchKeys)
        {
            return AssetRes.Allocate(resSearchKeys.AssetName, resSearchKeys.OwnerBundle, resSearchKeys.AssetType);
        }
    }
}
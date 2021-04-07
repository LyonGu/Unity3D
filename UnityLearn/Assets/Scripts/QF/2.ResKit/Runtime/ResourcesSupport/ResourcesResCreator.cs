namespace QFramework
{
    public class ResourcesResCreator : IResCreator
    {
        public bool Match(ResSearchKeys resSearchKeys)
        {

            bool isMatch = resSearchKeys.AssetName.StartsWith("resources/") ||
                   resSearchKeys.AssetName.StartsWith("resources://");
            return isMatch;
        }

        public IRes Create(ResSearchKeys resSearchKeys)
        {
            var resourcesRes = ResourcesRes.Allocate(resSearchKeys.AssetName,
                resSearchKeys.AssetName.StartsWith("resources://")
                    ? InternalResNamePrefixType.Url
                    : InternalResNamePrefixType.Folder);
            resourcesRes.AssetType = resSearchKeys.AssetType;
            return resourcesRes;
        }
    }
}
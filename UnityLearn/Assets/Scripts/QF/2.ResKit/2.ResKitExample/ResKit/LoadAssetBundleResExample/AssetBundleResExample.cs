using UnityEngine;
using UnityEngine.UI;

namespace QFramework.Example
{
	public class AssetBundleResExample : MonoBehaviour
	{
		private ResLoader mResLoader = ResLoader.Allocate();
        private ResLoader mResLoaderAsync = ResLoader.Allocate();

        public RawImage RawImage;

		private void Awake()
		{
			ResMgr.Init();
		}

		// Use this for initialization
		void Start()
		{
			RawImage rawImage = transform.Find("RawImage").GetComponent<RawImage>();

            // resource资源加载
            //RawImage.texture = mResLoader.LoadSync<Texture2D>("TestImage");

            // ab资源加载通过下边方式也一样
            //RawImage.texture = mResLoader.LoadSync<Texture2D>("testimage_png","TestImage");

            //mResLoader.LoadAsync();
            //mResLoader.LoadAsync

            //mResLoader.LoadSync()
            mResLoader.Add2Load<Texture2D>("TestImage", (succeed, res) =>
            {
                Debug.Log($"succeed========={succeed}");
                if (succeed)
                {
                    Texture2D texture = res.Asset as Texture2D;
                    RawImage.texture = texture;
                }
            });
            mResLoader.LoadAsync(()=> {
                Debug.Log("资源加载完毕=======");
            });

        }

		private void OnDestroy()
		{
			mResLoader.Recycle2Cache();
			mResLoader = null;
		}
	}
}
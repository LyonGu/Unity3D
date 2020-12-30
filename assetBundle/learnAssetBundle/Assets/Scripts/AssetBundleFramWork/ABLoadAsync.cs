using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ABLoadAsync : MonoBehaviour
{
    // Start is called before the first frame update

    public RawImage rawimage;
    
    void Start()
    {
        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, "Windows/scene_1/textures.ab"));
        request.completed -= abLoadDone;
        request.completed += abLoadDone;
    }

    void abLoadDone(AsyncOperation t)
    {
        if (t.isDone)
        {
            AssetBundleCreateRequest abRequset = t as AssetBundleCreateRequest;
            var assetBundle = abRequset.assetBundle;

            Texture2D tex = assetBundle.LoadAsset<Texture2D>("WhileFloor.jpg"); // 只使用资源名就行
            rawimage.texture = tex;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

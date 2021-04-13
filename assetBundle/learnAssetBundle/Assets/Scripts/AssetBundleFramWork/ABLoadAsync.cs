using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ABLoadAsync : MonoBehaviour
{
    // Start is called before the first frame update

    public RawImage rawimage;

    public Image image;
    private AssetBundleCreateRequest _request;
    void Start()
    {
        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, "Windows/scene_1/textures.ab"));
        request.completed -= abTextureLoadDone;
        request.completed += abTextureLoadDone;

        _request = request;

        AssetBundleCreateRequest request1 = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, "Windows/scene_1/atlas.ab"));
        request1.completed -= abSpriteLoadDone;
        request1.completed += abSpriteLoadDone;
    }

    void abTextureLoadDone(AsyncOperation t)
    {
        if (t.isDone)
        {
            AssetBundleCreateRequest abRequset = t as AssetBundleCreateRequest;
            var assetBundle = abRequset.assetBundle;

            Texture2D tex = assetBundle.LoadAsset<Texture2D>("WhileFloor"); // 只使用资源名就行
            rawimage.texture = tex;
        }
       
    }

    void abSpriteLoadDone(AsyncOperation t)
    {
        if (t.isDone)
        {
            AssetBundleCreateRequest abRequset = t as AssetBundleCreateRequest;
            var assetBundle = abRequset.assetBundle;

            Sprite sp = assetBundle.LoadAsset<Sprite>("FloorAl"); // 只使用资源名就行
            image.sprite = sp;
        }
    }

    bool isUpdate = true;
    // Update is called once per frame
    void Update()
    {
        if (!isUpdate) return;
        Debug.Log($"progress ===== {_request.progress}");
        if (_request.isDone)
        {
            isUpdate = false;
        }
    }
}

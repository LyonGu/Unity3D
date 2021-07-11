using UnityEngine;
using UnityEditor;

/*
OnPreprocessTexture：在导入纹理贴图之前调用
OnPreprocessModel：在导入模型之前调用 GameObject
OnPreprocessAudio：在导入音频之前调用

OnPostprocessTexture：在导入纹理贴图之后调用
OnPostprocessModel：在导入模型之后调用
OnPostprocessAudio：在导入音频之后调用


OnPostprocessAllAssets：所有资源的导入，删除，移动操作都会调用该方法
 

如果有多个类重写了OnPreprocessTexture方法，会被调用多次
 */

public class CustomAssetPostprocessor : AssetPostprocessor
{
    //模型导入之前调用
    public void OnPreprocessModel()
    {
        Debug.Log("OnPreprocessModel=" + this.assetPath);
    }

    public void OnPostprocessModel(GameObject go)
    {
        Debug.Log("OnPostprocessModel=" + go.name);
    }
    //纹理导入之前调用，针对入到的纹理进行设置
    public void OnPreprocessTexture()
    {
        Debug.Log("CustomAssetPostprocessor OnPreProcessTexture=" + this.assetPath);
        TextureImporter importer = this.assetImporter as TextureImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.maxTextureSize = 512;
        importer.mipmapEnabled = false;

    }
    public void OnPostprocessTexture(Texture2D tex)
    {
        Debug.Log("OnPostProcessTexture=" + this.assetPath);
    }


    public void OnPostprocessAudio(AudioClip clip)
    {

    }
    public void OnPreprocessAudio()
    {
        AudioImporter audio = this.assetImporter as AudioImporter;
        //audio. = AudioCompressionFormat.MP3;
    }
    //所有的资源的导入，删除，移动，都会调用此方法，注意，这个方法是static的
    public static void OnPostprocessAllAssets(string[] importedAsset, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        Debug.Log("OnPostprocessAllAssets");
        foreach (string str in importedAsset)
        {
            Debug.Log("importedAsset = " + str);
        }
        foreach (string str in deletedAssets)
        {
            Debug.Log("deletedAssets = " + str);
        }
        foreach (string str in movedAssets)
        {
            Debug.Log("movedAssets = " + str);
        }
        foreach (string str in movedFromAssetPaths)
        {
            Debug.Log("movedFromAssetPaths = " + str);
        }
    }
}

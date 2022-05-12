using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.AssetImporters;

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

/*
    TextureImporter
    AudioImporter
    ModelImporter
    ShaderImporter
    VideoClipImporter
    MonoImporter
    SketchUpImporter
    PluginImporter
    SpeedTreeImporter
 */

/*
OnAssignMaterialModel	==》Feeds a source material.  模型指定材质
OnPostprocessAllAssets	==》This is called after importing of any number of assets is complete (when the Assets progress bar has reached the end).
OnPostprocessAnimation	==》This function is called when an AnimationClip has finished importing.
OnPostprocessAssetbundleNameChanged	==》Handler called when asset is assigned to a different asset bundle.
OnPostprocessAudio	==》Add this function to a subclass to get a notification when an audio clip has completed importing.
OnPostprocessCubemap	==》Add this function to a subclass to get a notification just before a cubemap texture has completed importing.
OnPostprocessGameObjectWithAnimatedUserProperties	==》This function is called when the animation curves for a custom property are finished importing.
OnPostprocessGameObjectWithUserProperties	==》Gets called for each GameObject that had at least one user property attached to it in the imported file.
OnPostprocessMaterial	==》Add this function to a subclass to get a notification when a Material asset has completed importing.
OnPostprocessMeshHierarchy	==》This function is called when a new transform hierarchy has finished importing.
OnPostprocessModel	==》Add this function to a subclass to get a notification when a model has completed importing.
OnPostprocessPrefab	==》 Gets a notification when a Prefab completes importing.
OnPostprocessSpeedTree	==》Add this function to a subclass to get a notification when a SpeedTree asset has completed importing.
OnPostprocessSprites	==》Add this function to a subclass to get a notification when an texture of sprite(s) has completed importing.
OnPostprocessTexture	==》Add this function to a subclass to get a notification when a texture2D has completed importing just before Unity compresses it.
OnPostprocessTexture2DArray	==》Add this function to a subclass to get a notification when a texture2DArray has completed importing just before Unity compresses it.
OnPostprocessTexture3D	==》Add this function to a subclass to get a notification when a texture3D has completed importing just before Unity compresses it.
OnPreprocessAnimation	==》Add this function to a subclass to get a notification just before animation from a model (.fbx, .mb file etc.) is imported.
OnPreprocessAsset	==》Add this function to a subclass to get a notification just before any Asset is imported.
OnPreprocessAudio	==》Add this function to a subclass to get a notification just before an audio clip is being imported.
OnPreprocessCameraDescription	==》Add this function to a subclass to recieve a notification when a camera is imported from a Model Importer.
OnPreprocessLightDescription	==》Add this function to a subclass to recieve a notification when a light is imported from a Model Importer.
OnPreprocessMaterialDescription	==》Add this function to a subclass to recieve a notification when a material is imported from a Model Importer.
OnPreprocessModel	==》Add this function to a subclass to get a notification just before a model (.fbx, .mb file etc.) is imported.
OnPreprocessSpeedTree	==》Add this function to a subclass to get a notification just before a SpeedTree asset (.spm file) is imported.
OnPreprocessTexture	==》Add this function to a subclass to get a notification just before the texture importer is run.
 */

/*
    OnPreprocessAnimation
    OnPostprocessAnimation
    
    OnPreprocessAsset==》 所有资源导入都会调用这个方法

    OnPreprocessTexture
    OnPostprocessTexture

    OnPreprocessModel
    OnPostprocessModel

    OnPostprocessPrefab

    OnPreprocessAudio
    OnPostprocessAudio
    
    OnPreprocessMaterialDescription
    OnPostprocessMaterial

    OnPostprocessSprites
    
 */
public class CustomAssetPostprocessor : AssetPostprocessor
{


    //Unity 21才支持 19不支持
    public void OnPostprocessPrefab(GameObject g)
    {
        Debug.Log("OnPostprocessPrefab AssetPath=" + this.assetPath);
        Debug.Log("OnPostprocessPrefab GamgObjectName=" + g.name);
    }

    public void OnPreprocessAsset()
    {
        Debug.Log("[OnPreprocessAsset] AssetPath=" + this.assetPath);
        if (assetImporter.importSettingsMissing)
        {
            //ModelImporter modelImporter = assetImporter as ModelImporter;
            //if (modelImporter != null)
            //{
            //    if (!assetPath.Contains("@"))
            //        modelImporter.importAnimation = false;
            //    modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
            //}
        }
    }
    //Unity only calls this function if you set ModelImporter.materialImportMode to ModelImporterMaterialImportMode.ImportViaMaterialDescription
    public void OnPreprocessMaterialDescription(MaterialDescription description, Material material, AnimationClip[] materialAnimation)
    {
        var shader = Shader.Find("Standard");
        if (shader == null)
            return;
        material.shader = shader;

        List<string> props = new List<string>();
        // list the properties of type Vector4 :
        description.GetVector4PropertyNames(props);
        Debug.Log(props);

        // Read a texture property from the material description.
        TexturePropertyDescription textureProperty;
        if (description.TryGetProperty("DiffuseColor", out textureProperty))
        {
            // Assign the texture to the material.
            material.SetTexture("_MainTex", textureProperty.texture);
        }
    }


    //这个调用不到
    public void OnPostprocessMaterial(Material material)
    {
        Debug.Log("OnPostprocessMaterial AssetPath=" + this.assetPath);
        Debug.Log("OnPostprocessMaterial MaterialName=" + material.name);
    }

    public void OnPreprocessAnimation()
    {
        Debug.Log("OnPreprocessAnimation AssetPath=" + this.assetPath);
        ModelImporter modelImporter = assetImporter as ModelImporter;
        modelImporter.clipAnimations = modelImporter.defaultClipAnimations;

        //modelImporter.SaveAndReimport();
    }

    public void OnPostprocessAnimation(GameObject root, AnimationClip clip)
    {
        Debug.Log("OnPostprocessAnimation AssetPath=" + this.assetPath);
        Debug.Log($"OnPostprocessAnimation GameObject AnimationClip {root.name} {clip.name}");

    }
    public void OnPostprocessSprites(Texture2D texture, Sprite[] sprites)
    {
        Debug.Log("OnPostprocessSprites AssetPath=" + this.assetPath);
        for (int i = 0; i < sprites.Length; i++)
        {
            Sprite sp = sprites[i];
            Debug.Log("OnPostprocessSprites SpriteName=" + sp.name);
        }
    }
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
        Debug.Log("CustomAssetPostprocessor OnPostprocessAudio=" + this.assetPath);
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

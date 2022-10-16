using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class ImportPicture : AssetPostprocessor
{

    //被4整除
    bool IsDivisibleOf4(TextureImporter importer)
    {
        (int width, int height) = GetTextureImporterSize(importer);
        return (width % 4 == 0 && height % 4 == 0);
    }

    //2的整数次幂
    bool IsPowerOfTwo(TextureImporter importer)
    {
        (int width, int height) = GetTextureImporterSize(importer);
        return (width == height) && (width > 0) && ((width & (width - 1)) == 0);
    }

    //贴图不存在、meta文件不存在、图片尺寸发生修改需要重新导入
    bool IsFirstImport(TextureImporter importer)
    {
        (int width, int height) = GetTextureImporterSize(importer);
        Texture tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        bool hasMeta = File.Exists(AssetDatabase.GetAssetPathFromTextMetaFilePath(assetPath));
        return tex == null || !hasMeta || (tex.width != width && tex.height != height);
    }

    //获取导入图片的宽高
    (int, int) GetTextureImporterSize(TextureImporter importer)
    {
        if (importer != null)
        {
            object[] args = new object[2];
            MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
            mi.Invoke(importer, args);
            return ((int)args[0], (int)args[1]);
        }
        return (0, 0);
    }

    //可以写一个配置ScriptObject
    List<string> whiteList = new List<string>
    {

    };
    void OnPreprocessTexture()
    {
        if (whiteList.Contains(assetPath))
            return;
        bool isBackGround = assetPath.IndexOf("background") != -1;
        TextureImporter importer = assetImporter as TextureImporter;

        if (importer.isReadable)
        {
            importer.isReadable = false;
        }

        if (importer.mipmapEnabled)
        {
            importer.mipmapEnabled = false;
        }

        //是否要修改原图大小：TODO
        importer.npotScale = TextureImporterNPOTScale.ToNearest;

        bool isHaveAplha = importer.DoesSourceTextureHaveAlpha();

        //Android 
        TextureImporterPlatformSettings androidSet = importer.GetPlatformTextureSettings("Android");
        androidSet.overridden = true;
        androidSet.allowsAlphaSplitting = false;
        if (isHaveAplha)
        {
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            //使用ETC2格式:要求长宽是4的倍数
            if (IsDivisibleOf4(importer))
            {
                importer.npotScale = TextureImporterNPOTScale.None;
            }
            androidSet.maxTextureSize = isBackGround ? 1024 : 512;
            androidSet.format = TextureImporterFormat.ETC2_RGBA8;
        }
        else
        {
            //使用ETC1
            if (IsPowerOfTwo(importer))
            {
                importer.npotScale = TextureImporterNPOTScale.None;
            }
            androidSet.maxTextureSize = isBackGround ? 1024 : 512;
            androidSet.format = TextureImporterFormat.ETC_RGB4;
        }
        importer.SetPlatformTextureSettings(androidSet);


        //Ios 
        TextureImporterPlatformSettings iosSet = importer.GetPlatformTextureSettings("iPhone");
        iosSet.overridden = true;
        iosSet.maxTextureSize = isBackGround ? 1024 : 512;
        iosSet.format = isBackGround ? TextureImporterFormat.ASTC_4x4 : TextureImporterFormat.ASTC_6x6;
        importer.SetPlatformTextureSettings(iosSet);

        Debug.Log($"ImportPicture Assets ==={assetPath} ||  isHaveAplha = {isHaveAplha}");


        


    }
}

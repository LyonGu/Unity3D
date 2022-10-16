using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class ImportPicture : AssetPostprocessor
{

    //��4����
    bool IsDivisibleOf4(TextureImporter importer)
    {
        (int width, int height) = GetTextureImporterSize(importer);
        return (width % 4 == 0 && height % 4 == 0);
    }

    //2����������
    bool IsPowerOfTwo(TextureImporter importer)
    {
        (int width, int height) = GetTextureImporterSize(importer);
        return (width == height) && (width > 0) && ((width & (width - 1)) == 0);
    }

    //��ͼ�����ڡ�meta�ļ������ڡ�ͼƬ�ߴ緢���޸���Ҫ���µ���
    bool IsFirstImport(TextureImporter importer)
    {
        (int width, int height) = GetTextureImporterSize(importer);
        Texture tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        bool hasMeta = File.Exists(AssetDatabase.GetAssetPathFromTextMetaFilePath(assetPath));
        return tex == null || !hasMeta || (tex.width != width && tex.height != height);
    }

    //��ȡ����ͼƬ�Ŀ��
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

     public void ReadTextureSize(string texturePath, out int width, out int height, out int diskSize)
    {
        var texture2D = new Texture2D(1, 1);

        byte[] bytes = File.ReadAllBytes(texturePath);
        texture2D.LoadImage(bytes);

        width = texture2D.width;
        height = texture2D.height;
        diskSize = bytes.Length;

        UnityEngine.Object.DestroyImmediate(texture2D);
    }

    bool CheckSizeLimit(TextureImporter importer)
    {
        (int width, int height) = GetTextureImporterSize(importer);
        if (width > 2048 || height > 2048)
            return false;
        return true;
    }

    void RemoveCurFile(string log)
    {
        string filePath = Application.dataPath.Replace("\\", "/") + assetPath.Replace("Assets", "");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.LogError(log);
            return;
        }

    }

    void OnPreprocessTexture()
    {

        ImportAssetsWhiteList cfg = AssetDatabase.LoadAssetAtPath<ImportAssetsWhiteList>("Assets/Textures/ImportAssetsWhiteList.asset");
        if (cfg != null && cfg.importWhiteList != null)
        {
            if (cfg.importWhiteList.Contains(assetPath))
                return;
        }
        bool isBackGround = assetPath.IndexOf("background") != -1;
        TextureImporter importer = assetImporter as TextureImporter;

        //�ߴ����Ʋ�����
        if (!isBackGround && !CheckSizeLimit(importer))
        {
            RemoveCurFile("�ߴ糬����С���� 2048 X 2048, ǿ��ɾ���ļ�����������");
            return;
        }

        if (importer.isReadable)
        {
            importer.isReadable = false;
        }

        if (importer.mipmapEnabled)
        {
            importer.mipmapEnabled = false;
        }

        if (importer.streamingMipmaps)
        {
            importer.streamingMipmaps = false;
        }

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
            //ʹ��ETC2��ʽ:Ҫ�󳤿���4�ı���
            if (IsDivisibleOf4(importer))
            {
                importer.npotScale = TextureImporterNPOTScale.None;
            }
            androidSet.maxTextureSize = isBackGround ? 1024 : 512;
            androidSet.format = TextureImporterFormat.ETC2_RGBA8;
        }
        else
        {
            //ʹ��ETC1
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

//    public void OnPostprocessTexture(Texture2D tex)
//    {
//        Debug.Log($"OnPostProcessTexture={assetPath}  Size == {tex.width} X {tex.height}"); //����֮������Ĵ�С

        
        
//    }
}

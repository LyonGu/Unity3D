using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FakeReflection))]
public class FakeReflectionEditor : Editor
{
    private int reflectionRenderID = 0;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        FakeReflection fakeReflection = target as FakeReflection;

        SerializedProperty centerSP = serializedObject.FindProperty("center");
        EditorGUILayout.PropertyField(centerSP);

        SerializedProperty sizeSP = serializedObject.FindProperty("size");
        EditorGUILayout.PropertyField(sizeSP);

        SerializedProperty sizeScaleSp = serializedObject.FindProperty("sizeScale");
        EditorGUILayout.PropertyField(sizeScaleSp);

        SerializedProperty twoSizeSp = serializedObject.FindProperty("twoSide");
        EditorGUILayout.PropertyField(twoSizeSp);

        SerializedProperty cubemapSizeSP = serializedObject.FindProperty("cubemapSize");
        EditorGUILayout.PropertyField(cubemapSizeSP);

        SerializedProperty forwardAxisSP = serializedObject.FindProperty("forwardAxis");
        EditorGUILayout.PropertyField(forwardAxisSP);

        SerializedProperty cullingMaskSP = serializedObject.FindProperty("cullingMask");
        EditorGUILayout.PropertyField(cullingMaskSP);

        SerializedProperty isLocalSP = serializedObject.FindProperty("isLocal");
        EditorGUILayout.PropertyField(isLocalSP);

        SerializedProperty innerSimulationSP = serializedObject.FindProperty("innerSimulation");
        EditorGUILayout.PropertyField(innerSimulationSP);

        SerializedProperty roughnessSP = serializedObject.FindProperty("roughness");
        EditorGUILayout.PropertyField(roughnessSP);

        SerializedProperty convolutionTypeSP = serializedObject.FindProperty("convolutionType");
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(convolutionTypeSP);
        if(EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            if (fakeReflection.cubemap != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(fakeReflection.cubemap);
                TextureImporter ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                TextureImporterSettings tis = new TextureImporterSettings();
                ti.ReadTextureSettings(tis);
                tis.cubemapConvolution = (TextureImporterCubemapConvolution)convolutionTypeSP.intValue;
                ti.SetTextureSettings(tis);
                AssetDatabase.ImportAsset(assetPath);
            }
        }

        SerializedProperty cubemapSP = serializedObject.FindProperty("cubemap");
        EditorGUILayout.PropertyField(cubemapSP);

        if(GUILayout.Button("Bake Reflection"))
        {
            Cubemap oldCubemap = fakeReflection.cubemap;
            string oldCubemapPath = oldCubemap == null ? string.Empty : new FileInfo(AssetDatabase.GetAssetPath(oldCubemap)).Directory.FullName;
            string saveCubemapPath = EditorUtility.SaveFilePanelInProject("Save Cubemap", string.Empty, "exr", "Save Cubemap", oldCubemapPath);
            if (!string.IsNullOrEmpty(saveCubemapPath))
            {
                GameObject camGo = new GameObject();
                camGo.hideFlags = HideFlags.HideAndDontSave;
                camGo.transform.parent = null;
                camGo.transform.position = fakeReflection.transform.position;

                Camera cam = camGo.AddComponent<Camera>();
                cam.hideFlags = HideFlags.HideAndDontSave;
                cam.cullingMask = cullingMaskSP.intValue;

                int cubemapSize = cubemapSizeSP.intValue;

                Cubemap cubemap = new Cubemap(cubemapSize, TextureFormat.RGBAFloat, true);
                cubemap.hideFlags = HideFlags.HideAndDontSave;
                cam.RenderToCubemap(cubemap);

                Texture2D nx = CreateCubemapFace(cubemap, CubemapFace.NegativeX, saveCubemapPath, cubemapSize);
                Texture2D ny = CreateCubemapFace(cubemap, CubemapFace.NegativeY, saveCubemapPath, cubemapSize);
                Texture2D nz = CreateCubemapFace(cubemap, CubemapFace.NegativeZ, saveCubemapPath, cubemapSize);
                Texture2D px = CreateCubemapFace(cubemap, CubemapFace.PositiveX, saveCubemapPath, cubemapSize);
                Texture2D py = CreateCubemapFace(cubemap, CubemapFace.PositiveY, saveCubemapPath, cubemapSize);
                Texture2D pz = CreateCubemapFace(cubemap, CubemapFace.PositiveZ, saveCubemapPath, cubemapSize);

                Texture2D atlas = new Texture2D(cubemapSize * 6, cubemapSize, TextureFormat.RGBAFloat, true, true);
                atlas.hideFlags = HideFlags.HideAndDontSave;
                PackTexture(atlas, px, 0, cubemapSize);
                PackTexture(atlas, nx, cubemapSize, cubemapSize);
                PackTexture(atlas, py, cubemapSize * 2, cubemapSize);
                PackTexture(atlas, ny, cubemapSize * 3, cubemapSize);
                PackTexture(atlas, pz, cubemapSize * 4, cubemapSize);
                PackTexture(atlas, nz, cubemapSize * 5, cubemapSize);
                byte[] atlasBytes = atlas.EncodeToEXR(Texture2D.EXRFlags.None);
                File.WriteAllBytes(saveCubemapPath, atlasBytes);
                AssetDatabase.ImportAsset(saveCubemapPath);

                TextureImporter atlasImporter = AssetImporter.GetAtPath(saveCubemapPath) as TextureImporter;
                TextureImporterSettings atlasSettings = new TextureImporterSettings();
                atlasImporter.ReadTextureSettings(atlasSettings);
                atlasSettings.textureShape = TextureImporterShape.TextureCube;
                atlasSettings.cubemapConvolution = (TextureImporterCubemapConvolution)convolutionTypeSP.intValue;
                atlasSettings.seamlessCubemap = true;
                atlasImporter.SetTextureSettings(atlasSettings);
                AssetDatabase.ImportAsset(saveCubemapPath);

                cubemapSP.objectReferenceValue = AssetDatabase.LoadMainAssetAtPath(saveCubemapPath);

                AssetDatabase.Refresh();

                DestroyImmediate(camGo);
                DestroyImmediate(cubemap, true);
                DestroyImmediate(nx, true);
                DestroyImmediate(ny, true);
                DestroyImmediate(nz, true);
                DestroyImmediate(px, true);
                DestroyImmediate(py, true);
                DestroyImmediate(pz, true);
                DestroyImmediate(atlas, true);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void PackTexture(Texture2D atlas, Texture2D tex, int offset, int spriteSize)
    {
        atlas.SetPixels(offset, 0, spriteSize, spriteSize, tex.GetPixels());
        atlas.Apply();
    }

    private Texture2D CreateCubemapFace(Cubemap cubemap, CubemapFace face, string path, int cubemapSize)
    {
        Color[] nxc = cubemap.GetPixels(face);
        if (PlayerSettings.colorSpace == ColorSpace.Gamma)
        {
            EncodeGamme(nxc);
        }
        FlipPixels(nxc, cubemapSize);
        Texture2D nxct = new Texture2D(cubemapSize, cubemapSize, TextureFormat.RGBAFloat, true, true);
        nxct.hideFlags = HideFlags.HideAndDontSave;
        nxct.SetPixels(nxc);
        nxct.Apply(true);

        return nxct;
    }

    private Color[] EncodeGamme(Color[] colors)
    {
        float gamma = 2.2f;
        int count = colors.Length;
        for(int i = 0; i < count; ++i)
        {
            Color c = colors[i];
            c.r = Mathf.Pow(c.r, gamma);
            c.g = Mathf.Pow(c.g, gamma);
            c.b = Mathf.Pow(c.b, gamma);
            colors[i] = c;
        }
        return colors;
    }

    private void FlipPixels(Color[] colors, int size)
    {
        Color[] buffer = new Color[size];
        for (int i = 0; i < size / 2; ++i)
        {
            CopyColors(colors, i * size, buffer, 0, size);
            //MirrorColors(buffer, 0, size);
            CopyColors(colors, (size - 1 - i) * size, colors, i * size, size);
            //MirrorColors(colors, i * size, size);
            CopyColors(buffer, 0, colors, (size - 1 - i) * size, size);
        }
    }

    private void CopyColors(Color[] src, int srcIndex, Color[] dest, int destIndex, int length)
    {
        for(int i = 0; i < length; ++i)
        {
            dest[destIndex + i] = src[srcIndex + i];
        }
    }

    private void MirrorColors(Color[] colors, int startIndex, int length)
    {
        for(int i = 0; i < length / 2; ++i)
        {
            Color c = colors[startIndex + i];
            colors[startIndex + i] = colors[startIndex + length - 1 - i];
            colors[startIndex + length - 1 - i] = c;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
/*
 AssetsOnly is used on object properties, and restricts the property to project assets,
 and not scene objects. Use this when you want to ensure an object is from the project, and not from the scene.
     */
public class Odin_ex1 : MonoBehaviour
{
    [Title("Assets only")]
    [AssetsOnly]  //不能从场景里拖拽，只能是project窗口里的
    public List<GameObject> OnlyPrefabs;

    [AssetsOnly]
    public GameObject SomePrefab;

    [AssetsOnly]
    public Material MaterialAsset;

    [AssetsOnly]
    public MeshRenderer SomeMeshRendererOnPrefab;

    [Title("Scene Objects only")]
    [SceneObjectsOnly] //只能从场景里拖拽，不能是project窗口里的
    public List<GameObject> OnlySceneObjects;

    [SceneObjectsOnly]
    public GameObject SomeSceneObject;

    [SceneObjectsOnly]
    public MeshRenderer SomeMeshRenderer;
}

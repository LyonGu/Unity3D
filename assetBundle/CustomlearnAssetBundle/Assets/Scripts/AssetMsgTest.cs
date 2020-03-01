using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetMsgTest : MonoBehaviour {

	public GameObject targetObj;
	public GameObject targetObj1;

	private AssetMsg  _assetMsg;

	void Awake()
	{
		_assetMsg = AssetMsg.GetInstance();
	}
	// Use this for initialization
	void Start () {

		//非GameObject资源 ok
		Texture texture = _assetMsg.LoadAsset<Texture>("AB_Res/Scene_1/Textures/WhileFloor.jpg");
		targetObj.GetComponent<Renderer>().material.mainTexture = texture;

		Material material = _assetMsg.LoadAsset<Material>("AB_Res/Scene_1/Materials/Floor.mat");
		targetObj1.GetComponent<Renderer>().material = material;

		//GameObject资源
		GameObject obj = _assetMsg.LoadAsset<GameObject>("AB_Res/Scene_1/Prefabs/TestCubePrefab.prefab");
		obj.transform.SetParent(targetObj.transform);

	}

	// Update is called once per frame
	void Update () {

	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using libx;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
	public Dropdown dropdown;
	public Image temp;
	private string[] _assets;
	private int _optionIndex;

	List<GameObject> _gos = new List<GameObject> ();
	List<AssetRequest> _requests = new List<AssetRequest> ();

	public void OnLoad ()
	{
		StartCoroutine (LoadAsset ());
	}

	AssetRequest LoadSprite (string path)
	{
		var request = Assets.LoadAsset (path, typeof(Sprite));
		_requests.Add (request);
		return request;
	}

	AssetRequest LoadGameObjectAsync(string path)
	{
		var request = Assets.LoadAssetAsync(path, typeof(GameObject));
		_requests.Add(request);
		return request;
	}

	AssetRequest LoadGameObject(string path)
	{
		var request = Assets.LoadAsset(path, typeof(GameObject));
		_requests.Add(request);
		return request;
	}

	public void OnLoadAll ()
	{ 
		StartCoroutine (LoadAll (_assets.Length));
	}

	IEnumerator LoadAll (int size)
	{
		var count = 0; 
		List<AssetRequest> list = new List<AssetRequest> ();
		for (int i = _optionIndex; i < _assets.Length; i++) {
			var asset = _assets [i];
			var ext = Path.GetExtension (asset);
			if (count >= size) {
				_optionIndex = i; 
				break;
			}
			if (ext.Equals (".png", StringComparison.OrdinalIgnoreCase)) {
				var request = LoadSprite (asset);
				request.completed += OnCompleted;  
				list.Add (request); 
				count++;
			}
		}
		yield return new WaitUntil (() => list.TrueForAll (o => {
			return o.isDone;
		}));
	}

	private void OnCompleted (AssetRequest request)
	{
		if (!string.IsNullOrEmpty (request.error)) {
			request.Release ();
			return;
		}
		var go = Instantiate (temp.gameObject, temp.transform.parent);
		go.SetActive (true);
		go.name = request.asset.name;
		var image = go.GetComponent<Image> ();
		image.sprite = request.asset as Sprite;
		_gos.Add (go);
	}

	private IEnumerator LoadAsset ()
	{
		if (_assets == null || _assets.Length == 0) {
			yield break;
		} 
		var path = _assets [_optionIndex];
		var ext = Path.GetExtension (path);
		if (ext.Equals (".png", StringComparison.OrdinalIgnoreCase)) {
			var request = LoadSprite (path);
			yield return request;
			if (!string.IsNullOrEmpty (request.error)) {
				request.Release ();
				yield break;
			} 
			var go = Instantiate (temp.gameObject, temp.transform.parent);
			go.SetActive (true);
			go.name = request.asset.name;
			var image = go.GetComponent<Image> ();
			image.sprite = request.asset as Sprite; 
			_gos.Add (go);
		}
	}

	public void OnUnload ()
	{
		_optionIndex = 0;
		StartCoroutine (UnloadAssets ());
	}

	private IEnumerator UnloadAssets ()
	{
		foreach (var image in _gos) {
			DestroyImmediate (image);
		}
		_gos.Clear ();
        
		foreach (var request in _requests) {
			request.Release ();
		}

		_requests.Clear ();
		yield return null; 
	}

	private string hotUpatePrefabPath;
	// Use this for initialization
	void Start ()
	{
		dropdown.ClearOptions ();
		_assets = Assets.GetAllAssetPaths ();
		foreach (var item in _assets) {

			var ext = Path.GetExtension(item);
			if (ext.Equals(".prefab", StringComparison.OrdinalIgnoreCase))
			{
				hotUpatePrefabPath = item;
			}
			dropdown.options.Add (new Dropdown.OptionData (item));
		}

		dropdown.onValueChanged.AddListener (OnDropdown);


		if (!string.IsNullOrEmpty(hotUpatePrefabPath))
		{

			//同步加载
			var abRequest = LoadGameObject(hotUpatePrefabPath);
			abRequest.completed += delegate(AssetRequest request) {
				if (!string.IsNullOrEmpty(request.error))
				{
					request.Release();
					return;
				}
				var go = Instantiate(request.asset) as GameObject;
				go.SetActive(true);
				go.name = "HotTestSync";

			};

			//异步加载
			var abRequestAsync = LoadGameObjectAsync(hotUpatePrefabPath);
			abRequestAsync.completed += delegate (AssetRequest request) {
				if (!string.IsNullOrEmpty(request.error))
				{
					request.Release();
					return;
				}
				var go = Instantiate(request.asset) as GameObject;
				go.SetActive(true);
				go.name = "HotTestAsync";
				go.transform.position = new Vector3(2, 0, 0);

			};

			//加载进度
		}

	}

	private void OnDropdown (int index)
	{
		_optionIndex = index;
	}
}
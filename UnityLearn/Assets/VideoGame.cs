
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoGame : MonoBehaviour {

	public VideoPlayer videoPlayer;
	public RawImage raw;


	// Use this for initialization
	void Start () {

		RenderTexture rt = RenderTexture.GetTemporary(256, 256, 0, RenderTextureFormat.ARGB32);

		videoPlayer.targetTexture = rt;
		raw.texture = rt;


		videoPlayer.Play();

	}
	
	


}

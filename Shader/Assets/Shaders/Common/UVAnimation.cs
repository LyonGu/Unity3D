
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UVAnimation : MonoBehaviour {

	public Image imgCom;

	[Range(-1.0f, 1.0f)]
	public float speedX = 0.5f;

    [Range(-1.0f, 1.0f)]
	public float speedY = 0.0f;

	private Material material;
	void Start () {
        material = new Material(Shader.Find ("Common/UVAnimation"));
        material.hideFlags = HideFlags.DontSave;
        imgCom.material = material;
    }

	// Update is called once per frame
	void Update () {
		if(material!=null)
		{
            material.SetFloat("_SpeedX", speedX);
            material.SetFloat("_SpeedY", speedY);
            material.SetColor("_Color", imgCom.color);
		}
	}
}

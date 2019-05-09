
/***
 *
 *  Title: "Guardian" 项目
 *         描述：
 *
 *  Description:
 *        功能：
 *       
 *
 *  Date: 2019
 * 
 *  Version: 1.0
 *
 *  Modify Recorder:
 *     
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PostEffectHelp : MonoBehaviour
{

    public Toggle _toggleHSB;


    private GameObject _mainCamera;
    private BrightnessSaturationAndContrast _BrightnessSaturationAndContrast;
    private Bloom _Bloom;
    
    void Awake()
    { 
        _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        _BrightnessSaturationAndContrast = _mainCamera.GetComponent<BrightnessSaturationAndContrast>();
        _Bloom = _mainCamera.GetComponent<Bloom>();
    }
	// Use this for initialization
	void Start () {
        //_toggleHSB.OnPointerClick()

        //方法1，添加监听
        _toggleHSB.onValueChanged.AddListener((bool isOn) => { toggle_HSB(_toggleHSB, isOn); });

	}
	

    //色相饱和度亮度
    public void toggle_HSB(Toggle toggle, bool isOn)
    {
        //Debug.Log("toggle_HSB========" + isOn);
        _BrightnessSaturationAndContrast.enabled = isOn;
        _Bloom.enabled = isOn;
    }


}

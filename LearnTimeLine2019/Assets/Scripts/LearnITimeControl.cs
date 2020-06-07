using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class LearnITimeControl : MonoBehaviour , ITimeControl
{
    // Start is called before the first frame update

    public Text uitext;

    private string printLabel = "timeline的controltrack运行时每一帧调用,timeline的controltrack运行时每一帧调用,timeline的controltrack运行时每一帧调用,timeline的controltrack运行时每一帧调用";

    public void OnControlTimeStart()
    {
        Debug.Log("OnControlTimeStart======");
        uitext.gameObject.SetActive(true);
    }

    public void OnControlTimeStop()
    {
        Debug.Log("OnControlTimeStop======");
        uitext.gameObject.SetActive(false);
    }


    /// <summary>
    /// timeline的control track运行时没一帧调用
    /// </summary>
    /// <param name="time">当前已经运行的时间</param>
    public void SetTime(double time)
    {
        int len = (int)time;
        if(len <= printLabel.Length) 
            uitext.text = printLabel.Substring(0, len);  
    }

   
}

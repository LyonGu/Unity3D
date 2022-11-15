using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
 

public class TestCallJava : MonoBehaviour
{
    AndroidJavaClass javaClass = null;
    AndroidJavaObject javaObject = null;

    public InputField logInput;
    public InputField nameInput;

    public InputField getLogInput;
    public InputField getNameInput;

    public InputField showMessageInput;
    // Start is called before the first frame update
    void Start()
    {
        javaClass = new AndroidJavaClass("com.example.testunity.Test");

        javaObject = new AndroidJavaObject("com.example.testunity.Test");

        //AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        //AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    }

    public void SetLog() {
        javaClass.CallStatic("SetLOG", logInput.text);
    }

    public void SetLogField() {
        javaClass.SetStatic("LOG", logInput.text);
    }


    public void SetName()
    {
        javaObject.Call("SetName", nameInput.text);
    }

    public void SetNameField()
    {
        javaObject.Set("name", nameInput.text);
    }

    public void GetLog() {

        getLogInput.text = javaClass.CallStatic<string>("GetLOG");
    }

    public void GetLogField() {
        getLogInput.text = javaClass.GetStatic<string>("LOG");
    }

    public void GetName()
    {

        getNameInput.text = javaObject.Call<string>("GetName");
    }

    public void GetNameField()
    {
        getNameInput.text = javaObject.Get<string>("name");
    }

    public void ShowMessage() {
        javaObject.Call("ShowMessage", showMessageInput.text);


        
    }

}

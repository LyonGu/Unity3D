using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class CallObect : MonoBehaviour
{

    public InputField input;
    public Text text;

#if UNITY_IPHONE

    [DllImport("__Internal")]
    static extern void IOSLog(string message);

#endif


    public void OnButtonClick() {

        IOSLog(input.text);
    }

    public void IOSToUnity(string str) {
        text.text = str;
    }

}

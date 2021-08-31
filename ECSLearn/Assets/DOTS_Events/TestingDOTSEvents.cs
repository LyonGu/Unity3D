/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using CodeMonkey;

public class TestingDOTSEvents : MonoBehaviour {

    private void Start() {
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PipeMoveSystem_Done>().OnPipePassed += TestingDOTSEvents_OnPipePassed;
    }

    private void TestingDOTSEvents_OnPipePassed(object sender, System.EventArgs e) {
        Debug.Log("Pipe Event!");
        CMDebug.TextPopup("Ding!", new Vector3(.5f, .2f));
    }

}

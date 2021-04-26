using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CopyVolume : MonoBehaviour
{
    public GameObject mainCameraObj;
    void Start()
    {
        var volume = mainCameraObj.GetComponent<Volume>();

        var volumeT = this.gameObject.GetComponent<Volume>();
        //volumeT = volume;

        //ÕûÌå¸´ÖÆ
        volumeT.isGlobal = volume.isGlobal;
        volumeT.weight = volume.weight;
        volumeT.sharedProfile = volume.sharedProfile;
        volumeT.priority = volume.priority;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

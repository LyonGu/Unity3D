using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
public class StoryControl : MonoBehaviour
{


    public bool isTrigger;
    public PlayableDirector playableDirector;

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Trigger")
        {
            
            isTrigger = true;
        }
    }


    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "Trigger")
        {
            isTrigger = false;
        }
    }

    public void Update()
    {
        if (isTrigger)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                playableDirector.Play();
            }
        }
    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRootFind : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SceneRoot sceneRoot = GameObject.FindObjectOfType<SceneRoot>();
        Debug.Log($"{sceneRoot.gameObject.name}==========");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

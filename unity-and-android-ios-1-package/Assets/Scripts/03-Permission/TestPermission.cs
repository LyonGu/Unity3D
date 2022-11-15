using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class TestPermission : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        
    }

    public void RequestPermission() {

        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

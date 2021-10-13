using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Class2Singleton : Singleton<Class2Singleton>
{
    private static int mIndex = 0;
    private Class2Singleton() { }

    public override void OnSingletonInit()
    {
        mIndex++;
    }
    public void Log(string content)
    {
        Debug.Log("Class2Singleton" + mIndex + ":" + content);
    }
}
public class SingletonExample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Class2Singleton.Instance.Log("Hello World!");

        // delete instance
        Class2Singleton.Instance.Dispose();

        // a differente instance
        Class2Singleton.Instance.Log("Hello World!");
    }

}

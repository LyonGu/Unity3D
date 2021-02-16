using System.Collections;
using System.Collections.Generic;
using UnityEngine;


internal class Class2MonoSingletonProperty : MonoBehaviour, ISingleton
{
    public static Class2MonoSingletonProperty Instance
    {
        get { return MonoSingletonProperty<Class2MonoSingletonProperty>.Instance; }
    }

    public void Dispose()
    {
        MonoSingletonProperty<Class2MonoSingletonProperty>.Dispose();
    }

    public void OnSingletonInit()
    {
        Debug.Log(name + ":" + "OnSingletonInit");
    }

    private void Awake()
    {
        Debug.Log(name + ":" + "Awake");
    }

    private void Start()
    {
        Debug.Log(name + ":" + "Start");
    }

    protected void OnDestroy()
    {
        Debug.Log(name + ":" + "OnDestroy");
    }
}

public class MonoSingletonPropertyExample : MonoBehaviour
{
    // Start is called before the first frame update
    private IEnumerator Start()
    {
        var instance = Class2MonoSingletonProperty.Instance;

        yield return new WaitForSeconds(3.0f);

        instance.Dispose();
    }
}

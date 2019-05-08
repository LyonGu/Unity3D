using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TestObj {

    public string name = "";
    public TestObj(string nn)
    {
        name = nn;
    }

};

public class TestDictionary : MonoBehaviour {


    public GameObject _cube;
	// Use this for initialization
	void Start () {

        //不能一边遍历dictionary，一边删除
        //Dictionary<int, int> dict = new Dictionary<int, int>();
        //dict.Add(1, 1);
        //dict.Add(2, 2);
        //dict.Add(3, 3);
        //dict.Add(4, 4);

        //foreach (KeyValuePair<int, int> kv in dict)
        //{

        //    if (kv.Key == 1 || kv.Key == 2)
        //    {
        //       //dict.Remove(1);
               
        //    }

        //}

        //foreach (KeyValuePair<int, int> kv in dict)
        //{

        //    Debug.Log("kv========"+kv.Key + " / "+kv.Value);

        //}

        //测试1: list和dictionary里存的只是引用 --> ok
        Dictionary<int, TestObj> dict = new Dictionary<int, TestObj>();
        TestObj obj = new TestObj("hxp");
        dict.Add(1, obj);

        List<TestObj> list = new List<TestObj>();
        list.Add(obj);

        obj.name = "hxp22222";

        //测试下，引用的比较是否是比较引用指向的内存 ==>测试ok，是比较指向的内存
        if (dict[1] == list[0])
        {
            int a22 = 100;
        }

        int a = 10;

        //测试2，删除GameObject的时候，是否也会删除对应资源内存(texture,material...) ==>删除gameObject，对应的资源内存不会删除

        Texture tex = Resources.Load<Texture>("Textures/Floor");
        _cube.GetComponent<Renderer>().material.mainTexture = tex;

        //测试3 当list里存在相同的元素时，List.Remove会怎样 ==> 只会删除一个
        List<int> listT = new List<int>();
        listT.Add(2);
        listT.Add(2);
        listT.Add(3);

        listT.Remove(2);

        int o = 10;

        //测试4 list从后往前遍历，一边遍历一边删除 ==>ok

        int len = listT.Count;
        for (int i = len - 1; i >= 0; i--)
        {
            int v = listT[i];
            listT.Remove(v);
        }

        int nn = 10;


        //测试下资源框架

        //


}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.D))
        {
            Destroy(_cube);
        }
	}
}

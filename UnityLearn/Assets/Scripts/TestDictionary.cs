using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDictionary : MonoBehaviour {

	// Use this for initialization
	void Start () {

        Dictionary<int, int> dict = new Dictionary<int, int>();
        dict.Add(1, 1);
        dict.Add(2, 2);
        dict.Add(3, 3);
        dict.Add(4, 4);

        foreach (KeyValuePair<int, int> kv in dict)
        {

            if (kv.Key == 1 || kv.Key == 2)
            {
               //dict.Remove(1);
               
            }

        }

        foreach (KeyValuePair<int, int> kv in dict)
        {

            Debug.Log("kv========"+kv.Key + " / "+kv.Value);

        }

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class TestSignal : MonoBehaviour
{
   public void TestRecieve()
   {
        Debug.Log("TestRecieve============");
   }

     public void TestRecieve1(int id)
   {
        Debug.Log("TestRecieve1============"+id);
   }
}

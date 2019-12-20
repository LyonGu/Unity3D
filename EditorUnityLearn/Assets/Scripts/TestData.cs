//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;


//public interface IProperty : ISerializationCallbackReceiver
//{
//    string name { get; }
//}

//[Serializable]
//public abstract class PropertyInfo<T> : IProperty
//{
//    protected string _name;
//    public string name => _name;
//    public T value;

//    public abstract void OnBeforeSerialize();

//    public abstract void OnAfterDeserialize();
//}
//[Serializable]
//public class IntProperty : ISerializationCallbackReceiver// PropertyInfo<int>
//{
//    public string _n;
//    public int _v;
//    public void OnAfterDeserialize()
//    {

//    }

//    public void OnBeforeSerialize()
//    {


//    }
//}



////[Serializable]
////public class Verctor3Property : PropertyInfo<Vector3>
////{
////    public override void OnAfterDeserialize()
////    {

////    }

////    public override void OnBeforeSerialize()
////    {

////    }
////}


//public enum CompoentType : byte
//{
//    GameObject,
//    Particle,

//}

//[Serializable]
//public class ObjectData
//{
//    public ParticleSystem host;
//    public CompoentType type;
//    public List<IntProperty> iPropertys;
//    //public List<FloatProperty> fPropertys;

//}
////[Serializable]
////public class ParticleSystemData
////{
////    public ParticleSystem comp;

////}



//public class TestData : MonoBehaviour
//{
//    public int age = 1;
//    public string name = "Unity";

//    //public List<ParticleSystemData> ComponentDataList;
//    public ParticleSystem host;
//    public List<ObjectData> bornState;
//    public List<ObjectData> DeadState;
//    //public Dictionary<string, int> testDict = new Dictionary<string, int>();

//    //public List<int> testList = new List<int> { 1, 2, 3 };

//    //public Dictionary<string, int> testDict = new Dictionary<string, int>{
//    //        {"station",1},{"city",2},{"province",3}
//    //    };

//    public void SetState()
//    {
//        //FloatProperty fp = new FloatProperty();
//        //fp.name = "StartDelay";
//        //fp.value = 5f;
//        //ObjectData ob = new ObjectData();
//        //ob.host = ParticleSystem;
//        //ob.propertys.Add(fp);
//        //bornState.Add(ob);
//    }
//}

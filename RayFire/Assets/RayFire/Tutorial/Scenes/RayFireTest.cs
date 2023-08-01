using System;
using System.Collections;
using System.Collections.Generic;
using RayFire;
using UnityEngine;

public class RayFireTest : MonoBehaviour
{
    // Start is called before the first frame update
    private RayfireRigid _rayfirwRigid;

    private Dictionary<int, RayfireRigid> _rayfireRigidDic = new Dictionary<int, RayfireRigid>(32);
    private void Awake()
    {
        _rayfirwRigid = GetComponent<RayfireRigid>();
        AddListeners();
    }

    private void Start()
    {
        int a = 10;
        if (_rayfirwRigid.fragments != null)
        {
            foreach (var rayfireRigid in _rayfirwRigid.fragments)
            {
                int id = rayfireRigid.GetInstanceID();
                if (!_rayfireRigidDic.ContainsKey(id))
                {
                    _rayfireRigidDic.Add(id, rayfireRigid);
                }
            }
        }
    }

    private void AddListeners()
    {
        // if (_rayfirwRigid != null)
        // {
        //     _rayfirwRigid.activationEvent.LocalEvent += MyMethod;
        // }
        
        RFActivationEvent.GlobalEvent += MyMethod;
    }
    
    private void RemoveListeners()
    {
        // if (_rayfirwRigid != null)
        // {
        //     _rayfirwRigid.activationEvent.LocalEvent -=MyMethod;
        // }
        RFActivationEvent.GlobalEvent -= MyMethod;
    }

    private void OnDestroy()
    {
        RemoveListeners();
    }

    private void Update()
    {
        int a = 10;
    }

    void MyMethod(RayfireRigid rigid)
    {
        // if(rigid!=null && rigid.fragments!=null)
        //     Debug.Log($"{Time.frameCount} : {rigid.gameObject.name}================{rigid.fragments.Count}");

        int id = rigid.GetInstanceID();
        if (_rayfireRigidDic.ContainsKey(id))
        {
            Debug.Log($"{Time.frameCount} : {rigid.gameObject.name} 被激活================");
            rigid.gameObject.AddComponent<RayFireCustomeCom>();
        }

       
    }
}

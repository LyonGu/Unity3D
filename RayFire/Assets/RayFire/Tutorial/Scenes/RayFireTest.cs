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
        if (_rayfirwRigid!= null && _rayfirwRigid.fragments != null)
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

        Rigidbody rigidbody = GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            //加限制
            // var addMask = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionY;
            // rigidbody.constraints = rigidbody.constraints | addMask;
            
            //删限制
            var removeMask = ~RigidbodyConstraints.FreezeRotationZ;  //删一个
             removeMask = ~(RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY); //删多个
            rigidbody.constraints = rigidbody.constraints & removeMask;
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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Physics.Raycast(transform.position, transform.forward, out var _hit, 10))
            {
                Debug.Log($"{Time.frameCount} : {_hit.transform.name}================");
            }
        }
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

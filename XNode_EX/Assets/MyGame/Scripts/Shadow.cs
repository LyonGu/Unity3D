using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shadow : MonoBehaviour
{
    public bool isUp = false;
    Transform m_parent;
    Transform parent
    {
        get {
            if (m_parent == null)
            {
                m_parent= transform.parent;
            }
            return m_parent;
        }
    
    }
    public float distance = 1;
    float m_defaultScale = 0;
    float defaultScale
    {
        get {
            if (m_defaultScale == 0)
            {
                m_defaultScale= transform.localScale.x; 
            }
            return m_defaultScale;
        }
    }
    void Start()
    {
    }
    public void ShadowInit()
    {
        
        SetShadowPos();
    }

    void Update()
    {
        if (isUp)
        {
            SetShadowPos();
        }
    }

    void SetShadowPos()
    {
        transform.position = new Vector3(parent.position.x - (distance * 0.5f), parent.position.y - (distance*0.5f),0) ;
        transform.localScale = new Vector3(defaultScale / (distance + 1), defaultScale / (distance + 1), 1);
    }
}

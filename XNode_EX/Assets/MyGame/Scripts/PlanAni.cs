using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanAni : MonoBehaviour
{
    public Enermy enermy;
    void Start()
    {

    }

    void Update()
    {

    }
    public void Fire()
    {
        if (name == "Shadow")
            return;
        enermy.Fire();
    }
    
}

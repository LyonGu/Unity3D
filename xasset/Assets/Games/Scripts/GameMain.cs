using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameMain 
{
    public static void Init(Action initComplete)
    {
        
        initComplete?.Invoke();
    }
}
